using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureData", menuName = "Enemy/CreatureData")]
public class CreatureData : EnemyData
{
    [Header("Animation Triggers")]
    public string roarTrigger = "Roar";
    public string roarPoseTrigger = "RoarPose";
    public string crouchTrigger = "Crouch";
    public string crouchPoseTrigger = "CrouchPose";
    public string biteTrigger = "Bite";
    public string eatTrigger = "Eat";
    public string deathTrigger = "Die";
    public string idle1Trigger = "Idle1";
    public string idle2Trigger = "Idle2";
    public string idlePoseTrigger = "IdlePose";
    public string jumpOutTrigger = "JumpOut";
    public string jumpInTrigger = "JumpIn";
    public string punchTrigger = "Punch";
    public string slideTrigger = "Slide";
    public string sniffTrigger = "Sniff";
    public string walk1Trigger = "Walk1";
    public string walk2Trigger = "Walk2";

    [Header("Locomotion")]
    public float animTransSpeed = 5f;

    [Header("Roam Settings")]
    public float roamDist = 10f;
    public float roamSpeed = 1.5f;
    public float roamPauseTimer = 2f;

    [Header("Patrol Settings")]
    public bool canPatrol = true;
    public Transform[] patrolPoints;
    public bool loopPatrol = true;
    public float patrolSpeed = 2f;
    public float patrolPauseTime = 2f;
    
    public float creepSpeed = 1.2f;
    
    [Header("Leap Attack Settings")]
    public float leapRange = 6f;
    public float leapDelay = 0.8f;
    public float eatDuration = 3f;

    [Header("Ambient Animation Variants")]
    public string[] idleVariants = { "Idle1", "Idle2" };
    public string[] walkVariants = { "Walk1", "Walk2", "Crouch" };
    public string crouchIdleTrigger = "CrouchPose";
    public float ambientAnimInterval = 6f;
}