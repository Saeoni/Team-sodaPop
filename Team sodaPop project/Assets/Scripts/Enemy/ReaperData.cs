using UnityEngine;

[CreateAssetMenu(fileName = "NewReaperData", menuName = "Enemy/ReaperData")]
public class ReaperData : EnemyData
{

    [Header("Aggression")]
    public float aggressionStalkTime = 10f;
    public string aggressiveTrigger = "Aggressive";
    public string spasmTrigger = "Spasm";
    public float calmDownDelay = 5f;

    [Header("Locomotion")]
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public float maxStalkTime = 20f;
    public float animTransSpeed = 5f;
    public AnimationCurve speedRampCurve;

    [Header("Idle Randomization")]
    public float idleChangeInterval = 3f;
    public Vector3 idleWeights = new Vector3(1f, 1f, 1f);

    [Header("Teleportation")]
    public GameObject teleportVFX;
    public string teleportTrigger = "Teleport";
    public float killDistance = 2f;

    [Header("Stalk Teleport")]
    public GameObject stalkTeleportOutVFX;
   // public GameObject preTeleportCueVFX;
    public float stalkTeleportCooldown = 10f;
    public float stalkTeleportPreDelay = 1.5f;
    public float stalkTeleportDelay = 2f;
    [Range(0f, 1f)] public float stalkTeleportChance = 0.3f;

    [Header("Random Teleport")]
    [Range(0f, 1f)] public float randomTeleportChance = 0.05f;
    public float randomTeleportInterval = 3f;

    [Header("Spasm")]
    public float spasmCooldown = 5f;
    public float spasmDelay = 1f;
    public Vector2 spasmIntensityRange = new Vector2(1f, 3f);

    [Header("Combat FX")]
    public GameObject redSlashOfDeath;
    public GameObject dualPunchHitFX;
    public string damageTrigger = "Damage";

    [Header("Spawn")]
    public GameObject spawnVFX;
    public string spawnTrigger = "SpawnTrigger";
    
}

