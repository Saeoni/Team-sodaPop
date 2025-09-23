using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class ReaperAI : MonoBehaviour, IDamage
{

    private enum KillType { TeleportPull, ScytheSlam, Spasm, TeleportGrab }
    private KillType selectedKill;
    private System.Action performKill;

    [Header("Core Components")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [SerializeField] Renderer model;
    [SerializeField] GameObject keyPrefab;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (reaperIsActive) return;

        UpdateTimers();
        stalkingTimer += Time.deltaTime;
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
            case KillType.Spasm:        performKill = SpasmKill;        break;
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

    void TeleportToPlayerOffset(float delaySeconds = 3f, float forwardOffeset = 1.5f)
    {
        StartCoroutine(SpawnTeleport(delaySeconds, forwardOffeset));
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
        pullTimer = 0;

        TeleportToPlayerOffset(3f, pullOffset);

        yield return PlayClip(telepathicClip, 0, 0.1f);
        animator.Play(telepathicLoopClip.name, 0);

        float t = 0f;
        Vector3 start = player.position;
        Vector3 end = transform.position + transform.forward * 1.2f;
        while (t < pullDuration)
        {
            player.position = Vector3.Lerp(start, end, t / pullDuration);
            t += Time.deltaTime;
            yield return null;
        }

        yield return PlayClip(throwCatchClip, 0, 0.2f);
        gamemanager.instance.OnPlayerKilledByReaper();
        yield return new WaitForSeconds(0.5f);
        reaperIsActive= false;
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

        yield return PlayClip(punch3Clip, 0, 0.2f);
        gamemanager.instance.OnPlayerKilledByReaper();
        yield return new WaitForSeconds(0.5f);
        reaperIsActive = false;
    }

    void SpasmKill()
    {
        StartCoroutine(DoSpasmKill());
    }

    IEnumerator DoSpasmKill()
    {
        reaperIsActive = true;
        yield return PlayClip(spasmClip, 2, 0.2f);
        gamemanager.instance.OnPlayerKilledByReaper();
        yield return new WaitForSeconds(0.5f);
        reaperIsActive = false;
    }

    void TeleportGrabKill()
    {
        StartCoroutine(DoTeleportGrabKill());
    }

    IEnumerator DoTeleportGrabKill()
    {
        reaperIsActive = true;

        TeleportToPlayerOffset(3f, 0f);
        yield return new WaitForSeconds(3f);

        yield return PlayClip(comeOut1Clip, 2, 0.1f);
        yield return PlayClip(comeOut2Clip, 2, 0.1f);
        yield return PlayClip(telepathicClip, 0, 0.1f);
        yield return new WaitForSeconds(grabHoldTime);
        yield return PlayClip(comeOut1Clip);
        yield return PlayClip(comeOut2Clip);

        gamemanager.instance.OnPlayerKilledByReaper();
        yield return new WaitForSeconds(0.5f);
        reaperIsActive = false;
    }
    public void takeDamage(int amount)
    {

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
}
