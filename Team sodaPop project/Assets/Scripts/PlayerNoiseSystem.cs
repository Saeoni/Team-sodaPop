using UnityEngine;

public class PlayerNoiseSystem : MonoBehaviour
{

    [Header("Noise Settings")] public float noiseLevel = 0f;
    public float noiseThreshold = 100f;
    public float noiseDecayRate = 10f;

    public void AddNoise(float amount)
    {
        noiseLevel += amount;
        noiseLevel = Mathf.Clamp(noiseLevel, 0f, noiseThreshold);
    }

    public void DecayNoise()
    {
        noiseLevel = Mathf.Clamp(noiseLevel, 0f, noiseDecayRate * Time.deltaTime);
    }
    
    public float normalizedNoise => Mathf.Clamp01(noiseLevel / noiseThreshold);
    // Update is called once per frame
    private void Update()
    {
        DecayNoise();
    }
}
