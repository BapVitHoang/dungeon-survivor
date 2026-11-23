using UnityEngine;
using System;
using System.Collections;

public class DeathVFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private float lifetime = 0.45f;

    private Coroutine returnCoroutine;

    private void Awake()
    {
        if (ps == null)
        {
            ps = GetComponent<ParticleSystem>();
        }
    }

    public void Play(Color tint, Action<DeathVFX> releaseCallback = null)
    {
        if (ps != null)
        {
            ParticleSystem.MainModule main = ps.main;
            main.startColor = tint;
            ps.Clear();
            ps.Play();
        }

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        returnCoroutine = StartCoroutine(ReturnAfterLifetime(releaseCallback));
    }

    private IEnumerator ReturnAfterLifetime(Action<DeathVFX> releaseCallback)
    {
        yield return new WaitForSeconds(lifetime);
        returnCoroutine = null;
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
        if(returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }
 
}
