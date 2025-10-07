using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class EnemyAI : MonoBehaviour, IDamage
{
    public EnemyData enemyData;

    [Header("Core Components")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Animator animator;
    [SerializeField] Transform headPos;
    [SerializeField] GameObject spawnVFX;
    [SerializeField] bool isPatrolling = false;

    [Header("Reaper Logic")]
    [SerializeField] bool isActive = false;
    [SerializeField] float stalkTimer = 0f;
    bool _killTriggered = false;
    bool canSpasm = true;

    [Header("Spasm Behavior")]
    [SerializeField] float spasmDistance = 10f;
    [SerializeField] float spasmCooldown = 5f;


    float roamTimer;
    float originalStopDist;
    float angleToPlayer;
    bool playerInTrigger;
    Color colorOrig;
    Vector3 playerDir;
    Vector3 spawnPos;
    int currentHP;

    int patrolIndex = 0;
    float patrolPauseTimer = 0f;
    bool hasSpasmed = false;
    float spasmTimer = 0f;

    void Start()
    {
        colorOrig = model.material.color;
        spawnPos = transform.position;
        originalStopDist = agent.stoppingDistance;
        currentHP = enemyData.maxHP;

        

        if (spawnVFX != null )
        {
            spawnVFX.SetActive(true);
            Destroy(spawnVFX, 4f);
        }

        if (enemyData.canPatrol && enemyData.patrolPoints.Length > 0)
        {
            isPatrolling = true;
            agent.SetDestination(enemyData.patrolPoints[patrolIndex].position);
        }
    }

    // Update is called once per frame
    void Update()
    {
      
        UpdatedLocomotionAnim();

        switch (enemyData.EnemyType)
        {
            case EnemyType.Reaper:
                if (isActive) HandleReaperMovement(); 
                break;
            default:
                HandleDefaultMovement(); 
            break;
        }
    }

    void UpdatedLocomotionAnim()
    {
        float currSpeed = agent.velocity.magnitude;
        float normalIzedSpeed = Mathf.Clamp01(currSpeed / agent.speed);
        float currAnimSpeed = animator.GetFloat("Speed");

        animator.SetFloat("Speed", Mathf.Lerp(currAnimSpeed, normalIzedSpeed, Time.deltaTime * enemyData.animTransSpeed));

        // Calculate turning direction
        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float direction = Mathf.Clamp(localVelocity.x, -1f, 1f);
        animator.SetFloat("Direction", Mathf.Lerp(animator.GetFloat("Direction"), direction, Time.deltaTime * enemyData.animTransSpeed));

        // Switch stance based on stalking phase
        bool isReady = stalkTimer >= enemyData.maxStalkTime * 0.5f;
        animator.SetBool("IsReady", isReady);
    }

    void HandleReaperMovement()
    {
        if (_killTriggered) return;

        stalkTimer += Time.deltaTime;

        float t = Mathf.Clamp01(stalkTimer / enemyData.maxStalkTime);
        float rampSpeed = enemyData.speedRampCurve != null ? enemyData.speedRampCurve.Evaluate(t) : t;
        float currentSpeed = Mathf.Lerp(enemyData.minSpeed, enemyData.maxSpeed, rampSpeed);
        agent.speed = currentSpeed;

        Transform player = gamemanager.instance.player.transform;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= enemyData.killDistance && stalkTimer >= enemyData.maxStalkTime * 0.75f)
        {
            TriggerReaperKill();
            return;
        }

        if (hasSpasmed)
            spasmTimer += Time.deltaTime;

        if (distanceToPlayer > spasmDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("IsSpasming", false);

            if (hasSpasmed && spasmTimer >= spasmCooldown)
            {
                hasSpasmed = false;
                spasmTimer = 0f;
            }
        }
        else if (!hasSpasmed)
        {
            agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool("IsSpasming", true);
            hasSpasmed = true;
            spasmTimer = 0f;
        }

        transform.LookAt(player);
        Debug.DrawRay(headPos.position, player.position - headPos.position);

        if (stalkTimer >= enemyData.maxStalkTime)
        {
            TriggerReaperKill();
        }

        if (gamemanager.instance.noiseLevel >= gamemanager.instance.noiseThreshold)
        {
            TeleportToPlayer();
            StartCoroutine(DelayedKill());
            return;
        }
    }

    void TeleportToPlayer()
    {
        Transform player = gamemanager.instance.player.transform;

        if (enemyData.teleportVFX != null)
            Instantiate(enemyData.teleportVFX, transform.position, Quaternion.identity);

        Vector3 offset = player.forward * -1.5f;
        Vector3 targetPos = player.position + offset;
        agent.Warp(targetPos);
        transform.LookAt(player);

        animator.SetTrigger(enemyData.teleportTrigger); // Reuses spawn animation
    }

    IEnumerator DelayedKill()
    {
        yield return new WaitForSeconds(1.5f); // Match ComeOut2 length
        TriggerReaperKill();
    }

    void TriggerReaperKill()
    {
        _killTriggered = true;
        canSpasm = false;   
        animator.SetTrigger(enemyData.killTrigger);
        gamemanager.instance.OnPlayerKilledByReaper();
    }

    public void OnSpawnFinish()
    {
        agent.enabled = true;
        isActive = true;
        stalkTimer = 0f;

        animator.SetBool("hasSpawned", true);
        StartCoroutine(StalkSpasmRoutine());    
    }

    void HandleDefaultMovement()
    {
        if (playerInTrigger && !CanSeePlayer())
        {
            RoamAndPatrolCheck();
        }
        else if (!playerInTrigger)
        {
            RoamAndPatrolCheck();
        }
    }

    bool CanSeePlayer()
    {
        if (gamemanager.instance.isStealthed)
            return false;

        playerDir = gamemanager.instance.player.transform.position - headPos.position;
        Vector3 playerPos = gamemanager.instance.player.transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit rayHit;
        if (Physics.Raycast(headPos.position, playerDir, out rayHit, enemyData.detectionRadius, enemyData.lineOfSightMask))
        {
            if (angleToPlayer <= enemyData.FOV && rayHit.collider.CompareTag("Player"))
            {
                agent.SetDestination(playerPos);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    FaceTarget();
                }

                agent.stoppingDistance = originalStopDist;
                return true;
            }
        }

        agent.stoppingDistance = 0;
        return false;
    }

    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * enemyData.faceTargetSpeed);
    }

  
    void RoamAndPatrolCheck()
    {
        if (enemyData.canPatrol && enemyData.patrolPoints.Length > 0)
        {
            HandlePatrol();
            return;
        }

        roamTimer += Time.deltaTime;
        if (roamTimer >= enemyData.roamPauseTimer && agent.remainingDistance <= 0.01f)
        {
            BeginRoam();
        }
    }

    void BeginRoam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        NavMeshHit meshHit;
        Vector3 randomPos = Random.insideUnitSphere * enemyData.roamDist + spawnPos;
        NavMesh.SamplePosition(randomPos, out meshHit, enemyData.roamDist, 1);
        agent.SetDestination(meshHit.position);
    }

    void HandlePatrol()
    {
    
        if (agent.remainingDistance <= 0.1f)
        {
            patrolPauseTimer += Time.deltaTime;

            if (patrolPauseTimer >= enemyData.patrolPauseTime)
            {
                patrolPauseTimer = 0f;
                patrolIndex++;

                if (patrolIndex >= enemyData.patrolPoints.Length)
                {
                    patrolIndex = enemyData.loopPatrol ? 0 : enemyData.patrolPoints.Length - 1;
                }

                agent.SetDestination(enemyData.patrolPoints[patrolIndex].position);
            }
        }    
    }

    public void takeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP -= amount;
        StartCoroutine(FlashRed());
        agent.SetDestination(gamemanager.instance.player.transform.position);

        animator.SetTrigger(enemyData.damageTrigger);
        if (currentHP <= 0)
            OnEnemyDeath();
    }

    IEnumerator StalkSpasmRoutine()
    {
        while (isActive && !_killTriggered)
        {

            if (canSpasm)
            {
                animator.SetTrigger(enemyData.spasmTrigger);
            }

            float delay = Random.Range(4f, 10f);
            yield return new WaitForSeconds(delay);  
        }
    }

    IEnumerator FlashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    void OnEnemyDeath()
    {
        gamemanager.instance.updateGameGoal(-1);
        
        if (enemyData.keyPrefab  != null) 
            Instantiate(enemyData.keyPrefab, transform.position, Quaternion.identity);

        if (enemyData.deathVFX != null)
            Instantiate(enemyData.deathVFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            agent.stoppingDistance = 0f;
        }
    }
}