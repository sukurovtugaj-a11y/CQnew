using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SecMainCharacter : MonoBehaviour
{
    public enum PlayerState { Grounded, Airborne, Sliding }

    // === Component references ===
    private PlayerMovementComponent movement;
    private PlayerJumpComponent jump;
    private PlayerDashTeleportComponent dash;
    private PlayerSlideComponent slide;
    private PlayerInvulnerabilityComponent invuln;
    private PlayerHealthComponent health;
    private PlayerAchievementComponent achievements;
    private PlayerUpgradeComponent upgrades;
    private PlayerCheckpointComponent checkpoint;
    private PlayerMiscComponent misc;

    // === Inspector fields (unchanged) ===
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public PlayerMenuScript playerMenuScript;

    public bool IsInvulnerable() => health.IsInvulnerable();

    [Header("Dash/Teleport")]
    public float teleportMinDistance = 0f;
    public float teleportMaxDistance = 4f;
    public float dashDistance = 4f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float teleportRange = 3f;
    public float teleportOffset = 1f;
    internal bool canDash = true;
    internal bool isDashing;

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 5f;
    internal bool isInvulnerable;
    internal bool canActivateInvulnerability;

    [Header("Movement")] public float moveSpeed = 5f;
    public float airControl = 0.5f;

    [Header("Jump")] public float jumpHeight = 3f;
    public float coyoteTime = 0.3f;
    public float jumpBufferTime = 0.15f;

    [Header("Double Jump")]
    [Tooltip("Количество дополнительных прыжков в воздухе")]
    public int maxExtraJumps = 0;

    [Header("Upgrades")] public List<JumpAbilityData> activeAbilities = new List<JumpAbilityData>();

    [Header("Advanced")] public float groundStickForce = 2f;

    [Header("Animation")]
    [Tooltip("Дочерний объект с IDLE анимацией")]
    public GameObject idleAnimation;
    [Tooltip("Дочерний объект с RUN анимацией")]
    public GameObject runAnimation;
    [Tooltip("GameObject с SLIDE спрайтом (горизонтальный лежачий)")]
    public GameObject slideSprite;

    [Header("Slide")]
    public float normalSlideDuration = 1f;
    [Tooltip("Скорость скольжения")]
    public float slideSpeed = 5f;
    [Tooltip("Смещение спрайта вниз при подкате")]
    public float slideSpriteOffset = 0.5f;
    internal float normalColliderHeight;
    internal bool isSliding;
    internal int noSlideCount;
    internal float noSlideCooldown;
    public bool CanSlide => noSlideCount <= 0 && noSlideCooldown <= 0;
    internal string slideType;
    internal Vector2 slideDirection;
    internal float slideTimer;
    internal float zoneSlideSpeed;
    internal SlideZone currentSlideZone;

    internal float controlLockTimer;
    internal float savedSlideSpeed;
    internal float savedScaleX;
    internal bool slopeLimitDir;
    internal bool slopeSlideRight;
    internal bool slopeSlideLeft;
    internal float slopeAngle;

    public bool IsSliding => isSliding;
    public bool GetColliderEnabled() => playerCollider.enabled;

    public KeyControll keys;
    public SceneController SC;
    public GameObject MenuPanel;
    public CameraController cameraController;
    [Header("Camera")]
    public GameObject cameraPrefab;

    [Header("Тексты достижений")]
    [Tooltip("Текст при получении здоровья (train)")]
    public GameObject achievementHealth;
    [Tooltip("Текст при получении урона (train2)")]
    public GameObject achievementUpDamage;
    [Tooltip("Текст при получении двойного прыжка (firstLevel)")]
    public GameObject achievementDoubleJump;
    [Tooltip("Текст при получении рывка (firstLevel2)")]
    public GameObject achievementDash;
    [Tooltip("Текст при получении чекпоинта (secondLevel)")]
    public GameObject achievementCheckpoint;
    [Tooltip("Текст при получении неуязвимости (secondLevel2)")]
    public GameObject achievementInvincible;

    internal Rigidbody2D rb;
    internal SpriteRenderer spriteRenderer;
    internal SpriteRenderer runSpriteRenderer;
    internal Animator animator;
    internal Collider2D playerCollider;

    internal PlayerState currentState;
    internal int extraJumpsLeft;
    internal float jumpSpeed;
    internal float baseJumpHeight;
    internal float coyoteTimeCounter;
    internal float jumpBufferCounter;
    internal float horizontalInput;
    internal bool isBoosting;

    internal MonoBehaviour currentPlatform;
    internal bool isParentMode = false;

    internal ContactFilter2D groundFilter;
    internal Collider2D[] groundContacts = new Collider2D[5];

    internal bool inAIZone;
    internal bool firstMoveMade;
    internal bool checkpointEnabled;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        playerCollider = GetComponent<Collider2D>();
        normalColliderHeight = playerCollider.bounds.size.y;

        if (runAnimation != null)
            runSpriteRenderer = runAnimation.GetComponent<SpriteRenderer>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.freezeRotation = true;

        baseJumpHeight = jumpHeight;
        int initialMaxExtraJumps = maxExtraJumps;

        // Init components
        movement = new PlayerMovementComponent(this, rb);
        jump = new PlayerJumpComponent(this, rb, movement);
        dash = new PlayerDashTeleportComponent(this, rb);
        slide = new PlayerSlideComponent(this, rb);
        invuln = new PlayerInvulnerabilityComponent(this);
        health = new PlayerHealthComponent(this, rb);
        achievements = new PlayerAchievementComponent(this);
        upgrades = new PlayerUpgradeComponent(this);
        checkpoint = new PlayerCheckpointComponent(this);
        misc = new PlayerMiscComponent(this);

        jump.RecalculateJumpParameters();
        if (activeAbilities.Count == 0) { maxExtraJumps = initialMaxExtraJumps; extraJumpsLeft = maxExtraJumps; }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public void OnStart(SceneController sc)
    {
        SC = sc;
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainScene")
        {
            currentHealth = maxHealth;
            SpawnPointManager.LastHealth = maxHealth;
            ManualCheckpoint.Clear();
        }
        else
        {
            currentHealth = Mathf.Clamp(SpawnPointManager.LastHealth, 1, maxHealth);
        }

        misc.CreatePlayerCamera();
        currentState = PlayerState.Grounded;
        extraJumpsLeft = maxExtraJumps;
        Cursor.visible = false;

        if (idleAnimation != null) idleAnimation.SetActive(true);
        if (runAnimation != null) runAnimation.SetActive(false);

        upgrades.ApplyUpgrades();
        this.enabled = true;

        var spm = SpawnPointManager.Instance;
        if (spm != null && spm.lockControls)
            StartCoroutine(spm.ApplyControlLock(this));

        if (currentScene == "MainScene")
            StartCoroutine(achievements.CheckAchievementsOnHubSpawn());
    }

    private void Start()
    {
        if (VideoController.introJustPlayed)
        {
            VideoController.introJustPlayed = false;
            VideoController.spawnAtIntroZone = false;
            var introZone = GameObject.Find("IntroZone");
            if (introZone != null)
            {
                transform.position = introZone.transform.position;
                controlLockTimer = 2f;
                var mover = FindObjectOfType<OPollMover>();
                if (mover != null) mover.StartMoving();
            }
        }

        if (extraJumpsLeft <= 0 && maxExtraJumps > 0) extraJumpsLeft = maxExtraJumps;
    }

    private void Update()
    {
        if (controlLockTimer > 0f) controlLockTimer -= Time.deltaTime;
        if (!MenuPanel.activeSelf && Time.timeScale > 0)
        {
            movement.UpdateInput();
            jump.UpdateInput();
            if (controlLockTimer <= 0f && !isSliding && movement.IsGrounded() && Input.GetKeyDown(keys.MoveDown))
            {
                slide.StartSlide("Normal", normalSlideDuration, Vector2.zero);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) misc.ToggleMenu();
        if (Input.GetKeyDown(KeyCode.W)) dash.TryDashOrTeleport();
        if (Input.GetKeyDown(KeyCode.F5)) checkpoint.TrySetCheckpoint();
        invuln.UpdateInput();
    }

    public void LockControls(float duration = 1.5f)
    {
        controlLockTimer = Mathf.Max(controlLockTimer, duration);
    }
    private void FixedUpdate()
    {
        if (MenuPanel.activeSelf || Time.timeScale <= 0) return;
        if (isSliding) { slide.HandleSlide(); return; }
        movement.FixedUpdate();
        jump.HandleJumpRequests();
    }

    // === Wrappers for external scripts ===
    public void Die() => health.Die();
    public void RespawnImmediately() => health.RespawnAtCheckpoint();

    public void Damage(float dam, Transform respawnPoint = null) => health.Damage(dam, respawnPoint);
    public void ResetAfterRespawn() => health.ResetAfterRespawn();

    public void StartSlide(string type, float duration, Vector2 direction, float speed = -1f, bool limitDir = false, bool sRight = true, bool sLeft = true, float angle = 0f)
        => slide.StartSlide(type, duration, direction, speed, limitDir, sRight, sLeft, angle);

    public void StopSlide() => slide.StopSlide();
    public void EnterSlideZone(float speed, SlideZone zone) => slide.EnterSlideZone(speed, zone);
    public void SwitchToSpecialSlide() => slide.SwitchToSpecialSlide();
    public void ExitSlideZone(SlideZone zone) => slide.ExitSlideZone(zone);

    public void AddJumpAbility(JumpAbilityData ability) => jump.AddJumpAbility(ability);
    public void RemoveJumpAbility(JumpAbilityData ability) => jump.RemoveJumpAbility(ability);

    public void ShowAchievement(string upgradeKey) => achievements.ShowAchievement(upgradeKey);

    // === AI Zone ===
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger && HasAIController(other)) misc.OnAINotifyEnter();
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger && HasAIController(other)) misc.OnAINotifyExit();
    }
    private bool HasAIController(Collider2D other) =>
        other.GetComponent<AIcontroller>() != null ||
        other.GetComponentInParent<AIcontroller>() != null ||
        other.GetComponentInChildren<AIcontroller>() != null;
}