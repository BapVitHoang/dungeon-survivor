using UnityEngine;

public class XPGem : MonoBehaviour
{
    [SerializeField] private int xpAmount = 1; // Amount of XP this gem gives

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerLevel playerLevel = collision.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.AddXP(xpAmount); // Add XP to the player
            Destroy(gameObject); // Destroy the gem after collection
        }
    }
}
