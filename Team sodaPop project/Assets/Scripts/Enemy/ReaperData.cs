using UnityEngine;

[CreateAssetMenu(fileName = "NewReaperData", menuName = "Enemy/ReaperData")]
public class ReaperData : EnemyData
{

    [Header("Idle Randomization")] [Tooltip("How often (in seconds) the Reaper should switch idle animations.")]
    public float idleChangeInterval = 5f;
    
    [Tooltip("Weights for Idle1, Idle2, Idle3. Higher = more likely.")]
    public Vector3 idleWeights = new Vector3(0.6f, 0.3f, 0.1f);
    
    [Header("Spasm Settings")]
    public string spasmTrigger = "Spasm";
    public float spasmCooldown = 5f;
    public float spasmDelay = 0.4f;
    public Vector2 spasmIntensityRange = new Vector2(0, 2);

    [Header("Kill Logic")]
    public float maxStalkTime = 10f;
    public float minSpeed = 2f;
    public float maxSpeed = 6f;
    public AnimationCurve speedRampCurve;
    public float killDistance = 2f;
    public string killTrigger = "Kill";

    [Header("Damage Response")]
    public string damageTrigger = "GetDamage";

    [Header("Spawn & Teleport Prefabs")] 
    public GameObject stalkTeleportOutVFX;
    public GameObject teleportVFX;
    public GameObject spawnVFX;
    
    public float stalkTeleportCooldown;
    public float stalkTeleportChance;
    public float stalkTeleportDelay;

    [Header("Kill VFX")] 
    public GameObject redSlashOfDeath;
    public GameObject dualPunchHitFX;
    
    [Header("Animation")]
    public float animTransSpeed = 5f;
    public string teleportTrigger = "Teleport";
    
    [Header("Aggression Settings")] 
    public float aggressionStalkTime = 6f;
    public string aggressiveTrigger = "Spasm";
    
    [Header("Hearing")]
    public new float hearingRadius = 15f;
    [Range(0f, 1f)]
    public new float aggressionNoiseThreshold = 0.5f;

    [Tooltip("Cooldown before calming down after losing player (seconds).")]
    public float calmDownDelay = 5f;

    
}

