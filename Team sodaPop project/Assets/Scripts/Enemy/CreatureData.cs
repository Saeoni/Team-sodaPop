using UnityEngine;

public enum MovementMode
{
    None,
    Patrol,
    Roam
}

[CreateAssetMenu(fileName = "NewCreatureData", menuName = "Enemy/CreatureData")]
public class CreatureData : EnemyData
{
    
    [Header("Combat Settings")]
    public string punchTrigger = "Punch";
    public string biteTrigger = "Bite";
    public string roarTrigger = "Roar";
    public int punchDamage = 10;
    public int biteDamage = 15;
    public float punchCooldown = 5f;
    
    [Header("Locomotion Settings")]
    public string chaseStyleParam = "ChaseStyle"; // 0=Idle,1=Run,2=Creepy,3=Crouch
    public float animTransSpeed = 5f;
    public float creepSpeed = 1.2f;

    [Header("Patrol or Roam Mode")]
    public MovementMode movementMode = MovementMode.Patrol;

    [Header("Roam Settings")]
    public float roamDist = 10f;
    public float roamSpeed = 2.2f;
    public float roamPauseTimer = 2f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public bool loopPatrol = true;
    public float patrolSpeed = 1.8f;
    public float patrolPauseTime = 2f;

    [Header("Leap Attack Settings")]
    public string leapOutTrigger = "LeapOut";
    public string leapInTrigger = "LeapIn";
    public string eatTrigger = "Eat";
    public float leapRange = 8f;
    public float jumpOutDuration = 0.9f;
    public float jumpInDuration = 0.8f;
    public float eatDuration = 4.84f;

    [Header("Ambient Settings")]
    public float ambientAnimInterval = 6f;
    public string ambientIndexParam = "AmbientIndex";
    public float roarCooldown = 8f;
    public float sniffCooldown = 10f;
    
    [Header("Creeper Death")]
    public string deathTrigger = "Die";
    public string isDeadBool = "isDead";
 
}