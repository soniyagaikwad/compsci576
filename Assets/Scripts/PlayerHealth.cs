using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 1.0f;
    private float currentHealth;

    // Add reference to your health bar image
    public Image healthBarImage;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object has a specific tag or is in a predefined list
        if (IsDamagingObject(collision.gameObject))
        {
            TakeDamage();
        }
    }

    bool IsDamagingObject(GameObject other)
    {
        // Implement logic to check if the object should cause damage
        // For example, you can use tags or maintain a list of damaging prefabs
        // Replace "Enemy" with the actual tag or the identification logic you use
        return other.CompareTag("Enemy");
    }

    void TakeDamage()
    {
        // Check if the player has run out of health before taking damage
        
        // Decrease health by 1/3
        currentHealth -= maxHealth / 3;

            // Update the health bar display
        UpdateHealthBar();
        
        if (currentHealth <= 0.2)
        {
            // Implement game over logic or any other actions
            // For now, just reset the health after a delay
            SceneManager.LoadScene("LoseGame");
        }
    }

    

    void UpdateHealthBar()
    {
        // Update the health bar display based on the current health
        healthBarImage.fillAmount = currentHealth / maxHealth;
    }
}