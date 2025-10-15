using UnityEngine;
using UnityEngine.AI;

public class CreatureAI : EnemyAI
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Direction = Animator.StringToHash("Direction");
    private int patrolIndex;
    private float patrolPauseTimer;
    private bool isPaused;

    private Vector3 roamTarget;
    private float roamPauseTimer;
    private bool isRoamingPaused;

    protected override void Update()
    {
        base.Update();

        var data = (CreatureData)enemyData;

        if (data)
        {
            agent.speed = data.chaseSpeed;
            agent.SetDestination(gamemanager.instance.player.transform.position);
        }
        else if (data.canPatrol && data.patrolPoints.Length > 0)
        {
            Patrol(data);
        }
        else
        {
            Roam(data);
        }

        HandleLocomotion(data);
    }

    protected override void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var data = (CreatureData)enemyData;
        animator.SetTrigger(data.roarTrigger);
    }

    private void Patrol(CreatureData data)
    {
        if (isPaused)
        {
            patrolPauseTimer += Time.deltaTime;
            if (!(patrolPauseTimer >= data.patrolPauseTime)) return;
            isPaused = false;
            patrolPauseTimer = 0f;

            patrolIndex = data.loopPatrol
                ? (patrolIndex + 1) % data.patrolPoints.Length
                : Mathf.Min(patrolIndex + 1, data.patrolPoints.Length - 1);

            agent.SetDestination(data.patrolPoints[patrolIndex].position);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isPaused = true;
            agent.ResetPath();
        }

        agent.speed = data.patrolSpeed;
    }

    private void Roam(CreatureData data)
    {
        if (isRoamingPaused)
        {
            roamPauseTimer += Time.deltaTime;
            if (!(roamPauseTimer >= data.roamPauseTimer)) return;
            isRoamingPaused = false;
            roamPauseTimer = 0f;
            PickNewRoamTarget(data);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isRoamingPaused = true;
            agent.ResetPath();
        }

        agent.speed = data.roamSpeed;
    }

    private void PickNewRoamTarget(CreatureData data)
    {
        var randomCircle = Random.insideUnitCircle * data.roamDist;
        var randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
        roamTarget = SpawnPos + randomOffset;

        if (NavMesh.SamplePosition(roamTarget, out var hit, data.roamDist, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void HandleLocomotion(CreatureData data)
    {
        var currSpeed = agent.velocity.magnitude;
        var localVelocity = transform.InverseTransformDirection(agent.velocity);
        var direction = localVelocity.x;

        animator.SetFloat(Speed, Mathf.Lerp(animator.GetFloat(Speed), currSpeed, Time.deltaTime * data.animTransSpeed));
        animator.SetFloat(Direction, Mathf.Lerp(animator.GetFloat(Direction), direction, Time.deltaTime * data.animTransSpeed));
    }
}