using UnityEngine;

public class PlayerGameManagerBridge : MonoBehaviour
{
    public void UpdateHealthUI(float maxHealth, float currentHealth)
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.setMaxHealth(maxHealth);
        GameManager.Instance.updateHealth(currentHealth);
    }

    public void UpdateAmmoUI(float maxAmmo, float currentAmmo)
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.setMaxAmmo(maxAmmo);
        GameManager.Instance.updateAmmo(currentAmmo);
    }

    public void ShowAmmoWarning()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AmmoWarning();
    }

    public void LoseGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoseGame();
    }
}