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
    [Header("Hearing")]
    public float hearingRadius = 15f;
    [Range(0f, 1f)]
    public float aggressionNoiseThreshold = 0.5f;
}
