using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int kills;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddKill()
    {
        kills++;
    }

    public void GameOver()
    {
        // Placeholder
    }

    public void PlayerDied()
    {
        // Placeholder
    }

    public void WinGame()
    {
        // Placeholder
    }

    public void setMaxHealth(float maxHealth)
    {
        // Placeholder
    }

    public void updateHealth(float currentHealth)
    {
        // Placeholder
    }

    public void setMaxAmmo(float maxAmmo)
    {
        // Placeholder
    }

    public void updateAmmo(float currentAmmo)
    {
        // Placeholder
    }

    public void AmmoWarning()
    {
        // Placeholder
    }

    public void LoseGame()
    {
        // Placeholder
    }
}