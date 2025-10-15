using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureData", menuName = "Enemy/CreatureData")]
public class CreatureData : EnemyData
{
    [Header("Patrol & Roam")]
    public bool canPatrol;
    public Transform[] patrolPoints;
    public float patrolPauseTime = 2f;
    public bool loopPatrol = true;
    public float roamDist = 5f;
    public float roamPauseTimer = 2f;
    public float roamSpeed = 1.5f;
    public float patrolSpeed = 2f;

    [Header("Animation")]
    public float animTransSpeed = 5f;
    public string roarTrigger = "Roar";

}
