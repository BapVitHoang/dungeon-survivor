using UnityEngine;
public class EliteHealthBar : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Transform fillTransform;
    private void Awake()
    {
        if (enemy == null)
        {
            enemy = GetComponentInParent<Enemy>();
        }
    }

    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (enemy!=null)
        {
            enemy.OnHealthChanged -= HandleHealthChanged;
        }
    }
    private void HandleHealthChanged(int currentHP, int maxHP)
    {
        if (fillTransform == null || maxHP <= 0)
        {
            return;
        }

        float ratio = Mathf.Clamp01((float)currentHP / maxHP);
        Vector3 scale = fillTransform.localScale;
        scale.x = ratio;
        fillTransform.localScale = scale;
    }
}
