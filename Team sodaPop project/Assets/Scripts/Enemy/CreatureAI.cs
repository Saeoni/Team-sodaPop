using UnityEngine;
using UnityEngine.AI;

public class CreatureAI : EnemyAI
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Direction = Animator.StringToHash("Direction");
    private int _patrolIndex;
    private float _patrolPauseTimer;
    private bool _isPaused;

    private Vector3 _roamTarget;
    private float _roamPauseTimer;
    private bool _isRoamingPaused;
    
    protected override void Start()
    {
        base.Start();
        _playerTransform = Gamemanager.Instance.player.transform;
    }

    protected override void Update()
    {
        base.Update(); // Includes CheckLineOfSight()

        var data = (CreatureData)enemyData;
        if (data == null) return;

        if (CanSeePlayer)
        {
            Debug.Log($"Creature sees player at angle: {angleToPlayer}");

            // Optional: trigger cinematic reaction if angle is narrow
            if (angleToPlayer <= data.FOV * 0.25f)
            {
                OnPlayerSpotted(); // ✅ Trigger chase if not already
            }
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
    
    protected override void OnPlayerSpotted()
    {
        var data = (CreatureData)enemyData;
        animator.SetTrigger(data.roarTrigger);
        agent.speed = data.chaseSpeed;
        agent.SetDestination(_playerTransform.position);

        if (!(agent.remainingDistance <= data.stoppingDist)) return;
        agent.ResetPath();
        
    }

    protected override void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerSpotted();
    }

    private void Patrol(CreatureData data)
    {
        if (_isPaused)
        {
            _patrolPauseTimer += Time.deltaTime;
            if (!(_patrolPauseTimer >= data.patrolPauseTime)) return;
            _isPaused = false;
            _patrolPauseTimer = 0f;

            _patrolIndex = data.loopPatrol
                ? (_patrolIndex + 1) % data.patrolPoints.Length
                : Mathf.Min(_patrolIndex + 1, data.patrolPoints.Length - 1);

            agent.SetDestination(data.patrolPoints[_patrolIndex].position);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            _isPaused = true;
            agent.ResetPath();
        }

        agent.speed = data.patrolSpeed;
    }

    private void Roam(CreatureData data)
    {
        if (_isRoamingPaused)
        {
            _roamPauseTimer += Time.deltaTime;
            if (!(_roamPauseTimer >= data.roamPauseTimer)) return;
            _isRoamingPaused = false;
            _roamPauseTimer = 0f;
            PickNewRoamTarget(data);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            _isRoamingPaused = true;
            agent.ResetPath();
        }

        agent.speed = data.roamSpeed;
    }

    private void PickNewRoamTarget(CreatureData data)
    {
        var randomCircle = Random.insideUnitCircle * data.roamDist;
        var randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
        _roamTarget = SpawnPos + randomOffset;

        if (NavMesh.SamplePosition(_roamTarget, out var hit, data.roamDist, NavMesh.AllAreas))
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