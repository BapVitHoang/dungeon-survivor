using UnityEngine;

public class SpriteBob: MonoBehaviour
{
    [Header("Walk Bob")]
    [SerializeField] private float walkBobFrequency = 12f;
    [SerializeField] private float walkBobHeight = 0.05f;
    [SerializeField] private float walkTiltAngle = 6f;

    [Header("Idle")]
    [SerializeField] private float idleBreatheFrequency = 2.5f;
    [SerializeField] private float idleBreatheAmount = 0.03f;

    private Rigidbody2D parentRb;
    private Vector3 baseLocalPos;
    private Vector3 baseLocalScale;
    private float animTime;

    private void Awake()
    {
        parentRb = GetComponentInParent<Rigidbody2D>();
        baseLocalPos = transform.localPosition;
        baseLocalScale = transform.localScale;

        animTime = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        animTime += Time.deltaTime;
        bool isMoving = (parentRb != null && parentRb.linearVelocity.sqrMagnitude > 0.05f);

        if (isMoving )
        {
            float yOffset = Mathf.Abs(Mathf.Sin(animTime * walkBobFrequency)) * walkBobHeight;
            float zTilt = Mathf.Sin(animTime * walkBobFrequency) * walkTiltAngle;
            transform.localPosition = baseLocalPos + new Vector3(0f,yOffset, 0f);
            transform.localRotation = Quaternion.Euler(0f,0f,zTilt);
            transform.localScale = baseLocalScale;
        }
        else
        {
            float breathe = 1f + Mathf.Sin(animTime * idleBreatheFrequency) * idleBreatheAmount;
            transform.localPosition = Vector3.Lerp(transform.localPosition, baseLocalPos, Time.deltaTime * 10f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            transform.localScale = new Vector3(baseLocalScale.x, baseLocalScale.y * breathe, baseLocalScale.z);
        }
    }
}
