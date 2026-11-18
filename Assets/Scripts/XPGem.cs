using UnityEngine;

public class XPGem : MonoBehaviour
{
    [SerializeField] private int xpAmount = 1;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private AudioClip collectSFX;

    private Transform playerTransform;
    private PlayerLevel playerLevel;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerLevel = playerObj.GetComponent<PlayerLevel>();
        }
    }

    private void Update()
    {
        if (playerTransform == null || playerLevel == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance <= playerLevel.MagnetRadius)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

        }
    }
  
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerLevel hitplayerLevel = collision.GetComponent<PlayerLevel>();
        if (hitplayerLevel != null)
        {
            hitplayerLevel.AddXP(xpAmount);
            AudioManager.Instance?.PlaySFX(collectSFX,0.25f);
            Destroy(gameObject); 
        }
    }
}
