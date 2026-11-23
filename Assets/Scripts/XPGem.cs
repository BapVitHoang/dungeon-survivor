using UnityEngine;

public class XPGem : MonoBehaviour
{
    [SerializeField] private int xpAmount = 1;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private AudioClip collectSFX;

    private Transform playerTransform;
    private PlayerLevel playerLevel;
    private bool isCollected;

    public void Initialize(Transform playerTarget, PlayerLevel levelRef)
    {
        playerTransform = playerTarget;
        playerLevel = levelRef;
        isCollected = false;
        if (playerTransform != null && playerLevel == null)
        {
            playerLevel = playerTransform.GetComponent<PlayerLevel>();
        }
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerLevel = playerObj.GetComponent<PlayerLevel>();
            }
        }

    }

    private void Update()
    {
        if (playerTransform == null || playerLevel == null)
        {
            return;
        }

        float magnetRadius = playerLevel.MagnetRadius;
        float distanceSqr = (playerTransform.position - transform.position).sqrMagnitude;
        if (distanceSqr <= magnetRadius * magnetRadius)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected)
        {
            return;
        }

        if (collision.TryGetComponent(out PlayerLevel hitplayerLevel))
        {
            isCollected = true;
            hitplayerLevel.AddXP(xpAmount);
            AudioManager.Instance?.PlaySFX(collectSFX, 0.25f);
            ReleaseToPool();
        }
    }

    private void ReleaseToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReleaseXPGem(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
