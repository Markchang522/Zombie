using UnityEngine;
using UnityEngine.InputSystem;

public class CameraViewSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    public Camera firstPersonCamera;
    public Camera godViewCamera;

    private bool isGodView = false;

    void Start()
    {
        ApplyCameraState();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            isGodView = !isGodView;
            ApplyCameraState();
        }
    }

    void ApplyCameraState()
    {
        if (firstPersonCamera != null)
        {
            firstPersonCamera.enabled = !isGodView;
        }

        if (godViewCamera != null)
        {
            godViewCamera.enabled = isGodView;
        }
    }
}
