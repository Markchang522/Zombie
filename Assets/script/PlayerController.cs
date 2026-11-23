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

    private CharacterController controller;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void HandleMovement()
    {
        if (controller == null) return;

        Vector2 moveInput = Vector2.zero;

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

        // 2. 上下轉向：只旋轉攝影機（X 軸），並限制在 -90 到 +90 度之間避免翻轉
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

    void Update()
    {
        HandleMovement();
        HandleLook();
    }
}