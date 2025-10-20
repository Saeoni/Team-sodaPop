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
    [Header("Cooldowns")]
    public float roarCooldown = 8f;
    public float sniffCooldown = 10f;
    public float punchCooldown = 5f;

    [Header("Animator Parameters")]
    public string speedParam = "Speed";          // locomotion blend tree
    public string chaseStyleParam = "ChaseStyle"; // 0=Idle,1=Run,2=Creepy,3=Crouch
    public string ambientIndexParam = "AmbientIndex"; // drives ambient blend tree
    public string isDeadBool = "isDead";

    [Header("Combat Triggers")]
    public string punchTrigger = "Punch";
    public string biteTrigger = "Bite";
    public string leapOutTrigger = "JumpOut";
    public string leapInTrigger = "JumpIn";
    public string eatTrigger = "Eat";
    public string roarTrigger = "Roar";

    [Header("Death Trigger (optional)")]
    public string deathTrigger = "Die";

    [Header("Locomotion Settings")]
    public float animTransSpeed = 5f;
    public new float chaseSpeed = 3.5f;
    public float creepSpeed = 1.2f;

    [Header("Movement Mode")]
    public MovementMode movementMode = MovementMode.Patrol;

    [Header("Roam Settings")]
    public float roamDist = 10f;
    public float roamSpeed = 1.5f;
    public float roamPauseTimer = 2f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public bool loopPatrol = true;
    public float patrolSpeed = 2f;
    public float patrolPauseTime = 2f;

    [Header("Leap Attack Settings")]
    public float leapRange = 6f;
    public float jumpOutDuration = 1.28f;
    public float jumpInDuration = 1.28f;
    public float eatDuration = 4.84f;

    [Header("Ambient Settings")]
    public float ambientAnimInterval = 6f;
}