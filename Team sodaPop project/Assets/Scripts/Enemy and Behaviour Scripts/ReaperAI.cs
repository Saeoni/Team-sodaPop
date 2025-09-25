using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class ReaperAI : MonoBehaviour
{

    private enum KillType { TeleportPull, ScytheSlam, Spasm, TeleportGrab }
    private KillType selectedKill;
    private System.Action performKill;

    [Header("Core Components")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [SerializeField] Renderer model;
    [SerializeField] GameObject keyPrefab;
    [SerializeField] GameObject scytheCollider;
    [SerializeField] GameObject reaperScythe;
    [SerializeField] GameObject flyingScythePrefab;
    [SerializeField] Transform leftHandSocket;
    [SerializeField] GameObject player_;

    [HideInInspector] public Scythe_Projectile activeProjectile;

    [Header("Movement, HP, & DMG Settings")]
    [SerializeField] float baseSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float rampRate;
    [SerializeField] int damage = 999;
    [SerializeField] int HP = 200;

    [Header("Whisper Settings")]
    [SerializeField] AudioClip whisperClip;
    [SerializeField] float whisperIntervalMin = 5f;
    [SerializeField] float whisperIntervalMax = 15f;

    [Header("Locomotion Clips")]
    [SerializeField] AnimationClip walk1Clip;
    [SerializeField] AnimationClip runClip;
    [SerializeField] AnimationClip jumpLongClip;
    [SerializeField] AnimationClip jumpShort;
    [SerializeField] AnimationClip turnRight;
    [SerializeField] AnimationClip turnLeft;
    [SerializeField] AnimationClip[] idle;
 
    [Header("Attack Settings")]
    [SerializeField] float killRange;
    [SerializeField] float meleCooldown;
    [SerializeField] float pullCooldown;
    [SerializeField] float pullOffset;
    [SerializeField] float pullWindup;
    [SerializeField] float pullDuration;
    [SerializeField] float grabHoldTime;

    [Header("Exit Zone Settings")]
    [SerializeField] string exitZoneTag = "ExitZone";
    [SerializeField] Vector3 exitSpawnOffset = new Vector3(0, 0, 2f);
    [SerializeField] AnimationClip getDamageClip;
    [SerializeField] AnimationClip deathClip;

    [Header("Kill Method Clips")]
    [SerializeField] AnimationClip comeOut1Clip;
    [SerializeField] AnimationClip comeOut2Clip;
    [SerializeField] AnimationClip telepathicClip;
    [SerializeField] AnimationClip telepathicLoopClip;
    [SerializeField] AnimationClip throwClip;
    [SerializeField] AnimationClip throwCatchClip;
    [SerializeField] AnimationClip punch3Clip;
    [SerializeField] AnimationClip spasmClip;

    [Header("Stalking Behaviour")]
    [SerializeField] float stalkinDuration = 30f;
    [SerializeField] AnimationClip walk2Clip;
    float stalkingTimer;
    bool hasTeleported;
    Vector3 startingPos;

    Transform player;
    Color originalColor;
    float meleTimer;
    float pullTimer;
    float whisperTimer;
    bool reaperIsActive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         startingPos = transform.position;
        player = gamemanager.instance.player.transform;
        StartCoroutine(InitializeReaper());
        player_ = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (reaperIsActive) return;

        UpdateTimers();
        stalkingTimer += Time.deltaTime;
        StalkPlayerGradually();
        UpdateLocomotion();
        TryTriggerKill();
    }

    // Reaper Initialization
    IEnumerator InitializeReaper()
    {
        reaperIsActive = true;

        yield return PlayClip(comeOut1Clip, 2, 0.1f);
        yield return PlayClip(comeOut2Clip, 2, 0.1f);
        yield return PlayClip(spasmClip, 2, 0.2f);

        reaperIsActive = false;
    }

    void ChooseKillMethod()
    {
        var types = Enum.GetValues(typeof(KillType));
        selectedKill = (KillType)types.GetValue(UnityEngine.Random.Range(0, types.Length));

        switch (selectedKill)
        {
            case KillType.TeleportPull: performKill = TeleportPullKill; break;
            case KillType.ScytheSlam:   performKill = ScytheSlamKill;   break;
            case KillType.TeleportGrab: performKill = TeleportGrabKill; break;
        }
    }

    void StalkPlayerGradually()
    {
        float t = stalkingTimer / stalkinDuration;
        Vector3 targetPos = Vector3.Lerp(startingPos, player.position, t);
        agent.SetDestination(targetPos);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float normalizedSpeed = Mathf.Clamp01(agent.velocity.magnitude / agent.speed);

        animator.SetFloat("Speed", normalizedSpeed);
    }

    void UpdateTimers()
    {
        meleTimer += Time.deltaTime;
        pullTimer += Time.deltaTime;
        whisperTimer -= Time.deltaTime;
    }

    void UpdateLocomotion()
    {
        if (reaperIsActive) return;

        float velocity = agent.velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(velocity / maxSpeed);
        animator.SetFloat("Speed", normalizedSpeed);
    }

    IEnumerator SpawnTeleport(float delaySeconds, float forwardOffeset)
    {
        reaperIsActive = true;
        model.enabled = false;
        agent.isStopped = true;

        yield return new WaitForSeconds(delaySeconds);

        Vector3 offsetPos = player.position + transform.forward * forwardOffeset;
        agent.Warp(offsetPos);

        model.enabled = true;
        agent.isStopped = false;

        yield return PlayClip(comeOut1Clip, 2, 0.1f);
        yield return PlayClip(comeOut2Clip, 2, 0.1f);

        reaperIsActive = false; 
    }

    void TryTriggerKill()
    {
        if (Vector3.Distance(transform.position, player.position) <= killRange)
        {
            performKill?.Invoke();
            
            
        }
    }

    void TeleportPullKill()
    {
        if (pullTimer < pullCooldown) return;
        StartCoroutine(DoTeleportPullKill());
    }

    IEnumerator DoTeleportPullKill()
    {
        reaperIsActive = true;
        pullTimer = 0f;

        // Step 1: Teleport near player
        yield return SpawnTeleport(3f, pullOffset);

        yield return PlayClip(throwClip, 0, 0.1f);
        
        animator.Play(throwCatchClip.name, 0);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => activeProjectile != null && activeProjectile.pullComplete);   
    }

    void ScytheSlamKill()
    {
        if (meleTimer < meleCooldown) return;
        StartCoroutine(DoScytheSlamKill());
    }

    IEnumerator DoScytheSlamKill()
    {
        reaperIsActive = true;
        meleTimer = 0f;

        agent.SetDestination(player.position);
        while (Vector3.Distance(transform.position, player.position) > 1.5f)
        {
            yield return null;
        }
        
        agent.isStopped = true;
        transform.LookAt(player);
        animator.Play(punch3Clip.name, 0);
        yield return new WaitForSeconds(0.5f);   
    }

    void TeleportGrabKill()
    {
        StartCoroutine(DoTeleportGrabKill());
    }

    IEnumerator DoTeleportGrabKill()
    {
        reaperIsActive = true;

        yield return SpawnTeleport(3f, 0f);
        yield return new WaitForSeconds(3f);
        yield return PlayClip(telepathicClip, 0, 0.1f);
        yield return PlayClip(telepathicLoopClip, 0, 0.1f);
        yield return new WaitForSeconds(grabHoldTime);
        StartCoroutine(ExitTeleportWithPlayer());
         
    }
 
    IEnumerator PlayClip(AnimationClip clip, int layer = 0, float buffer = 0f)
    {
        if (clip == null)
        {
            Debug.LogWarning("Null clip passed to PlayClip.");
            yield break;
        }

        animator.Play(clip.name, layer);
        yield return new WaitForSeconds(clip.length + buffer);
    }
    public IEnumerator ExitTeleportWithPlayer(float delaySeconds = 0.5f, float backwardOffset = 3f)
    {
        reaperIsActive = true;

        // Disable player control
        var pc = player.GetComponent<playerController>();
        if (pc != null) pc.enabled = false;

        // Optional delay before disappearing
        yield return new WaitForSeconds(delaySeconds);

        // Hide visuals
        model.enabled = false;
        agent.isStopped = true;

        // Calculate exit position
        Vector3 offset = -transform.forward * backwardOffset;
        Vector3 exitPos = transform.position + offset;
        exitPos.y = transform.position.y;

        // Move both Reaper and player
        transform.position = exitPos;
        player.position = exitPos;

        // Optional: Parent player to Reaper for dramatic exit
        player.SetParent(transform);
        player.LookAt(transform);

        // Optional: wait before fade or scene transition
        yield return new WaitForSeconds(0.5f);

        FinalizeKill();
    }

    public void EnableScytheCollider() => scytheCollider.SetActive(true);
    public void DisableScytheCollider() => scytheCollider.SetActive(false);
    public void HideScythe() => reaperScythe.SetActive(false);
    public void ShowScythe() => reaperScythe.SetActive(true);

    public GameObject GetFlyingScythePrefab()
    {
        return flyingScythePrefab;
    }

    public void GrabPlayerToHand()
    {
        if (leftHandSocket == null || player == null) return;
        
        var pc = player.GetComponent<playerController>();
        if (pc != null) pc.enabled = false;

        player.SetParent(null); // unparent to avoid snapping
        StartCoroutine(PullPlayerToHandThenGrab());
    }

    IEnumerator PullPlayerToHandThenGrab()
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = player.position;
        Vector3 targetPos = leftHandSocket.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            player.position = Vector3.Lerp(startPos, targetPos, t);
            player.LookAt(transform);
            yield return null;
        }

        // Final grab and lock
        player.SetParent(leftHandSocket);
        player.position = leftHandSocket.position;
    }

    public void FinalizeKill()
    {
        gamemanager.instance.OnPlayerKilledByReaper();

        reaperIsActive = false;
    }
}
