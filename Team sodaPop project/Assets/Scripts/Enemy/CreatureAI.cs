using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

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

    private bool _isLeaping;
    private float _roarTimer;
    private float _sniffTimer;
    private float _punchTimer;
    private float _ambientTimer;
    [FormerlySerializedAs("_roarCooldown")] [SerializeField] private float roarCooldown = 8f;
    [FormerlySerializedAs("_sniffCooldown")] [SerializeField] private float sniffCooldown = 10f;
    [FormerlySerializedAs("_punchCooldown")] [SerializeField] private float punchCooldown = 12f;

    protected override void Start()
    {
        base.Start();
        PlayerTransform = Gamemanager.Instance.player.transform;
    }

    protected override void Update()
    {
        base.Update();

        var data = (CreatureData)enemyData;
        if (data == null) return;

        HandleAmbientBehaviour(data);

        if (CanSeePlayer)
        {
            if (angleToPlayer <= data.FOV * 0.25f)
                ChooseAttackStyle(data);
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

    private void HandleAmbientBehaviour(CreatureData data)
    {
        _roarTimer -= Time.deltaTime;
        _sniffTimer -= Time.deltaTime;
        _punchTimer -= Time.deltaTime;
        _ambientTimer += Time.deltaTime;

        if (_roarTimer <= 0f)
        {
            animator.SetTrigger(data.roarPoseTrigger);
            _roarTimer = roarCooldown;
        }

        if (_sniffTimer <= 0f)
        {
            animator.SetTrigger(data.sniffTrigger);
            _sniffTimer = sniffCooldown;
        }

        if (_punchTimer <= 0f)
        {
            animator.SetTrigger(data.punchTrigger);
            _punchTimer = punchCooldown;
        }

        if (!(_ambientTimer >= data.ambientAnimInterval)) return;
        PlayAmbientAnimation(data);
        _ambientTimer = 0f;
    }

    private void PlayAmbientAnimation(CreatureData data)
    {
        int roll = Random.Range(0, 5);
        switch (roll)
        {
            case 0:
                animator.SetTrigger(data.idleVariants[Random.Range(0, data.idleVariants.Length)]);
                break;
            case 1:
                animator.SetTrigger(data.walkVariants[Random.Range(0, data.walkVariants.Length)]);
                break;
            case 2:
                animator.SetTrigger(data.crouchPoseTrigger);
                break;
            case 3:
                animator.SetTrigger(data.sniffTrigger);
                break;
            case 4:
                animator.SetTrigger(data.crouchTrigger);
                break;
        }
    }

    private void ChooseAttackStyle(CreatureData data)
    {
        int style = Random.Range(0, 5); // Now 0–4
        switch (style)
        {
            case 0:
                StartCoroutine(PerformLeapAttack(data));
                break;
            case 1:
                animator.SetTrigger(data.walk2Trigger);
                agent.speed = data.creepSpeed;
                agent.SetDestination(PlayerTransform.position);
                break;
            case 2:
                animator.SetTrigger(data.crouchTrigger);
                agent.speed = data.chaseSpeed;
                agent.SetDestination(PlayerTransform.position);
                break;
            case 3:
                animator.SetTrigger(data.punchTrigger);
                agent.speed = 0f;
                StartCoroutine(DelayedKill());
                break;
            case 4:
                StartCoroutine(PerformBiteAttack(data)); // 🆕 Bite logic
                break;
        }
    }
    
    private IEnumerator PerformBiteAttack(CreatureData data)
    {
        agent.isStopped = true;

        // Optional: face player and pause
        transform.LookAt(PlayerTransform);
        yield return new WaitForSeconds(0.3f);

        animator.SetTrigger(data.biteTrigger);

        yield return new WaitForSeconds(1.5f); // Match animation length

        Gamemanager.Instance.YouLose();
        agent.isStopped = false;
    }
    private IEnumerator PerformLeapAttack(CreatureData data)
    {
        _isLeaping = true;
        agent.isStopped = true;

        animator.SetTrigger(data.jumpOutTrigger);
        yield return new WaitForSeconds(data.leapDelay);

        Vector3 offset = PlayerTransform.forward * -1.2f;
        Vector3 targetPos = PlayerTransform.position + offset;
        transform.position = targetPos;
        transform.LookAt(PlayerTransform);
        agent.Warp(targetPos);

        animator.SetTrigger(data.jumpInTrigger);
        yield return new WaitForSeconds(0.5f);

        // Randomize between bite or eat
        animator.SetTrigger(Random.value < 0.5f ? data.biteTrigger : data.eatTrigger);

        yield return new WaitForSeconds(data.eatDuration);
        Gamemanager.Instance.YouLose();
        _isLeaping = false;
    }

    private IEnumerator ChasePlayer(CreatureData data)
    {
        agent.isStopped = true;

        animator.SetTrigger(data.crouchTrigger);
        yield return new WaitForSeconds(1.2f);

        animator.SetTrigger(data.sniffTrigger);
        yield return new WaitForSeconds(2f);

        animator.SetTrigger(data.idle1Trigger);
        yield return new WaitForSeconds(1f);

        animator.SetTrigger(data.roarTrigger);
        yield return new WaitForSeconds(1.5f);

        animator.SetTrigger(data.crouchTrigger);
        yield return new WaitForSeconds(0.5f);

        agent.isStopped = false;
        agent.speed = data.chaseSpeed;
        agent.SetDestination(PlayerTransform.position);
    }

    protected override void OnPlayerSpotted()
    {
        var data = (CreatureData)enemyData;
        StartCoroutine(ChasePlayer(data));
        ChooseAttackStyle(data);

        if (agent.remainingDistance <= data.stoppingDist)
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
    }

    private void Roam(CreatureData data)
    {
        if (_isRoamingPaused)
        {
            _roamPauseTimer += Time.deltaTime;
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

    private IEnumerator DelayedKill()
    {
        yield return new WaitForSeconds(1.2f);
        Gamemanager.Instance.YouLose();
    }

    public override void takeDamage(int amount)
    {
        var data = (CreatureData)enemyData;
        animator.SetTrigger(data.roarTrigger);
        StartCoroutine(FlashRed());
    }
}