using System.Collections;
using UnityEngine;


public class ReaperAI : EnemyAI
{
    private static readonly int HasSpawned = Animator.StringToHash("hasSpawned");
    private static readonly int IsSpasming = Animator.StringToHash("IsSpasming");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Direction = Animator.StringToHash("Direction");
    private static readonly int SpasmIntensity = Animator.StringToHash("SpasmIntensity");
    private static readonly int SpasmSpeed = Animator.StringToHash("AggressiveState");
    
    private bool _isActive;
    private bool _killTriggered;
    private bool _hasTriggeredSpasm;
    private bool _isAggressive;

    private float _spasmCooldownTimer;
    private float _stalkTimer;

    protected override void Awake()
    {
        if (PlayerInTrigger)
        {
            TrySpasm();
        }
        
    }

    protected override void Start()
    {
        HandleMovement();
        base.Update();
        base.Start();
        agent.enabled = true;
        _isActive = true;
        _stalkTimer = 0f;

        if(enemyData.spawnVFX != null)
            Instantiate(enemyData.spawnVFX, transform.position, Quaternion.identity);
        Destroy(gameObject, 3f);
        

        animator.SetBool(HasSpawned, true);
    }

    protected override void Update()
    {
        if (!_isActive || _killTriggered) return;

        HandleLocomotion();


        if (!_hasTriggeredSpasm) return;
        _spasmCooldownTimer += Time.deltaTime;
        var data = (ReaperData)enemyData;

        if (!(_spasmCooldownTimer >= data.spasmCooldown)) return;
        _hasTriggeredSpasm = false;
        _spasmCooldownTimer = 0f;
    }

    protected override void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("Reaper senses the player... preparing to spasm");
        TrySpasm();
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
        var noiseTriggered = gamemanager.instance.noiseLevel >= data.aggressionNoiseThreshold;
        var timeTriggered = gamemanager.instance.noiseLevel >= data.aggressionStalkTime;

        if (!noiseTriggered && !timeTriggered) return;
        _isAggressive =  true;
        animator.SetTrigger(data.aggressiveTrigger);
        animator.SetBool(null, true);
    }

   

    private void HandleMovement()
    {
        var data = (ReaperData)enemyData;
        _stalkTimer += Time.deltaTime;

        var t = Mathf.Clamp01(_stalkTimer / data.maxStalkTime);
        agent.speed = Mathf.Lerp(data.minSpeed, data.maxSpeed, data.speedRampCurve.Evaluate(t));

        var distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        // Call OnPlayerSpotted if stalking has escalated
        if (!_isAggressive && _stalkTimer >= data.maxStalkTime * 0.5f)
        {
            OnPlayerSpotted();
        }

        switch (PlayerInTrigger)
        {
            case true when _stalkTimer >= data.maxStalkTime * 0.75f && distanceToPlayer <= data.killDistance:
                TriggerKill();
                return;
            case false:
                agent.isStopped = false;
                agent.SetDestination(PlayerTransform.position);
                animator.SetBool(IsSpasming, false);
                break;
            default:
                agent.isStopped = true;
                agent.ResetPath();
                animator.SetBool(IsSpasming, true);
                break;
        }

        transform.LookAt(PlayerTransform);

        if (_stalkTimer >= data.maxStalkTime)
        {
            TriggerKill();
        }

        if (!(gamemanager.instance.noiseLevel >= gamemanager.instance.noiseThreshold)) return;
        TeleportToPlayer();
        StartCoroutine(DelayedKill());
    }

    private void HandleLocomotion()
    {
        var data = (ReaperData)enemyData;

        var speed = agent.velocity.magnitude;
        var localVelocity = transform.InverseTransformDirection(agent.velocity);
        var direction = localVelocity.x;

        animator.SetFloat(Speed, Mathf.Lerp(animator.GetFloat(Speed), speed, Time.deltaTime * data.animTransSpeed));
        animator.SetFloat(Direction, Mathf.Lerp(animator.GetFloat(Direction), direction, Time.deltaTime * data.animTransSpeed));
    }
    
    protected override void OnPlayerSpotted()
    {
        var data = (ReaperData)enemyData;

        if (!_isAggressive)
        {
            // Trigger cinematic spasm and enter aggressive state
            animator.SetTrigger(data.spasmTrigger);
            animator.SetBool(SpasmSpeed, true);
            _isAggressive = true;
        }

        // Begin chasing the player
        agent.speed = data.maxSpeed;
        agent.SetDestination(PlayerTransform.position);

        // Stop if within striking distance
        if (!(agent.remainingDistance <= data.stoppingDist)) return;
        agent.ResetPath();
    }

    private void TriggerKill()
    {
        var data = (ReaperData)enemyData;
        _killTriggered = true;
        animator.SetTrigger(data.killTrigger);
        gamemanager.instance.youLose();
    }

    private void TeleportToPlayer()
    {
        var data = (ReaperData)enemyData;
        var player = gamemanager.instance.player.transform;
        var offset = player.forward * -1.5f;
        var targetPos = player.position + offset;

        if (data.teleportVFX)
            Instantiate(data.teleportVFX, transform.position, Quaternion.identity);

        transform.position = targetPos;
        transform.LookAt(player);
        agent.Warp(targetPos);
    }

    private IEnumerator DelayedKill()
    {
        yield return new WaitForSeconds(1.5f);
        TriggerKill();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator DelayedSpasm()
    {
        var data = (ReaperData)enemyData;
        yield return new WaitForSeconds(data.spasmDelay);

        var intensity = Random.Range((int)data.spasmIntensityRange.x, (int)data.spasmIntensityRange.y);
        animator.SetInteger(SpasmIntensity, intensity);
        animator.SetTrigger(data.spasmTrigger);

        Debug.Log($"Spasm triggered with intensity {intensity}");
    }

    public override void takeDamage(int amount)
    {
        if (CurrentHp <= 0) return;

        CurrentHp -= amount;
        var data = (ReaperData)enemyData;
        animator.SetTrigger(data.damageTrigger);
        StartCoroutine(FlashRed());

        if (CurrentHp <= 0)
            base.OnEnemyDeath();
    }
}