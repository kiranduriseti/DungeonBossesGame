using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Death")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private PlayerAnim anim;

    [Header("Invoke Events")]
    public UnityEvent<float, float> OnHealthChanged;
    public UnityEvent OnPlayerDied;

    private bool dead;
    private bool invulnerable;

    private void Start()
    {
        currentHealth = maxHealth;

        if (anim == null)
            anim = GetComponentInChildren<PlayerAnim>();

        InvokeHealthChanged();
    }

    public void TakeDamage(float damage)
    {
        if (dead || invulnerable)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        InvokeHealthChanged();

        Debug.Log(currentHealth);
        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (dead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        InvokeHealthChanged();
    }

    public bool NeedsHealth()
    {
        return currentHealth < maxHealth;
    }

    private void Die()
    {
        dead = true;

        if (anim != null)
            anim.SetPlayerDead(true);

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        OnPlayerDied?.Invoke();
    }

    private void InvokeHealthChanged()
    {
        OnHealthChanged?.Invoke(maxHealth, currentHealth);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetInvulnerable(bool value)
    {
        invulnerable = value;
    }
}