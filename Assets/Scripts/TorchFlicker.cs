using UnityEngine;
using UnityEngine.Rendering.Universal;
public class TorchFlicker : MonoBehaviour
{
    [SerializeField] private Light2D torchLight;
    [SerializeField] private float flickerSpeed = 6f;
    [SerializeField] private float intensityVariance = 0.15f;
    [SerializeField] private float radiusVariance = 0.25f;

    private float baseIntensity;
    private float baseRadius;
    private float noiseOffset;

    private void Awake()
    {
        if (torchLight == null)
        {
            torchLight = GetComponentInChildren<Light2D>();
        }

        if(torchLight != null)
        {
            baseIntensity = torchLight.intensity;
            baseRadius = torchLight.pointLightOuterRadius;
        }
        noiseOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (torchLight == null) return;

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + noiseOffset, 0f);
        float wave = (noise - 0.5f) * 2f;

        torchLight.intensity = baseIntensity + wave * intensityVariance;
        torchLight.pointLightOuterRadius = baseRadius + wave * radiusVariance;
    }

    public void SetColor(Color color)
    {
        if (torchLight != null)
        {
            torchLight.color = color;
        }
    }
}
