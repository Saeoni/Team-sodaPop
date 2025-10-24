using System.Collections;
using UnityEngine;


public class ReaperAI : EnemyAI
{
    private enum ReaperPunchType { LeftHand, RightHand, DualHand}
    
    private static readonly int HasSpawned = Animator.StringToHash("hasSpawned");
    private static readonly int IsSpasming = Animator.StringToHash("IsSpasming");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Direction = Animator.StringToHash("Direction");
    private static readonly int SpasmIntensity = Animator.StringToHash("SpasmIntensity");
    private static readonly int SpasmSpeed = Animator.StringToHash("AggressiveState");
    private static readonly int Punch1Trigger = Animator.StringToHash("Punch1Trigger");
    private static readonly int Punch2Trigger = Animator.StringToHash("Punch2Trigger");
    private static readonly int Punch3Trigger = Animator.StringToHash("Punch3Trigger");
    private static readonly int IdleIndex = Animator.StringToHash("IdleIndex");
    
    private bool _isActive;
    private bool _killTriggered;
    private bool _hasTriggeredSpasm;
    private bool _isAggressive;
    private bool _isTeleporting;

    private float _spasmCooldownTimer;
    private float _stalkTimer;
    private float _stalkTeleportTimer;
    private float _idleChangeTimer;
    private int _currentIdleIndex;

    protected override void Awake()
    {
        base.Awake();
        if (playerInTrigger) TrySpasm();
    }

    protected override void Start()
    {
        base.Start();
        agent.enabled = true;
        _isActive = true;
        _stalkTimer = 0f;
    }

    protected override void Update()
    {
        base.Update();
        if (!_isActive || _killTriggered) return;

        HandleMovement();
        HandleLocomotion();
        CheckAggression();
        HandleIdleRandomization();
        if (_hasTriggeredSpasm)
        {
            _spasmCooldownTimer += Time.deltaTime;
            var data = (ReaperData)enemyData;
            if (_spasmCooldownTimer >= data.spasmCooldown)
            {
                _hasTriggeredSpasm = false;
                _spasmCooldownTimer = 0f;
            }
        }

        HandleStalkTeleport();
    }

    public void OnSpawnFinished()
    {
        var data = (ReaperData)enemyData;
        
        if (data.spawnVFX == null) return;
        var vfx = Instantiate(data.spawnVFX, transform.position, Quaternion.identity);
        Destroy(vfx, 3f);
        
        // Spawn Sequence Complete
        animator.SetBool(HasSpawned, true);
    }

    private void HandleIdleRandomization()
    {
        var data = (ReaperData)enemyData;
        
        // Only randomize when basically in Idle
        if (animator.GetFloat(Speed) > 0.1f) return;
        
        _idleChangeTimer += Time.deltaTime;
        if (_idleChangeTimer >= data.idleChangeInterval)
        {
            _idleChangeTimer = 0f;
            _currentIdleIndex = GetWeightedIdleIndex(data.idleWeights);
            animator.SetFloat(IdleIndex, _currentIdleIndex);
        }
    }

    private static int GetWeightedIdleIndex(Vector3 weights)
    {
        var total = weights.x * weights.y * weights.z;
        var roll = Random.value *  total;

        if (roll < weights.x) return 0;              // Idle1
        return roll < weights.x + weights.y ? 1 :   // Idle2
            2; // Idle3
    }
    
    private void HandleStalkTeleport()
    {
        _stalkTeleportTimer += Time.deltaTime;
        var data = (ReaperData)enemyData;

        if (!_isAggressive || _killTriggered || _isTeleporting ||
            !(_stalkTeleportTimer >= data.stalkTeleportCooldown)) return;
        _stalkTeleportTimer = 0f;
        if (Random.value <= data.stalkTeleportChance)
            StartCoroutine(PerformStalkTeleportSequence());
    }

    private IEnumerator PerformStalkTeleportSequence()
    {
        _isTeleporting = true;
        TeleportOut();

        yield return new WaitForSeconds(((ReaperData)enemyData).stalkTeleportDelay);

        TeleportIn(false); // suspenseful reappearance
        _isTeleporting = false;
    }

    private void TeleportOut()
    {
        var data = (ReaperData)enemyData;
        if (data.stalkTeleportOutVFX)
            Instantiate(data.stalkTeleportOutVFX, transform.position, Quaternion.identity);

        model.enabled = false;
        agent.enabled = false;
    }

    private void TeleportIn(bool forKill)
    {
        var data = (ReaperData)enemyData;
        Vector3 targetPos;

        if (forKill)
        {
            Vector3 offset = PlayerTransform.forward * 1.5f;
            targetPos = PlayerTransform.position + offset;
        }
        else
        {
            Vector3 offset = Random.insideUnitSphere * 8f;
            offset.y = 0f;
            targetPos = PlayerTransform.position + offset;
        }

        transform.position = targetPos;
        agent.Warp(targetPos);
        transform.LookAt(PlayerTransform);

        if (data.teleportVFX)
            Instantiate(data.teleportVFX, transform.position, Quaternion.identity);

        model.enabled = true;
        agent.enabled = true;
        
        animator.SetTrigger(((ReaperData)enemyData).teleportTrigger);
    }

