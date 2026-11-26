using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0) return;

        currentHealth -= damageAmount;

        // 之後可在此更新血條 UI 與流血／受傷畫面效果

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player has died!");
        // 之後可在此顯示死亡畫面或停用玩家操作
    }
}
