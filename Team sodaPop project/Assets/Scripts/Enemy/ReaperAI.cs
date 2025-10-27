using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class ReaperAI : EnemyAI
{
    private enum ReaperPunchType { LeftHand, RightHand, DualHand }

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
    private bool _hasSpawned;

    private float _spasmCooldownTimer;
    private float _stalkTimer;
    private float _stalkTeleportTimer;
    private float _idleChangeTimer;
    private float _randomTeleportCooldown;
    private int _currentIdleIndex;

    protected override void Start()
    {
        base.Start();
        TriggerSpawn();
        _stalkTimer = 0f;
    }

    protected override void Update()
    {
        base.Update();
        if (!_isActive || _killTriggered) return;

        HandleMovement();
        HandleLocomotion();
        HandleIdleRandomization();
        HandleStalkTeleport();
        TryRandomTeleport();

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
    }

    protected override void HandlePerception()
    {
        var data = (ReaperData)enemyData;
        if (_isAggressive) return;

        bool heardPlayer = canHearPlayer;
        bool timeTriggered = _stalkTimer >= data.aggressionStalkTime;

        if (!heardPlayer || !timeTriggered) return;
        _isAggressive = true;
        animator.SetTrigger(data.aggressiveTrigger);
        animator.SetBool(SpasmSpeed, true);
    }

    protected override void HandleTriggerEnter(Collider other)
    {
        if (playerInTrigger || !other.CompareTag("Player")) return;

        playerInTrigger = true;
        HandlePlayerTriggerEnter();
    }

    private void HandlePlayerTriggerEnter()
    {
        TrySpasm();
        StartCoroutine(DelayedKill());
    }

    public override void takeDamage(int amount)
    {
        if (_killTriggered || _isTeleporting) return;

        var data = (ReaperData)enemyData;
        animator.SetTrigger(data.damageTrigger);
        animator.SetTrigger(data.spasmTrigger);
        animator.SetBool(SpasmSpeed, true);
    }

    private void TriggerSpawn()
    {
        if (_hasSpawned) return;

        var data = (ReaperData)enemyData;

        // Trigger spawn animation
        if (!string.IsNullOrEmpty(data.spawnTrigger))
            animator.SetTrigger(data.spawnTrigger);

        animator.SetBool(HasSpawned, true);

        // Delay activation until animation finishes
        StartCoroutine(DelayedActivation(data.spawnDelay));
    }

    public void TriggerSpawnVFX()
    {
        var data = (ReaperData)enemyData;

        if (data.spawnVFX == null) return;

        var vfx = Instantiate(data.spawnVFX, transform.position, Quaternion.identity);
        vfx.transform.parent = null; // Detach from Reaper hierarchy

        StartCoroutine(CleanupVFX(vfx, data.spawnVFXDuration));
        Debug.Log("Spawn VFX triggered via animation event.");
    }


    private IEnumerator DelayedActivation(float delay)
    {
        yield return new WaitForSeconds(delay);

        agent.enabled = true;
        _hasSpawned = true;
        _isActive = true;
        _isAggressive = false; // Not aggressive yet — just ambient stalking
        Debug.Log("Reaper is now active and stalking.");
    }

    private void HandleMovement()
    {
        if (!_hasSpawned || !agent.isOnNavMesh) return;

        var data = (ReaperData)enemyData;
        _stalkTimer += Time.deltaTime;

        float t = Mathf.Clamp01(_stalkTimer / data.maxStalkTime);
        agent.speed = Mathf.Lerp(data.minSpeed, data.maxSpeed, data.speedRampCurve.Evaluate(t));

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (_isAggressive && playerInTrigger && distanceToPlayer <= data.killDistance)
        {
            TriggerKill();
            return;
        }

        if (_isAggressive && canHearPlayer)
        {
            TeleportIn(false);
            StartCoroutine(DelayedKill());
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(PlayerTransform.position);
        animator.SetBool(IsSpasming, false);

        if (_isAggressive)
            transform.LookAt(PlayerTransform);
    }
    

    private void HandleLocomotion()
    {
        var data = (ReaperData)enemyData;
        var speed = agent.velocity.magnitude;
        var direction = transform.InverseTransformDirection(agent.velocity).x;

        animator.SetFloat(Speed, Mathf.Lerp(animator.GetFloat(Speed), speed, Time.deltaTime * data.animTransSpeed));
        animator.SetFloat(Direction, Mathf.Lerp(animator.GetFloat(Direction), direction, Time.deltaTime * data.animTransSpeed));
    }

    private void HandleIdleRandomization()
    {
        var data = (ReaperData)enemyData;
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
        float total = weights.x + weights.y + weights.z;
        float roll = Random.value * total;

        if (roll < weights.x) return 0;
        return roll < weights.x + weights.y ? 1 : 2;
    }

    private void TryRandomTeleport()
    {
        var data = (ReaperData)enemyData;
        if (_isAggressive || _isTeleporting || playerInTrigger) return;

        _randomTeleportCooldown += Time.deltaTime;
        if (_randomTeleportCooldown < data.randomTeleportInterval) return;

        if (Random.value <= data.randomTeleportChance)
        {
            _randomTeleportCooldown = 0f;
            StartCoroutine(PerformStalkTeleportSequence());
        }
    }

    private void HandleStalkTeleport()
    {
        _stalkTeleportTimer += Time.deltaTime;
        var data = (ReaperData)enemyData;

        if (!_isAggressive || _killTriggered || _isTeleporting || _stalkTeleportTimer < data.stalkTeleportCooldown) return;

        _stalkTeleportTimer = 0f;
        if (Random.value <= data.stalkTeleportChance)
            StartCoroutine(PerformStalkTeleportSequence());
    }

    private IEnumerator PerformStalkTeleportSequence()
    {
        _isTeleporting = true;
        TeleportOut();
        yield return new WaitForSeconds(((ReaperData)enemyData).stalkTeleportDelay);
        TeleportIn(false);
        _isTeleporting = false;
    }

    private void TeleportOut()
    {
    
        model.Equals(false);
        agent.enabled = false;
    }

    private void TeleportIn(bool forKill)
    {
        var data = (ReaperData)enemyData;
        if (!_isAggressive && !forKill) return;

        float t = Mathf.Clamp01(_stalkTimer / data.proximityTightenTime);
        float radius = Mathf.Lerp(data.maxTeleportDistance, data.minTeleportDistance, data.proximityCurve.Evaluate(t));

        Vector3 offset = forKill ? PlayerTransform.forward * 1.5f : Random.insideUnitSphere * radius;
        offset.y = 0f;
        Vector3 targetPos = PlayerTransform.position + offset;

        NavMeshHit hit;
        bool validNavPos = NavMesh.SamplePosition(targetPos, out hit, 2f, NavMesh.AllAreas);
        targetPos = validNavPos ? hit.position : transform.position;

        agent.enabled = true;
        agent.Warp(targetPos);
        transform.position = targetPos;

        if (_isAggressive)
            transform.LookAt(PlayerTransform);

        animator.SetTrigger(data.teleportTrigger);
        gameObject.SetActive(true);
        animator.enabled = true;
    }
    
    public void TriggerTeleportVFX()
    {
        var data = (ReaperData)enemyData;

        if (data.teleportVFX == null) return;

        var vfx = Instantiate(data.teleportVFX, transform.position, Quaternion.identity);
        vfx.transform.parent = null; // Detach from Reaper

        StartCoroutine(CleanupVFX(vfx, data.teleportVFXDuration));
        Debug.Log("Teleport VFX triggered via animation event.");
    }


    private void TriggerKill()
    {
        if (_killTriggered) return;
        _killTriggered = true;
        agent.enabled = false;
        
        var punchType = (ReaperPunchType)Random.Range(0, 3);

        switch (punchType)
        {
            case ReaperPunchType.LeftHand: animator.SetTrigger(Punch1Trigger); break;
            case ReaperPunchType.RightHand: animator.SetTrigger(Punch2Trigger); break;
            case ReaperPunchType.DualHand: animator.SetTrigger(Punch3Trigger); break;
        }
        
    }

    private IEnumerator DelayedKill()
    {
        yield return new WaitForSeconds(1.5f);
        TriggerKill();
    }

    private void TrySpasm()
    {
        if (_hasTriggeredSpasm) return;
        _hasTriggeredSpasm = true;
        _spasmCooldownTimer = 0f;
        StartCoroutine(DelayedSpasm());
    }

    private IEnumerator DelayedSpasm()
    {
        var data = (ReaperData)enemyData;
        yield return new WaitForSeconds(data.spasmDelay);

        int intensity = Random.Range((int)data.spasmIntensityRange.x, (int)data.spasmIntensityRange.y);
        animator.SetInteger(SpasmIntensity, intensity);
        animator.SetTrigger(data.spasmTrigger);
    }

    private void PlayVFX(GameObject prefab, Vector3 position, float duration = 3f)
    {
        if (prefab == null) return;

        var vfx = Instantiate(prefab, position, Quaternion.identity);

        vfx.transform.parent = null; // ✅ Detach from Reaper or any hierarchy
        StartCoroutine(CleanupVFX(vfx, duration));
    }

    private IEnumerator CleanupVFX(GameObject fx, float delay)
    {
        yield return new WaitForSeconds(delay);

        var psList = fx.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in psList)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Destroy(fx);
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
    
    private void OnDrawGizmosSelected()
    {
        var data = (ReaperData)enemyData;
        if (data == null || headPos == null) return;

        // 👂 Hearing radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, data.hearingRadius);

        // 👀 Vision cone
        Gizmos.color = Color.yellow;
        Vector3 forward = headPos.forward;
        float halfFOV = enemyData.FOV / 2f;

        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        Gizmos.DrawRay(headPos.position, leftRayDirection * enemyData.detectionRadius);
        Gizmos.DrawRay(headPos.position, rightRayDirection * enemyData.detectionRadius);

        // 🔴 Forward direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(headPos.position, forward * enemyData.detectionRadius);
    }

}