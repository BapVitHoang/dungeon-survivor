using UnityEngine;

public class XPGem : MonoBehaviour
{
    [SerializeField] private int xpAmount = 1; // Amount of XP this gem gives
    [SerializeField] private AudioClip collectSFX; // Sound effect for collecting the gem
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerLevel playerLevel = collision.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.AddXP(xpAmount); // Add XP to the player
            AudioManager.Instance?.PlaySFX(collectSFX,0.25f);
            Destroy(gameObject); // Destroy the gem after collection
        }
    }
}
