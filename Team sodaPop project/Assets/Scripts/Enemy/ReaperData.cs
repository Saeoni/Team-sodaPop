using UnityEngine;

[CreateAssetMenu(fileName = "NewReaperData", menuName = "Enemy/ReaperData")]
public class ReaperData : EnemyData
{
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

    [Header("Stalk Teleport Settings")] 
    public GameObject stalkTeleportOutVFX;
    public GameObject teleportVFX;
    public float stalkTeleportCooldown;
    public float stalkTeleportChance;
    public float stalkTeleportDelay;

    [Header("Kill VFX")] 
    public GameObject redSlashOfDeath;
    public GameObject dualPunchHitFX;
    
    [Header("Animation")]
    public float animTransSpeed = 5f;
    public string teleportTrigger = "Teleport";
    
    [Header("Aggression Settings")] public float aggressionNoiseThreshold = 0.8f;
    public float aggressionStalkTime = 6f;
    public string aggressiveTrigger = "Spasm";
}