    protected override void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        TrySpasm();
        TeleportIn(true); // cinematic kill setup
        StartCoroutine(DelayedKill());
    }

    private void TrySpasm()
    {
        if (_hasTriggeredSpasm) return;
        _hasTriggeredSpasm = true;
        _spasmCooldownTimer = 0f;
        StartCoroutine(DelayedSpasm());
    }

    private void CheckAggression()
    {
        if (_isAggressive) return;

        var data = (ReaperData)enemyData;
        var noiseTriggered = gamemanager.instance.CanPlayerBeHeard(transform.position, data.hearingRadius,
            data.aggressionNoiseThreshold);
        bool timeTriggered = _stalkTimer >= data.aggressionStalkTime;

       if (!noiseTriggered && !timeTriggered) return;

        _isAggressive = true;
        animator.SetTrigger(data.aggressiveTrigger);
        animator.SetBool(SpasmSpeed, true);
    }

    private void HandleMovement()
    {
        var data = (ReaperData)enemyData;
        _stalkTimer += Time.deltaTime;

        var t = Mathf.Clamp01(_stalkTimer / data.maxStalkTime);
        agent.speed = Mathf.Lerp(data.minSpeed, data.maxSpeed, data.speedRampCurve.Evaluate(t));

        var distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (!_isAggressive && _stalkTimer >= data.maxStalkTime * 0.5f)
            OnPlayerSpotted();

        if (_stalkTimer >= data.maxStalkTime || (playerInTrigger && distanceToPlayer <= data.killDistance))
        {
            TeleportIn(true);
            TriggerKill();
            return;
        }

        if (gamemanager.instance.CanPlayerBeHeard(
                transform.position,
                data.hearingRadius,
                data.aggressionNoiseThreshold))
        {
            TeleportIn(false);
            StartCoroutine(DelayedKill());
        }

        if (!playerInTrigger)
        {
            agent.isStopped = false;
            agent.SetDestination(PlayerTransform.position);
            animator.SetBool(IsSpasming, false);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool(IsSpasming, true);
        }

        transform.LookAt(PlayerTransform);
    }

    private void HandleLocomotion()
    {
        var data = (ReaperData)enemyData;
        var speed = agent.velocity.magnitude;
        var direction = transform.InverseTransformDirection(agent.velocity).x;

        animator.SetFloat(Speed, Mathf.Lerp(animator.GetFloat(Speed), speed, Time.deltaTime * data.animTransSpeed));
        animator.SetFloat(Direction,
            Mathf.Lerp(animator.GetFloat(Direction), direction, Time.deltaTime * data.animTransSpeed));
    }

    protected override void OnPlayerSpotted()
    {
        var data = (ReaperData)enemyData;

        if (!_isAggressive)
        {
            animator.SetTrigger(data.spasmTrigger);
            animator.SetBool(SpasmSpeed, true);
            _isAggressive = true;
        }

        agent.speed = data.maxSpeed;
        agent.SetDestination(PlayerTransform.position);

        if (agent.remainingDistance <= data.stoppingDist)
            agent.ResetPath();
    }

    private void TriggerKill()
    {
        _killTriggered = true;
        var punchType = (ReaperPunchType)Random.Range(0, 3);

        switch (punchType)
        {
            case ReaperPunchType.LeftHand:
                animator.SetTrigger(Punch1Trigger);
                break;
            case ReaperPunchType.RightHand:
                animator.SetTrigger(Punch2Trigger);
                break;
            case ReaperPunchType.DualHand:
                animator.SetTrigger(Punch3Trigger);
                break;
        }

        gamemanager.instance.youLose();
    }

    private IEnumerator DelayedKill()
    {
        yield return new WaitForSeconds(1.5f);
        TriggerKill();
    }

    private IEnumerator DelayedSpasm()
    {
        var data = (ReaperData)enemyData;
        yield return new WaitForSeconds(data.spasmDelay);

        var intensity = Random.Range((int)data.spasmIntensityRange.x, (int)data.spasmIntensityRange.y);
        animator.SetInteger(SpasmIntensity, intensity);
        animator.SetTrigger(data.spasmTrigger);
    }

    private void SpawnPunchHitFX(Vector3 spawnPosition, bool isDualHand)
    {
        var data = (ReaperData)enemyData;
        var fxPrefab = isDualHand ? data.dualPunchHitFX : data.redSlashOfDeath;

        if (fxPrefab == null) return;

        var fx = Instantiate(fxPrefab, spawnPosition, Quaternion.identity);

        foreach (string child in new[]
                 {
                     "BasicHit", "Shockwave (3)", "Shockwave (2)", "Shockwave (1)", "Shockwave", "Flash", "Sparks"
                 })
        {
            var ps = fx.transform.Find(child)?.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
    }
    public void TriggerLeftHandFX() => SpawnPunchHitFX(leftHandHitSpawn.position, false);
    public void TriggerRightHandFX() => SpawnPunchHitFX(rightHandHitSpawn.position, false);

    public void TriggerDualHandFX()
    {
        SpawnPunchHitFX(leftHandHitSpawn.position, true);
        SpawnPunchHitFX(rightHandHitSpawn.position, true);
    }

    public override void takeDamage(int amount)
    {
        if (_killTriggered || _isTeleporting) return;

        var data = (ReaperData)enemyData;

        // Trigger damage reaction
        animator.SetTrigger(data.damageTrigger);

        // Trigger spasm as rage response
        animator.SetTrigger(data.spasmTrigger);
        animator.SetBool(SpasmSpeed, true);
    }
    
    private void OnDrawGizmosSelected()
    {
        var data = (ReaperData)enemyData;
        
        if ( data == null || headPos == null) return;

        // 👂 Hearing radius (green wire sphere)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, data.hearingRadius);

        // 👀 Vision cone (yellow lines)
        Gizmos.color = Color.yellow;
        Vector3 forward = headPos.forward;
        float halfFOV = enemyData.FOV / 2f;

        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        Gizmos.DrawRay(headPos.position, leftRayDirection * enemyData.detectionRadius);
        Gizmos.DrawRay(headPos.position, rightRayDirection * enemyData.detectionRadius);

        // Forward direction line
        Gizmos.color = Color.red;
        Gizmos.DrawRay(headPos.position, forward * enemyData.detectionRadius);
    }

}