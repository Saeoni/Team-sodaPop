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
    bool isActive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         startingPos = transform.position;
        StartCoroutine(InitializeReaper());
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive) return;

        UpdateTimers();
        stalkingTimer += Time.deltaTime;
    }

    // Reaper Initialization
    IEnumerator InitializeReaper()
    {
        player = gamemanager.instance.player.transform;
        originalColor = model.material.color;
        agent.speed = baseSpeed;
        whisperTimer = whisperIntervalMax;
        isActive = true;

        // Spawn animation sequence
        animator.Play(comeOut1Clip.name);
        yield return new WaitForSeconds(comeOut1Clip.name.Length);

        animator.Play(comeOut2Clip.name);
        yield return new WaitForSeconds(comeOut2Clip.name.Length);

        animator.Play(spasmClip.name);
        yield return new WaitForSeconds(spasmClip.name.Length);

        ChooseKillMethod();
        isActive = false;
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
        float velocity = agent.velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(velocity / maxSpeed);
        animator.SetFloat("Speed", normalizedSpeed);
    }

    

    void TeleportPullKill()
    {

    }

    void ScytheSlamKill()
    {

    }

    void SpasmKill()
    {

    }

    void TeleportGrabKill()
    {

    }

    public void takeDamage(int amount)
    {

    }
}
