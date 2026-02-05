using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    private enum PlayerState { Grounded, Airborne }

    // Ссылки
    public KeyControll keys;
    public SceneController SC;
    public GameObject MenuPanel;
    public CameraController cameraController;

    // Параметры
    [Header("Movement")] public float moveSpeed = 5f;
    public float airControl = 0.5f;

    [Header("Jump")] public float jumpHeight = 3f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;

    [Header("Double Jump")]
    [Tooltip("Количество дополнительных прыжков в воздухе. 1 = двойной прыжок, 2 = тройной и т.д.")]
    public int maxExtraJumps = 5; // Теперь будет работать с запуском игры!

    [Header("Slide")]
    public float slideDuration = 0.4f;
    public float slideSpeedMultiplier = 1.8f;

    [Header("Upgrades")] public List<JumpAbilityData> activeAbilities = new List<JumpAbilityData>();

    // Компоненты
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Переменные состояния
    private PlayerState currentState;
    private int extraJumpsLeft;
    private float jumpSpeed;
    private float baseJumpHeight;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float horizontalInput;
    private bool jumpRequested;
    private bool isBoosting;

    // === СКОЛЬЖЕНИЕ ПО ЗЕМЛЕ ===
    private bool isSliding = false;
    private float slideTimer = 0f;
    // =========================

    // Фильтр для детекции земли
    private ContactFilter2D groundFilter;
    private Collider2D[] groundContacts = new Collider2D[1];
    private CapsuleCollider2D mainCollider;
    private BoxCollider2D slideCollider;

    public GameObject PlayerCamera;
    private void Awake()
    {
        mainCollider = GetComponent<CapsuleCollider2D>();
        slideCollider = transform.Find("SlideHitbox")?.GetComponent<BoxCollider2D>();

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        SetupGroundFilter();
        baseJumpHeight = jumpHeight;

        // ИНИЦИАЛИЗАЦИЯ ПРЫЖКОВ ДО ВЫЗОВА RecalculateJumpParameters
        int initialMaxExtraJumps = maxExtraJumps;

        RecalculateJumpParameters();

        // ВОССТАНОВЛЕНИЕ НАЧАЛЬНОГО ЗНАЧЕНИЯ ПОСЛЕ РАСЧЕТОВ
        if (activeAbilities.Count == 0) // Если нет улучшений из системы прокачки
        {
            maxExtraJumps = initialMaxExtraJumps;
            extraJumpsLeft = maxExtraJumps;
        }

        // Восстанавливаем нормальное время при запуске
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        Debug.Log($"[MainCharacter] Инициализация завершена. Доп. прыжков: {maxExtraJumps}, Осталось: {extraJumpsLeft}");
    }

    private void SetupGroundFilter()
    {
        groundFilter = new ContactFilter2D();
        groundFilter.SetLayerMask(LayerMask.GetMask("Ground"));
        groundFilter.useLayerMask = true;
        groundFilter.useNormalAngle = true;
        groundFilter.minNormalAngle = 70f;
        groundFilter.maxNormalAngle = 110f;
    }

    public void OnStart(SceneController sc)
    {
        SC = sc;
        CreatePlayerCamera();
        currentState = PlayerState.Grounded;

        // ДОПОЛНИТЕЛЬНАЯ ИНИЦИАЛИЗАЦИЯ ДЛЯ НАДЕЖНОСТИ
        extraJumpsLeft = maxExtraJumps;
        Debug.Log($"[MainCharacter] OnStart вызван. Доп. прыжков: {maxExtraJumps}");

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
    }
    private void Start()
    {
        // ФИНАЛЬНАЯ ПРОВЕРКА И ИНИЦИАЛИЗАЦИЯ НА СТАРТЕ
        if (extraJumpsLeft <= 0 && maxExtraJumps > 0)
        {
            extraJumpsLeft = maxExtraJumps;
            Debug.Log($"[MainCharacter] Start: Восстановлены прыжки до {extraJumpsLeft}");
        }
    }

    private void Update()
    {
        // Обработка ввода только когда игра не на паузе
        if (!MenuPanel.activeSelf && Time.timeScale > 0)
        {
            HandleInput();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }
    }



    private void FixedUpdate()
    {
        // Физика работает только когда игра не на паузе
        if (MenuPanel.activeSelf || Time.timeScale <= 0) return;

        bool isGrounded = IsGrounded();
        UpdateTimers(isGrounded);
        UpdateState(isGrounded);

        // === УПРАВЛЕНИЕ ТАЙМЕРОМ СКОЛЬЖЕНИЯ ===
        if (isSliding)
        {
            slideTimer -= Time.fixedDeltaTime;
            if (slideTimer <= 0f || !isGrounded)
            {
                isSliding = false;
            }
        }
        // ====================================

        if (isSliding)
        {
            mainCollider.enabled = false;
            if (slideCollider) slideCollider.enabled = true;
        }
        else
        {
            mainCollider.enabled = true;
            if (slideCollider) slideCollider.enabled = false;
        }

        ApplyMovement();
        HandleJumpRequests();
        UpdateAnimations();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetKey(keys.MoveFront) ? 1 : Input.GetKey(keys.MoveBack) ? -1 : 0;

        if (horizontalInput != 0)
        {
            spriteRenderer.flipX = horizontalInput > 0;
        }

        isBoosting = Input.GetKey(keys.boost);

        if (Input.GetKeyDown(keys.JumpButton))
        {
            jumpRequested = true;
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // === АКТИВАЦИЯ СКОЛЬЖЕНИЯ НА КЛАВИШУ "S" (MoveDown) ===
        if (Input.GetKeyDown(keys.MoveDown) && currentState == PlayerState.Grounded && !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
            // Фиксируем направление скольжения (горизонтальное!)
            horizontalInput = horizontalInput != 0 ? horizontalInput : (spriteRenderer.flipX ? 1 : -1);
        }
        // ======================================================
    }

    private void ToggleMenu()
    {
        bool menuActive = !MenuPanel.activeSelf;
        MenuPanel.SetActive(menuActive);
        Cursor.visible = menuActive;

        if (menuActive)
        {
            Time.timeScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private bool IsGrounded() => rb.GetContacts(groundFilter, groundContacts) > 0;

    private void UpdateTimers(bool isGrounded)
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }
    }

    private void UpdateState(bool isGrounded)
    {
        PlayerState previousState = currentState;

        switch (currentState)
        {
            case PlayerState.Grounded:
                if (!isGrounded)
                {
                    currentState = PlayerState.Airborne;
                }
                break;
            case PlayerState.Airborne:
                if (isGrounded)
                {
                    currentState = PlayerState.Grounded;
                }
                break;
        }

        // ВОССТАНОВЛЕНИЕ ПРЫЖКОВ ПРИ ПРИЗЕМЛЕНИИ
        if (previousState == PlayerState.Airborne && currentState == PlayerState.Grounded)
        {
            extraJumpsLeft = maxExtraJumps;
            Debug.Log($"Прыжки восстановлены при приземлении! Доступно: {extraJumpsLeft}");
        }
    }

    private void ApplyMovement()
    {
        float speed = moveSpeed;
        float control = 1f;

        if (currentState == PlayerState.Airborne)
        {
            speed *= airControl;
            control = airControl;
        }
        else if (isSliding)
        {
            speed *= slideSpeedMultiplier;
            control = 1.5f;
        }

        if (isBoosting) speed *= 2f;

        Vector2 targetVelocity = new Vector2(horizontalInput * speed, rb.velocity.y);
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, control);
    }

    private void HandleJumpRequests()
    {
        if (!jumpRequested) return;

        bool isGrounded = IsGrounded();

        // Прыжок с земли или в coyote time
        bool canGroundJump = (isGrounded || coyoteTimeCounter > 0);

        // Воздушный прыжок (двойной)
        bool canAirJump = currentState == PlayerState.Airborne && extraJumpsLeft > 0;

        // Сначала пытаемся сделать прыжок с земли
        if (canGroundJump)
        {
            ExecuteGroundJump();
            Debug.Log($"Прыжок с земли! Состояние: {currentState}, Скорость Y: {rb.velocity.y:F2}");
        }
        // Если не получилось - пробуем воздушный прыжок
        else if (canAirJump)
        {
            ExecuteAirJump();
            Debug.Log($"Воздушный прыжок! Осталось: {extraJumpsLeft}, Скорость Y: {rb.velocity.y:F2}");
        }
        else
        {
            Debug.Log($"Прыжок невозможен! На земле: {isGrounded}, Coyote Time: {coyoteTimeCounter:F2}, Extra jumps: {extraJumpsLeft}");
        }

        jumpRequested = false;
    }

    private void ExecuteGroundJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        currentState = PlayerState.Airborne;
        // Важно: при прыжке с земли мы ВСЕГДА восстанавливаем количество двойных прыжков
        extraJumpsLeft = maxExtraJumps;
        ResetJumpTimers();
    }

    private void ExecuteAirJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        extraJumpsLeft--;
        ResetJumpTimers();
    }

    private void ResetJumpTimers()
    {
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
    }

    private void UpdateAnimations()
    {
        if (MenuPanel.activeSelf || Time.timeScale <= 0) return;

        animator.SetBool("Moving", Mathf.Abs(horizontalInput) > 0.1f);
        animator.SetBool("IsGrounded", currentState == PlayerState.Grounded);
        animator.SetBool("Sliding", isSliding); // Активация анимации скольжения
        Debug.Log($"Sliding: {isSliding}, State: {animator.GetCurrentAnimatorStateInfo(0).shortNameHash}");
    }

    public void AddJumpAbility(JumpAbilityData ability)
    {
        activeAbilities.Add(ability);
        RecalculateJumpParameters();
    }

    public void RemoveJumpAbility(JumpAbilityData ability)
    {
        activeAbilities.Remove(ability);
        RecalculateJumpParameters();
    }

    private void RecalculateJumpParameters()
    {
        // СОХРАНЯЕМ ИСХОДНОЕ ЗНАЧЕНИЕ ДЛЯ СЛУЧАЯ, ЕСЛИ НЕТ УЛУЧШЕНИЙ
        int originalMaxExtraJumps = maxExtraJumps;

        maxExtraJumps = 0; // Обнуляем для пересчета из улучшений
        jumpHeight = baseJumpHeight;

        foreach (var ability in activeAbilities)
        {
            maxExtraJumps += ability.additionalJumps;
            jumpHeight = Mathf.Max(jumpHeight, baseJumpHeight * ability.heightMultiplier);
        }

        // Если нет активных улучшений, восстанавливаем исходное значение
        if (activeAbilities.Count == 0)
        {
            maxExtraJumps = originalMaxExtraJumps;
        }

        float gravity = Physics2D.gravity.y * rb.gravityScale;
        jumpSpeed = Mathf.Sqrt(-2f * gravity * jumpHeight);

        // Восстанавливаем прыжки, если на земле
        if (IsGrounded())
        {
            extraJumpsLeft = maxExtraJumps;
        }

        Debug.Log($"[Recalculate] maxExtraJumps = {maxExtraJumps}, jumpHeight = {jumpHeight:F2}, jumpSpeed = {jumpSpeed:F2}");
    }

    public void SaveGame() => SC.SaveGame();
}

[System.Serializable]
public class JumpAbilityData : ScriptableObject
{
    public string abilityName;
    public int additionalJumps;
    public float heightMultiplier = 1f;
}