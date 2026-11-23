using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 5f;
    [Tooltip("左搖桿死區門檻，避免輕微偏移導致角色自己走動")]
    public float moveDeadzone = 0.15f;

    [Header("Camera Look Settings")]
    [Tooltip("將第一人稱攝影機（Main Camera）拖曳到此欄位")]
    public Transform cameraTransform;
    [Tooltip("右搖桿轉向速度（度 / 秒）")]
    public float lookSensitivity = 180f;
    [Tooltip("右搖桿死區門檻")]
    public float lookDeadzone = 0.15f;
    [Tooltip("是否反轉上下視角（勾選後往上推會低頭）")]
    public bool invertY = false;

    [Header("Gravity & Ground Check")]
    [Tooltip("重力加速度，預設為 -9.81")]
    public float gravity = -9.81f;
    [Tooltip("將玩家腳底的 GroundCheck 空物件拖曳到此欄位")]
    public Transform groundCheck;
    [Tooltip("勾選要視為地面的 Layer（例如 Ground）")]
    public LayerMask groundMask;
    [Tooltip("腳底偵測球體的半徑")]
    public float groundDistance = 0.4f;

    [Header("Jump Settings")]
    [Tooltip("跳躍高度／力道，教學中調整為 1.5 左右")]
    public float jumpForce = 1.5f;

    [Header("Footsteps")]
    [Tooltip("左腳 AudioSource 元件")]
    public AudioSource leftFootAudioSource;
    [Tooltip("右腳 AudioSource 元件")]
    public AudioSource rightFootAudioSource;
    [Tooltip("腳步音效片段陣列（可放入多個音效隨機播放）")]
    public AudioClip[] footstepSounds;
    [Tooltip("每次腳步聲的時間間隔（秒）")]
    public float stepInterval = 0.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isGrounded;
    private float xRotation = 0f;
    private float nextFootstepTime = 0f;
    private bool isLeftFootstep = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void CheckGround()
    {
        if (groundCheck == null) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 踩在地面且垂直速度小於 0 時，給予一個小的負值讓角色穩穩貼合地面
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
    }

    void HandleMovement()
    {
        moveInput = Vector2.zero;

        // 1. 讀取手把左搖桿（前後左右移動）
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.magnitude > moveDeadzone)
            {
                moveInput = stick;
            }
        }

        // 2. 鍵盤 WASD / 方向鍵備用
        if (Keyboard.current != null)
        {
            float x = 0f;
            float y = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;

            if (x != 0f || y != 0f)
            {
                moveInput = new Vector2(x, y);
            }
        }

        if (controller == null) return;

        Vector3 movement = transform.right * moveInput.x + transform.forward * moveInput.y;
        movement.y = 0f;

        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        controller.Move(movement * movementSpeed * Time.deltaTime);
    }

    void HandleLook()
    {
        if (cameraTransform == null) return;

        Vector2 lookInput = Vector2.zero;

        // 讀取手把右搖桿（左右轉頭、上下看）
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.rightStick.ReadValue();
            if (stick.magnitude > lookDeadzone)
            {
                lookInput = stick;
            }
        }

        // 1. 左右轉向：旋轉玩家本體（Y 軸），讓 transform.forward 跟著轉
        float yaw = lookInput.x * lookSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * yaw);

        // 2. 上下轉向：只旋轉攝影機（X 軸），並限制在 -90 到 +90 度之間
        float pitchDelta = lookInput.y * lookSensitivity * Time.deltaTime;
        if (invertY)
        {
            xRotation += pitchDelta;
        }
        else
        {
            xRotation -= pitchDelta;
        }

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleFootsteps()
    {
        if (!isGrounded) return;
        if (moveInput.magnitude <= 0.1f) return;
        if (Time.time < nextFootstepTime) return;

        PlayFootstepSound();
        nextFootstepTime = Time.time + stepInterval;
    }

    void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        int index = Random.Range(0, footstepSounds.Length);
        AudioClip footstepClip = footstepSounds[index];
        if (footstepClip == null) return;

        if (isLeftFootstep)
        {
            if (leftFootAudioSource != null)
            {
                leftFootAudioSource.PlayOneShot(footstepClip);
            }
        }
        else
        {
            if (rightFootAudioSource != null)
            {
                rightFootAudioSource.PlayOneShot(footstepClip);
            }
        }

        isLeftFootstep = !isLeftFootstep;
    }

    void HandleJump()
    {
        bool jumpPressed = false;

        // 1. 手把南向鍵（Xbox 的 A 鍵 / PlayStation 的 Cross ✕ 鍵）剛按下的那一幀
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            jumpPressed = true;
        }

        // 2. 鍵盤 Space 鍵備用
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpPressed = true;
        }

        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void HandleGravity()
    {
        if (controller == null) return;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleLook();
        HandleFootsteps();
        HandleJump();
        HandleGravity();
    }
}