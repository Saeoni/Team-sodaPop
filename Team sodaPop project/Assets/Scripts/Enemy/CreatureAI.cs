using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CreatureAI : EnemyAI
{
    
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int DirectionX = Animator.StringToHash("DirectionX");
    private static readonly int DirectionZ = Animator.StringToHash("DirectionZ");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    
    private int _patrolIndex;
    private float _patrolPauseTimer;
    private bool _isPaused;

    private Vector3 _roamTarget;
    private float _roamPauseTimer;
    private bool _isRoamingPaused;

    private bool _isLeaping;
    private float _ambientTimer;

    // Cooldown timers
    private float _roarTimer;
    private float _sniffTimer;
    private float _punchTimer;

    protected override void Start()
    {
        base.Start();
        var data = (CreatureData)enemyData;
        CurrentHp = data.maxHP;

        _ambientTimer = 0;
        _roarTimer = 0;
        _sniffTimer = 0;
        _punchTimer = 0;
    }

    protected override void Update()
    {
        base.Update();
        var data = (CreatureData)enemyData;
        if (data == null) return;
        if (_isLeaping) return;

        // 🔹 Timers
        _roarTimer -= Time.deltaTime;
        _sniffTimer -= Time.deltaTime;
        _punchTimer -= Time.deltaTime;
        _ambientTimer += Time.deltaTime;

        // 🔹 Always update locomotion blend tree
        HandleLocomotion(data);

        // 🔹 If agent has reached destination, increment roam/patrol pause timers
        if (agent.remainingDistance < 0.01f)
        {
            _roamPauseTimer += Time.deltaTime;
            _patrolPauseTimer += Time.deltaTime;
        }

        // 🔹 Decide behaviour based on trigger + line of sight
        if (!PlayerInTrigger || CanSeePlayer)
        {
            if (PlayerInTrigger)
            {
                if (!PlayerInTrigger || !CanSeePlayer) return;
                HandleRoamOrPatrol(data, chaseStyle: 3); // Crouch chase
                ChooseAttackStyle(data);
            }
            else
            {
                HandleRoamOrPatrol(data, chaseStyle: 1); // Normal patrol/roam
            }
        }
        else
        {
            HandleRoamOrPatrol(data, chaseStyle: 2); // Creepy Walk
        }
    }

    private void HandleRoamOrPatrol(CreatureData data, int chaseStyle)
    {
        switch (data.movementMode)
        {
            case MovementMode.Patrol:
                Patrol(data);
                break;
            case MovementMode.Roam:
                Roam(data);
                break;
            case MovementMode.None:
            default:
                HandleAmbientBehaviour(data);
                break;
        }

        animator.SetFloat(data.chaseStyleParam, chaseStyle);
    }

    private void HandleAmbientBehaviour(CreatureData data)
    {
        if (_ambientTimer < data.ambientAnimInterval) return;

        int roll = Random.Range(0, 5); // 0–4 ambient slots
        if (roll == 2 && _sniffTimer > 0f) roll = 0;
        if (roll == 4 && _roarTimer > 0f) roll = 0;

        animator.SetFloat(data.ambientIndexParam, roll);

        if (roll == 2) _sniffTimer = data.sniffCooldown;
        if (roll == 4) _roarTimer = data.roarCooldown;

        _ambientTimer = 0f;
    }

    private void ChooseAttackStyle(CreatureData data)
    {
        float distance = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distance <= 1.5f)
        {
            int closeStyle = Random.Range(0, 2);
            if (closeStyle == 0 && _punchTimer <= 0f)
            {
                animator.SetTrigger(data.punchTrigger);
                agent.speed = 0f;
                StartCoroutine(DelayedKill());
                _punchTimer = data.punchCooldown;
            }
            else
            {
                StartCoroutine(PerformBiteAttack(data));
            }
            return;
        }

        if (!(distance >= data.leapRange)) return;
        StartCoroutine(PerformLeapAttack(data));
    }

    private IEnumerator PerformBiteAttack(CreatureData data)
    {
        agent.isStopped = true;
        transform.LookAt(PlayerTransform);
        yield return new WaitForSeconds(0.3f);

        animator.SetTrigger(data.biteTrigger);
        yield return new WaitForSeconds(4.8f);

       
        agent.isStopped = false;
    }

    private IEnumerator PerformLeapAttack(CreatureData data)
    {
        _isLeaping = true;
        agent.isStopped = true;

        animator.SetTrigger(data.leapOutTrigger);
        yield return new WaitForSeconds(data.jumpOutDuration);

        Vector3 offset = PlayerTransform.forward * -1.2f;
        Vector3 targetPos = PlayerTransform.position + offset;
        transform.position = targetPos;
        agent.Warp(targetPos);
        transform.LookAt(PlayerTransform);

        animator.SetTrigger(data.leapInTrigger);
        yield return new WaitForSeconds(data.jumpInDuration);

        animator.SetTrigger(data.eatTrigger);
        yield return new WaitForSeconds(data.eatDuration);
        _isLeaping = false;
        agent.isStopped = false;
    }

    private IEnumerator DelayedKill()
    {
        yield return new WaitForSeconds(1.2f);
        Gamemanager.Instance.YouLose();
    }

    private void Patrol(CreatureData data)
    {
        if (_isPaused)
        {
            if (_patrolPauseTimer >= data.patrolPauseTime)
            {
                _isPaused = false;
                _patrolPauseTimer = 0f;

                _patrolIndex = data.loopPatrol
                    ? (_patrolIndex + 1) % data.patrolPoints.Length
                    : Mathf.Min(_patrolIndex + 1, data.patrolPoints.Length - 1);

                agent.SetDestination(data.patrolPoints[_patrolIndex].position);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            _isPaused = true;
            agent.ResetPath();
        }

        agent.speed = data.patrolSpeed;
        HandleAmbientBehaviour(data);
    }

    private void Roam(CreatureData data)
    {
        if (_isRoamingPaused)
        {
            if (_roamPauseTimer >= data.roamPauseTimer)
            {
                _isRoamingPaused = false;
                _roamPauseTimer = 0f;
                PickNewRoamTarget(data);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            _isRoamingPaused = true;
            agent.ResetPath();
        }

        agent.speed = data.roamSpeed;
        HandleAmbientBehaviour(data);
    }

    private void PickNewRoamTarget(CreatureData data)
    {
        Vector3 randomCircle = Random.insideUnitSphere * data.roamDist;
        randomCircle += transform.position;

        if (NavMesh.SamplePosition(randomCircle, out NavMeshHit hit, data.roamDist, NavMesh.AllAreas))
        {
            _roamTarget = hit.position;
            agent.SetDestination(_roamTarget);
        }
    }

    private void HandleLocomotion(CreatureData data)
    {
        Vector3 velocity = agent.velocity;
        float speed = velocity.magnitude;
        Vector3 localDirection = transform.InverseTransformDirection(velocity.normalized);

        animator.SetFloat(Speed, speed);
        animator.SetFloat(DirectionX, localDirection.x);
        animator.SetFloat(DirectionZ, localDirection.z);
    }

   

    protected override void OnEnemyDeath()
    {
        var data = (CreatureData)enemyData;
        animator.SetBool(data.isDeadBool, true);

        if (!string.IsNullOrEmpty(data.deathTrigger))
            animator.SetTrigger(data.deathTrigger);

        base.OnEnemyDeath(); // spawns key, VFX, destroys object
    }

    protected override void OnPlayerSpotted()
    {
        var data = (CreatureData)enemyData;

        if (_roarTimer <= 0f)
        {
            animator.SetTrigger(data.roarTrigger);
            _roarTimer = data.roarCooldown;
        }

        HandleRoamOrPatrol(data, chaseStyle: 3); // escalate to crouch chase
    }

    protected override void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerSpotted();
    }
     public override void takeDamage(int amount)
    {
        // Reduce HP and flash red (base handles this)
        base.takeDamage(amount);

        var data = (CreatureData)enemyData;

        // If still alive, optional roar reaction
        if (CurrentHp > 0 && _roarTimer <= 0f)
        {
            animator.SetTrigger(data.roarTrigger);
            _roarTimer = data.roarCooldown;
        }

        // If dead, trigger death animation before cleanup
        if (CurrentHp > 0) return;
        animator.SetBool(data.isDeadBool, true);

        if (!string.IsNullOrEmpty(data.deathTrigger))
            animator.SetTrigger(data.deathTrigger);

        // Call base death logic (spawns key, VFX, destroys object)
        OnEnemyDeath();
    }

    public void KillPlayer()
    {
        Gamemanager.Instance.YouLose();
    }
}
