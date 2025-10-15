using UnityEngine;

public abstract class EnemyData : ScriptableObject
{
    [Header("General Stats")]
    public int maxHP;

    [Header("Detection")]
    public float detectionRadius;
    public float FOV = 90f;
    public LayerMask lineOfSightMask;
    public float faceTargetSpeed;
    public float stoppingDist;
    public float chaseSpeed;

    [Header("Spawn, Drops & Death FX")]
    public GameObject keyPrefab;
    public GameObject spawnVFX;
    public GameObject deathVFX;
}
