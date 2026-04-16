using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCharacter : MonoBehaviour
{
    private enum PlayerState { Grounded, Airborne, Sliding }

    // === Здоровье ===
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public PlayerMenuScript playerMenuScript; // Ссылка на меню

    public bool IsInvulnerable() => isInvulnerable;

    // === Телепорт (F) ===
    [Header("Dash/Teleport")]
    public float teleportMinDistance = 0f;  // Мин. расстояние для телепорта
    public float teleportMaxDistance = 4f;
    public float dashDistance = 4f;     // <-- Расстояние рывка в метрах
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float teleportRange = 3f;
    public float teleportOffset = 1f;
    private bool canDash = true;
    private bool isDashing;

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 5f; // Длительность (настраивается в Инспекторе)
    private bool isInvulnerable;

    // Ссылки
    public KeyControll keys;
    public SceneController SC;
    public GameObject MenuPanel;
    public CameraController cameraController;
    [Header("Camera")]
    public GameObject cameraPrefab;

    // Параметры
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

    [Header("Slide")]
    public float normalSlideDuration = 1f;
    public float slideSpeed = 5f;
    private float normalColliderHeight;
    private bool isSliding;
    private string slideType;
    private Vector2 slideDirection;
    private float slideTimer;
    private float zoneSlideSpeed;
    private SlideZone currentSlideZone;

    public bool IsSliding => isSliding;
    public bool GetColliderEnabled() => playerCollider.enabled;


    // Компоненты
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D playerCollider;

    // Переменные состояния
    private PlayerState currentState;
    private int extraJumpsLeft;
    private float jumpSpeed;
    private float baseJumpHeight;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float horizontalInput;
    private bool isBoosting;

    // Платформы
    private MonoBehaviour currentPlatform;
    private bool isParentMode = false;

    // Фильтр для детекции земли
    private ContactFilter2D groundFilter;
    private Collider2D[] groundContacts = new Collider2D[5];

    // === ОБНОВЛЁН: Урон и смерть (вместо пустого метода) ===
    public void Damage(float dam, Transform respawnPoint = null)
    {
        if (isInvulnerable) return;

        currentHealth -= (int)dam;
        Debug.Log($"[MainCharacter] Урон получен! Текущее HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            rb.velocity = Vector2.zero;
        }
    }

    private void Die()
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        SpawnPointManager.LastHealth = currentHealth;

        if (TryCheckpointRespawn()) return;

        if (playerMenuScript != null)
            playerMenuScript.ShowDeathPanel();
        else
            Debug.LogError("[MainCharacter] ОШИБКА! Ссылка playerMenuScript НЕ назначена в Инспекторе!");
    }

    private bool TryCheckpointRespawn()
    {
        if (!ManualCheckpoint.Has) return false;

        currentHealth = 50;

        if (SceneManager.GetActiveScene().name == ManualCheckpoint.SceneName)
        {
            transform.position = ManualCheckpoint.Position;
        }
        else
        {
            var sp = SpawnPointManager.Instance;
            transform.position = sp != null ? sp.spawnPosition : Vector3.zero;
        }

        ManualCheckpoint.Consume();
        rb.isKinematic = false;
        Debug.Log("[MainCharacter] Респаун по чекпоинту: 50 HP");
        return true;
    }

    // === НОВОЕ: Сброс состояния после респауна ===
    public void ResetAfterRespawn()
    {
        rb.isKinematic = false;
        currentHealth = maxHealth;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        normalColliderHeight = playerCollider.bounds.size.y;

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.freezeRotation = true;

        SetupGroundFilter();
        baseJumpHeight = jumpHeight;

        int initialMaxExtraJumps = maxExtraJumps;
        RecalculateJumpParameters();

        if (activeAbilities.Count == 0)
        {
            maxExtraJumps = initialMaxExtraJumps;
            extraJumpsLeft = maxExtraJumps;
        }

        //currentHealth = maxHealth;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void SetupGroundFilter()
    {
        groundFilter = new ContactFilter2D();
        int groundLayer = LayerMask.NameToLayer("Ground");
        int platformLayer = LayerMask.NameToLayer("Platform");
        int mask = (platformLayer != -1) ? (1 << groundLayer) | (1 << platformLayer) : (1 << groundLayer);
        groundFilter.SetLayerMask(mask);
        groundFilter.useLayerMask = true;
        groundFilter.useNormalAngle = true;
        groundFilter.minNormalAngle = 45f;
        groundFilter.maxNormalAngle = 135f;
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

        Debug.Log($"[MainCharacter] Здоровье при старте: {currentHealth} (сцена: {currentScene})");

        ApplyUpgrades();
        CreatePlayerCamera();
        currentState = PlayerState.Grounded;
        extraJumpsLeft = maxExtraJumps;
        Cursor.visible = false;
        this.enabled = true;
    }

    private void CreatePlayerCamera()
    {
        GameObject cameraObject = new GameObject("PlayerCamera");
        cameraObject.AddComponent<Camera>();
        cameraController = cameraObject.AddComponent<CameraController>();
        cameraController.target = transform;
        cameraObject.transform.position = transform.position + new Vector3(0, 1, -10);

        //if (cameraPrefab != null)
        //{
        //    // Спавним префаб
        //    GameObject cameraInstance = Instantiate(cameraPrefab, transform.position + new Vector3(0, 1, -10), Quaternion.identity);
        //    cameraController = cameraInstance.GetComponent<CameraController>();

        //    if (cameraController == null)
        //    {
        //        cameraController = cameraInstance.AddComponent<CameraController>();
        //    }

        //    cameraController.target = transform;
        //}
        //else
        //{
        //    // Резервный вариант: создаём камеру программно
        //    GameObject cameraObject = new GameObject("PlayerCamera");
        //    Camera cam = cameraObject.AddComponent<Camera>();

        //    // 🔥 Настройки камеры
        //    cam.orthographic = true;
        //    cam.orthographicSize = 5;
        //    cam.nearClipPlane = 0.3f;
        //    cam.farClipPlane = 1000f;
        //    cam.clearFlags = CameraClearFlags.SolidColor;
        //    cam.backgroundColor = new Color(0, 0, 0, 1);

        //    cameraController = cameraObject.AddComponent<CameraController>();
        //    cameraController.target = transform;
        //    cameraObject.transform.position = transform.position + new Vector3(0, 1, -10);
        //}

    }

    private void Start()
    {
        if (extraJumpsLeft <= 0 && maxExtraJumps > 0) extraJumpsLeft = maxExtraJumps;
    }

    private void Update()
    {
        if (!MenuPanel.activeSelf && Time.timeScale > 0) HandleInput();
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleMenu();
        if (Input.GetKeyDown(KeyCode.W)) TryDashOrTeleport();
        if (Input.GetKeyDown(KeyCode.F5)) TrySetCheckpoint();

        if (keys != null && Input.GetKeyDown(keys.InvulnerabilityButton) && !isInvulnerable)
            StartCoroutine(InvulnerabilityRoutine());
    }

    private void FixedUpdate()
    {
        if (MenuPanel.activeSelf || Time.timeScale <= 0) return;

        if (isSliding) { HandleSlide(); return; }

        bool isGrounded = IsGrounded();
        UpdateTimers(isGrounded);
        UpdateState(isGrounded);
        ApplyMovement();

        if (currentPlatform == null && isGrounded && Mathf.Abs(horizontalInput) > 0.01f)
        {
            rb.AddForce(Vector2.down * groundStickForce, ForceMode2D.Force);
        }

        HandleJumpRequests();
        UpdateAnimations();
        CheckMovingPlatform(isGrounded);
    }

    private bool firstMoveMade;

    private void HandleInput()
    {
        if (isSliding) { horizontalInput = 0; return; }

        horizontalInput = Input.GetKey(keys.MoveFront) ? 1 : Input.GetKey(keys.MoveBack) ? -1 : 0;
        if (horizontalInput != 0) spriteRenderer.flipX = horizontalInput > 0;
        isBoosting = Input.GetKey(keys.boost);
        if (Input.GetKeyDown(keys.JumpButton)) jumpBufferCounter = jumpBufferTime;

        if (!isSliding && IsGrounded() && Input.GetKeyDown(keys.MoveDown))
        {
            if (currentSlideZone != null)
                StartSlide("Special", 0, Vector2.zero);
            else
                StartSlide("Normal", normalSlideDuration, Vector2.zero);
        }

        if (!firstMoveMade && (horizontalInput != 0 || Input.GetKey(keys.JumpButton)))
        {
            firstMoveMade = true;
            if (GameProgressManager.Instance != null) GameProgressManager.Instance.OnFirstMove();
        }
    }

    private void ToggleMenu()
    {
        bool menuActive = !MenuPanel.activeSelf;
        MenuPanel.SetActive(menuActive);
        Cursor.visible = menuActive;
        Time.timeScale = menuActive ? 0f : 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private bool IsGrounded() => rb.GetContacts(groundFilter, groundContacts) > 0;

    private void UpdateTimers(bool isGrounded)
    {
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.fixedDeltaTime;
        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;
    }

    private void UpdateState(bool isGrounded)
    {
        PlayerState previousState = currentState;
        if (currentState == PlayerState.Grounded && !isGrounded) currentState = PlayerState.Airborne;
        if (currentState == PlayerState.Airborne && isGrounded)
        {
            currentState = PlayerState.Grounded;
            extraJumpsLeft = maxExtraJumps;
        }
    }

    private void ApplyMovement()
    {
        if (isDashing) return;

        float speed = moveSpeed * (isBoosting ? 2f : 1f);
        float control = 1f;

        if (currentState == PlayerState.Airborne)
        {
            speed *= airControl;
            control = airControl;
        }

        if (currentPlatform != null && isParentMode)
        {
            rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
        }
        else
        {
            Vector2 targetVelocity = new Vector2(horizontalInput * speed, rb.velocity.y);
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, control);
        }
    }

    private void HandleJumpRequests()
    {
        if (jumpBufferCounter <= 0) return;
        bool isGrounded = IsGrounded();
        bool canGroundJump = (isGrounded || coyoteTimeCounter > 0);
        bool canAirJump = currentState == PlayerState.Airborne && extraJumpsLeft > 0;

        if (canGroundJump) { ExecuteGroundJump(); jumpBufferCounter = 0; }
        else if (canAirJump) { ExecuteAirJump(); jumpBufferCounter = 0; }
    }

    private void ExecuteGroundJump()
    {
        DetachFromPlatform();
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        currentState = PlayerState.Airborne;
        extraJumpsLeft = maxExtraJumps;
        coyoteTimeCounter = 0;
    }

    private void ExecuteAirJump()
    {
        DetachFromPlatform();
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        extraJumpsLeft--;
        coyoteTimeCounter = 0;
    }

    // === ПЛАТФОРМЫ — ПРОСТАЯ ЛОГИКА ===

    private void CheckMovingPlatform(bool isGrounded)
    {
        if (!isGrounded) { DetachFromPlatform(); return; }

        if (currentPlatform != null)
        {
            if (!IsActuallyOnTop(currentPlatform.GetComponent<Collider2D>()))
                DetachFromPlatform();
            else
            {
                // Для вертикальной платформы — добавляем её скорость к игроку
                if (!isParentMode && currentPlatform is MovingPlatform_Vertical vert)
                {
                    rb.velocity += vert.PlatformVelocity;
                }
            }
            return;
        }

        for (int i = 0; i < groundContacts.Length; i++)
        {
            if (groundContacts[i] == null) continue;

            // Пробуем горизонтальную (SetParent)
            var horiz = groundContacts[i].GetComponent<MovingPlatform_Horizontal>();
            if (horiz != null && IsActuallyOnTop(groundContacts[i]))
            {
                AttachToHorizontal(horiz);
                return;
            }

            // Пробуем вертикальную (Kinematic)
            var vert = groundContacts[i].GetComponent<MovingPlatform_Vertical>();
            if (vert != null && IsActuallyOnTop(groundContacts[i]))
            {
                AttachToVertical(vert);
                return;
            }
        }
    }

    private bool IsActuallyOnTop(Collider2D platCollider)
    {
        float playerFeetY = transform.position.y - playerCollider.bounds.extents.y;
        float platformTopY = platCollider.bounds.max.y;
        return playerFeetY <= platformTopY + 0.15f && playerFeetY >= platformTopY - 0.1f;
    }

    private void AttachToHorizontal(MovingPlatform_Horizontal platform)
    {
        currentPlatform = platform;
        isParentMode = true;
        transform.SetParent(platform.transform);
    }

    private void AttachToVertical(MovingPlatform_Vertical platform)
    {
        currentPlatform = platform;
        isParentMode = false;
    }

    private void DetachFromPlatform()
    {
        if (currentPlatform != null)
        {
            if (isParentMode) transform.SetParent(null);
            currentPlatform = null;
            isParentMode = false;
        }
    }

    // ===============================================

    private void UpdateAnimations()
    {
        if (MenuPanel.activeSelf || Time.timeScale <= 0) return;
        animator.SetBool("Moving", Mathf.Abs(horizontalInput) > 0.1f);
        animator.SetBool("IsGrounded", currentState == PlayerState.Grounded);
    }

    public void AddJumpAbility(JumpAbilityData ability) { activeAbilities.Add(ability); RecalculateJumpParameters(); }
    public void RemoveJumpAbility(JumpAbilityData ability) { activeAbilities.Remove(ability); RecalculateJumpParameters(); }

    private void RecalculateJumpParameters()
    {
        int originalMaxExtraJumps = maxExtraJumps;
        maxExtraJumps = 0;
        jumpHeight = baseJumpHeight;
        foreach (var ability in activeAbilities)
        {
            maxExtraJumps += ability.additionalJumps;
            jumpHeight = Mathf.Max(jumpHeight, baseJumpHeight * ability.heightMultiplier);
        }
        if (activeAbilities.Count == 0) maxExtraJumps = originalMaxExtraJumps;
        float gravity = Physics2D.gravity.y * rb.gravityScale;
        jumpSpeed = Mathf.Sqrt(-2f * gravity * jumpHeight);
        if (IsGrounded()) extraJumpsLeft = maxExtraJumps;
    }

    private void ApplyUpgrades()
    {
        var gpm = GameProgressManager.Instance;
        if (gpm == null) return;

        // Train
        string train = gpm.GetUpgrade("train", false);
        if (train == "health") { maxHealth = 125; currentHealth = 125; }

        // First Level
        string first = gpm.GetUpgrade("firstLevel", false);
        if (first == "doubleJump") { maxExtraJumps = 1; extraJumpsLeft = 1; }
        if (first == "dash") { canDash = true; } else { canDash = false; }

        // Second Level
        string second = gpm.GetUpgrade("secondLevel", false);
        if (second == "checkpoint") { checkpointEnabled = true; } else { checkpointEnabled = false; }
        if (second == "invincible") { isInvulnerable = true; } else { isInvulnerable = false; }

        // === Второй выбор (достижения за повторное прохождение) ===
        string train2 = gpm.GetUpgrade("train", true);
        if (train2 == "health") { maxHealth = 125; currentHealth = 125; }

        string first2 = gpm.GetUpgrade("firstLevel", true);
        if (first2 == "doubleJump" && maxExtraJumps < 1) { maxExtraJumps = 1; extraJumpsLeft = 1; }
        if (first2 == "dash") { canDash = true; }

        string second2 = gpm.GetUpgrade("secondLevel", true);
        if (second2 == "checkpoint") { checkpointEnabled = true; }
        if (second2 == "invincible") { isInvulnerable = true; }
    }

    private bool checkpointEnabled;
    private bool inAIZone;

    private void TrySetCheckpoint()
    {
        if (!checkpointEnabled) return;
        if (MenuPanel.activeSelf || Time.timeScale <= 0 || ManualCheckpoint.Has || ManualCheckpoint.Used) return;
        if (SceneManager.GetActiveScene().name == "MainScene" || !IsGrounded()) return;

        if (inAIZone)
        {
            Debug.Log("[MainCharacter] Нельзя: внутри зоны AI");
            return;
        }

        ManualCheckpoint.Set(transform.position, SceneManager.GetActiveScene().name);
        Debug.Log("[MainCharacter] Чекпоинт установлен");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger &&
            (other.GetComponent<AIcontroller>() != null ||
             other.GetComponentInParent<AIcontroller>() != null ||
             other.GetComponentInChildren<AIcontroller>() != null))
        {
            inAIZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger &&
            (other.GetComponent<AIcontroller>() != null ||
             other.GetComponentInParent<AIcontroller>() != null ||
             other.GetComponentInChildren<AIcontroller>() != null))
        {
            inAIZone = false;
        }
    }

    private void HandleSlide()
    {
        if (slideType == "Slope")
        {
            rb.velocity = new Vector2(slideDirection.x * slideSpeed, slideDirection.y * slideSpeed);
            if (Input.GetKeyDown(keys.JumpButton)) { StopSlide(); return; }
            if (!IsGrounded()) StopSlide();
            return;
        }

        if (slideType == "Special")
        {
            float sDir = spriteRenderer.flipX ? -1f : 1f;
            rb.velocity = new Vector2(sDir * zoneSlideSpeed, rb.velocity.y);

            if (currentSlideZone != null)
            {
                var zc = currentSlideZone.GetComponent<Collider2D>();
                if (zc != null && !zc.OverlapPoint(transform.position))
                    StopSlide();
            }
            return;
        }

        slideTimer -= Time.fixedDeltaTime;
        float dir = spriteRenderer.flipX ? -1f : 1f;
        rb.velocity = new Vector2(dir * slideSpeed, rb.velocity.y);

        if (slideType == "Normal" && Input.GetKeyDown(keys.JumpButton)) { StopSlide(); return; }
        if (slideTimer <= 0) StopSlide();
    }

    public void StartSlide(string type, float duration, Vector2 direction, float speed = -1f)
    {
        if (isSliding) return;
        isSliding = true;
        slideType = type;
        slideTimer = duration;
        slideDirection = direction;
        if (type == "Slope" && speed > 0) slideSpeed = speed;
        if (type == "Special") slideSpeed = zoneSlideSpeed;
        currentState = PlayerState.Sliding;
        playerCollider.enabled = false;
    }

    public void StopSlide()
    {
        if (!isSliding) return;
        isSliding = false;
        playerCollider.enabled = true;
        if (IsGrounded()) currentState = PlayerState.Grounded;
    }

    public void EnterSlideZone(float speed, SlideZone zone)
    {
        zoneSlideSpeed = speed;
        currentSlideZone = zone;
    }

    public void ExitSlideZone(SlideZone zone)
    {
        if (currentSlideZone == zone)
        {
            if (isSliding && !playerCollider.enabled) return;
            currentSlideZone = null;
            if (isSliding && slideType == "Special") StopSlide();
        }
    }

    private void TryDashOrTeleport()
    {
        if (isSliding || isDashing || !canDash) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, teleportRange);
        foreach (var hit in hits)
        {
            // Телепорт ТОЛЬКО если объект с тегом И игрок сбоку И смотрит на объект
            if (hit.CompareTag("TeleportObj") && IsPlayerOnSide(hit) && IsPlayerFacingObject(hit.bounds))
            {
                Teleport(hit.bounds);
                return;
            }
        }
        // Во всех остальных случаях — обычный рывок
        StartCoroutine(DoDash());
    }

    // Проверяет, смотрит ли игрок на объект
    private bool IsPlayerFacingObject(Bounds objBounds)
    {
        // Направление взгляда игрока (flipX: true = смотрит вправо, false = влево)
        bool facingRight = spriteRenderer.flipX;
        
        // Позиция центра объекта
        float objCenterX = objBounds.center.x;
        
        // Игрок смотрит на объект, если:
        // - смотрит вправо И объект справа
        // - смотрит влево И объект слева
        if (facingRight)
            return objCenterX > transform.position.x; // Объект справа
        else
            return objCenterX < transform.position.x; // Объект слева
    }

    // Вычисляет расстояние от игрока до ближайшей границы объекта по X
    private float GetDistanceToEdge(Bounds objBounds)
    {
        if (transform.position.x < objBounds.min.x)
            return objBounds.min.x - transform.position.x;
        else if (transform.position.x > objBounds.max.x)
            return transform.position.x - objBounds.max.x;
        
        return 0f; // Игрок внутри объекта по X
    }

    private bool IsPlayerOnSide(Collider2D obj)
    {
        Bounds b = obj.bounds;
        // Игрок сбоку по X и в пределах высоты объекта
        return (transform.position.x < b.min.x || transform.position.x > b.max.x) &&
               (transform.position.y >= b.min.y && transform.position.y <= b.max.y);
    }

    private void Teleport(Bounds objBounds)
    {
        float newX = transform.position.x < objBounds.min.x
            ? objBounds.max.x + teleportOffset
            : objBounds.min.x - teleportOffset;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private System.Collections.IEnumerator DoDash()
    {
        isDashing = true;
        canDash = false;

        float dir = spriteRenderer.flipX ? 1f : -1f;
        Vector2 startPos = rb.position;
        Vector2 direction = new Vector2(dir, 0);

        // Проверяем, есть ли стена на пути (игнорируем землю под ногами)
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, dashDistance, 
                                              LayerMask.GetMask("Ground", "Platform"));

        float actualDistance = dashDistance;
        if (hit.collider != null && hit.distance > 0.1f)
        {
            // Если стена есть — останавливаемся чуть перед ней
            actualDistance = hit.distance - 0.1f;
            if (actualDistance < 0.1f) actualDistance = 0.1f; // Минимальный рывок
        }

        Vector2 targetPos = startPos + direction * actualDistance;
        float elapsed = 0f;
        float savedYVel = rb.velocity.y;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, elapsed / dashDuration));
            rb.velocity = new Vector2(0, savedYVel);
            yield return null;
        }

        rb.MovePosition(targetPos);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private System.Collections.IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        // Опционально: здесь можно добавить визуальный эффект (мигание, частицы)

        yield return new WaitForSeconds(invulnerabilityDuration);

        isInvulnerable = false;
    }
}

[CreateAssetMenu(fileName = "NewJumpAbility", menuName = "Abilities/JumpAbility")]
public class JumpAbilityData : ScriptableObject
{
    public string abilityName;
    public int additionalJumps;
    public float heightMultiplier = 1f;
}