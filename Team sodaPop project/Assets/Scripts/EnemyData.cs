using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyType EnemyType;

    [Header("Combat")]
    public int maxHP;
    public int damage;
    public float attackRange;
    public float attackCooldown;

    [Header("Detection")]
    public float detectionRadius;
    public float FOV;
    public LayerMask lineOfSightMask;

    [Header("Movement")]
    public float chaseSpeed;
    public float faceTargetSpeed;
    public float roamDist;
    public float roamPauseTimer;
    public float animTransSpeed;

    [Header("Patrol")]
    public bool canPatrol = false;
    public Transform[] patrolPoints;
    public float patrolPauseTime = 2f;
    public bool loopPatrol = true;

    [Header("Reaper Settings")]
    public float maxStalkTime;
    public float minSpeed;
    public float maxSpeed;

    [Header("Animation")]
    public string shootTrigger = "Shoot";
    public string killTrigger = "TriggerKill";
    public string spawnTrigger = "SpawnSequence"; // Reuses spawn animation
    public string teleportTrigger = "TeleportSequence";
    public string spasmTrigger = "DoSpasm";
    public string damageTrigger = "GetDamage";

    [Header("Drops & FX")]
    public GameObject keyPrefab;
    public GameObject deathVFX;
    public GameObject teleportVFX; // Red circle or shadow burst
}
