using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Follow & Bounds")]
    [SerializeField] private Transform target;
    [SerializeField] private ArenaGenerator arenaGenerator;
    [SerializeField] private float wallBorderMargin = 1f;

    private Camera cam;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void TriggerShake(float duration, float magnitude)
    {
        if (duration <= 0f || magnitude <= 0f) return;
        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTimer = Mathf.Max(shakeTimer, duration);
        shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
    }
    // LateUpdate is called after all Update functions have been called. Put camera code here.
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        if (arenaGenerator != null && cam != null)
        {
            Bounds b = arenaGenerator.PlayableBounds;
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            float minX = b.min.x - wallBorderMargin + halfW;
            float maxX = b.max.x + wallBorderMargin - halfW;
            float minY = b.min.y - wallBorderMargin + halfH;
            float maxY = b.max.y + wallBorderMargin - halfH;

            desiredPos.x = (minX > maxX) ? b.center.x : Mathf.Clamp(desiredPos.x, minX, maxX);
            desiredPos.y = (minY > maxY) ? b.center.y : Mathf.Clamp(desiredPos.y, minY, maxY);
        }
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;
                float damper = (shakeDuration > 0f) ? Mathf.Clamp01(shakeTimer / shakeDuration) : 0f;
                Vector2 shakeOffset = Random.insideUnitCircle * (shakeMagnitude * damper);
                desiredPos.x += shakeOffset.x;
                desiredPos.y += shakeOffset.y;
                if (shakeTimer <= 0)
                {
                    shakeMagnitude = 0f;
                    shakeDuration = 0f;
                }
            }
            transform.position = desiredPos;
        }
    }
