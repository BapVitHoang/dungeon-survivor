using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float riseHeight = 0.7f;
    [SerializeField] private float duration = 0.5f;

    private Coroutine animateCoroutine;

    private void Awake()
    {
        if (damageText == null)
        {
            damageText = GetComponentInChildren<TMP_Text>();
        }
    }

    public void Play(int damageAmount,Color color, Action<DamagePopup> releaseCallback = null)
    {
        color.a = 1f;
        if (damageText != null)
        {
            damageText.text = damageAmount.ToString();
            damageText.color = color;
        }

        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
        }
        animateCoroutine = StartCoroutine(AnimateAndRelease(color,releaseCallback));
    }

    private IEnumerator AnimateAndRelease(Color baseColor, Action<DamagePopup> releaseCallback)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0f, riseHeight, 0f);

        yield return Ease.Tween(duration, t =>
        {
            float posT = Ease.OutCubic(t);
            transform.position = Vector3.Lerp(startPos, endPos, posT);

            if (damageText != null)
            {
                Color c = baseColor;
                c.a = 1f - (t * t);
                damageText.color = c;
            }

        }, null, unscaled: false);
        animateCoroutine = null;
        if (releaseCallback != null)
        {
            releaseCallback.Invoke(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if (animateCoroutine !=null)
        {
            StopCoroutine(animateCoroutine);
            animateCoroutine = null;
        }
    }
}

