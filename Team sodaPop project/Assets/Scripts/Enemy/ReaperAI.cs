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
    
    private bool isActive;
    private bool killTriggered;
    private bool hasTriggeredSpasm;
    private bool isAggressive;

    private float spasmCooldownTimer;
    private float stalkTimer;

    protected override void Awake()
    {
        if (PlayerInTrigger)
        {
            TrySpasm();
        }
        
    }

    protected override void Start()
    {
        base.Update();
        base.Start();
        agent.enabled = true;
        isActive = true;
        stalkTimer = 0f;

        animator.SetBool(HasSpawned, true);
    }

    protected override void Update()
    {
        if (!isActive || killTriggered) return;

        HandleMovement();
        HandleLocomotion();


        if (!hasTriggeredSpasm) return;
        spasmCooldownTimer += Time.deltaTime;
        var data = (ReaperData)enemyData;

        if (!(spasmCooldownTimer >= data.spasmCooldown)) return;
        hasTriggeredSpasm = false;
        spasmCooldownTimer = 0f;
    }

    protected override void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("Reaper senses the player... preparing to spasm");
        TrySpasm();
    }

    private void TrySpasm()
    {
        if (hasTriggeredSpasm) return;

        hasTriggeredSpasm = true;
        spasmCooldownTimer = 0f;
        StartCoroutine(DelayedSpasm());
    }

    private void CheckAggression()
    {
        if (isAggressive) return;

        var data = (ReaperData)enemyData;
        var noiseTriggered = Gamemanager.Instance.noiseLevel >= data.aggressionNoiseThreshold;
        var timeTriggered = Gamemanager.Instance.noiseLevel >= data.aggressionStalkTime;

        if (!noiseTriggered && !timeTriggered) return;
        isAggressive =  true;
        animator.SetTrigger(data.aggressiveTrigger);
        animator.SetBool(AggressiveState, true);
    }

    public string AggressiveState { get; set; }

    private void HandleMovement()
    {
        var data = (ReaperData)enemyData;
        stalkTimer += Time.deltaTime;

        var t = Mathf.Clamp01(stalkTimer / data.maxStalkTime);
        agent.speed = Mathf.Lerp(data.minSpeed, data.maxSpeed, data.speedRampCurve.Evaluate(t));

        var player = Gamemanager.Instance.player.transform;

        if (PlayerInTrigger && stalkTimer >= data.maxStalkTime * 0.75f)
        {
            var distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= data.killDistance)
            {
                TriggerKill();
                return;
            }
        }

        if (!PlayerInTrigger)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool(IsSpasming, false);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool(IsSpasming, true);
        }

        transform.LookAt(player);

        if (stalkTimer >= data.maxStalkTime)
        {
            TriggerKill();
        }

        if (!(Gamemanager.Instance.noiseLevel >= Gamemanager.Instance.noiseThreshold)) return;
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

    private void TriggerKill()
    {
        var data = (ReaperData)enemyData;
        killTriggered = true;
        animator.SetTrigger(data.killTrigger);
        Gamemanager.Instance.YouLose();
    }

    private void TeleportToPlayer()
    {
        var data = (ReaperData)enemyData;
        var player = Gamemanager.Instance.player.transform;
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