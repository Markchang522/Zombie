using UnityEngine;
using UnityEngine.InputSystem;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    [Tooltip("射線發射起點（例如 Main Camera 或槍口位置）")]
    public Transform firePoint;
    [Tooltip("每秒射擊次數（例如 10 代表每秒約 10 發）")]
    public float fireRate = 10f;
    [Tooltip("射線最大射程")]
    public float fireRange = 25f;

    [Header("Fire Mode & Trigger")]
    [Tooltip("勾選為全自動（按住右扳機連發）；取消勾選為單發（扣下右扳機觸發一次）")]
    public bool isAuto = false;
    [Tooltip("手把右扳機（RT / R2）的按下門檻（0 到 1 之間）")]
    [Range(0.1f, 0.95f)]
    public float triggerThreshold = 0.5f;
    [Tooltip("單發模式下 shoot 布林維持為 true 的最短秒數，確保射擊動畫能被看見")]
    public float singleShotAnimDuration = 0.12f;

    [Header("Ammo & Reload Settings")]
    [Tooltip("彈匣最大子彈容量")]
    public int maxAmmo = 30;
    [Tooltip("換彈所需時間（秒，建議與 reload 動畫長度對齊）")]
    public float reloadTime = 4f;

    [Header("Animation")]
    [Tooltip("步槍的 Animator 元件（需包含 Bool 參數 shoot 與 Trigger 參數 reload）")]
    public Animator animator;

    [Header("Sound Effects")]
    [Tooltip("播放音效用的 AudioSource 元件")]
    public AudioSource soundAudioSource;
    [Tooltip("射擊音效片段")]
    public AudioClip shootingSoundClip;
    [Tooltip("換彈音效片段")]
    public AudioClip reloadSoundClip;

    private int currentAmmo;
    private float nextFireTime = 0f;
    private float singleShotAnimEndTime = 0f;
    private bool isReloading = false;
    private bool wasTriggerPulledLastFrame = false;

    void Start()
    {
        if (firePoint == null && Camera.main != null)
        {
            firePoint = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        currentAmmo = maxAmmo;
    }

    void Update()
    {
        bool isTriggerPulled = IsRightTriggerPulled();
        bool isTriggerJustPulled = isTriggerPulled && !wasTriggerPulledLastFrame;
        wasTriggerPulledLastFrame = isTriggerPulled;

        // 正在換彈時不執行射擊與手動換彈
        if (isReloading)
        {
            SetShootAnim(false);
            return;
        }

        // 換彈鍵：手把 X 鍵（PS 為 □）或鍵盤 R 鍵；子彈未滿時可手動換彈
        if (IsReloadPressed() && currentAmmo < maxAmmo)
        {
            Reload();
            return;
        }

        bool fireInput = isAuto ? isTriggerPulled : isTriggerJustPulled;

        // 開火判斷：需符合射速冷卻時間
        if (fireInput && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                float safeFireRate = Mathf.Max(0.01f, fireRate);
                nextFireTime = Time.time + 1f / safeFireRate;
                Shoot();
            }
            else
            {
                // 子彈打光時自動換彈
                Reload();
            }
        }

        UpdateShootAnimationState(isTriggerPulled);
    }

    void UpdateShootAnimationState(bool isTriggerPulled)
    {
        if (isAuto)
        {
            bool shouldShoot = isTriggerPulled && currentAmmo > 0 && !isReloading;
            SetShootAnim(shouldShoot);
        }
        else
        {
            SetShootAnim(Time.time < singleShotAnimEndTime && !isReloading);
        }
    }

    void SetShootAnim(bool value)
    {
        if (animator != null)
        {
            animator.SetBool("shoot", value);
        }
    }

    bool IsRightTriggerPulled()
    {
        if (Gamepad.current == null) return false;
        return Gamepad.current.rightTrigger.ReadValue() >= triggerThreshold;
    }

    bool IsReloadPressed()
    {
        bool gamepadReload = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
        bool keyboardReload = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
        return gamepadReload || keyboardReload;
    }

    void Shoot()
    {
        if (firePoint == null) return;

        currentAmmo--;
        singleShotAnimEndTime = Time.time + singleShotAnimDuration;
        SetShootAnim(true);

        if (soundAudioSource != null && shootingSoundClip != null)
        {
            soundAudioSource.PlayOneShot(shootingSoundClip);
        }

        Debug.Log($"[Shoot] 剩餘子彈: {currentAmmo}/{maxAmmo}");

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, fireRange))
        {
            Debug.Log("[Shoot] 擊中: " + hit.transform.name);
        }

        if (currentAmmo <= 0)
        {
            Reload();
        }
    }

    void Reload()
    {
        if (isReloading || currentAmmo >= maxAmmo) return;

        isReloading = true;
        SetShootAnim(false);

        if (animator != null)
        {
            animator.SetTrigger("reload");
        }

        if (soundAudioSource != null && reloadSoundClip != null)
        {
            soundAudioSource.PlayOneShot(reloadSoundClip);
        }

        Debug.Log($"[Reload] 換彈中...（{reloadTime} 秒）");
        Invoke(nameof(FinishReloading), reloadTime);
    }

    void FinishReloading()
    {
        currentAmmo = maxAmmo;
        isReloading = false;

        if (animator != null)
        {
            animator.ResetTrigger("reload");
        }

        Debug.Log($"[Reload] 換彈完成！目前子彈: {currentAmmo}/{maxAmmo}");
    }
}