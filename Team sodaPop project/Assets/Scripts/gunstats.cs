using UnityEngine;

[CreateAssetMenu]
public class gunstats : ScriptableObject
{
    public GameObject gunModel;
    [Range(1, 10)] public int shootDamage;
    [Range(0.1f, 3)] public float shootRate;
    [Range(5, 500)] public int shootDist;
    public int ammoCur;
    [Range(5, 50)] public int ammoMax;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
}