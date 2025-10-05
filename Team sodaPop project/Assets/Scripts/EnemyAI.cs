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
    [SerializeField] bool _isPatrolling = false;

    [Header("Reaper Logic")]
    [SerializeField] bool _isActive = false;
    [SerializeField] float _stalkTimer = 0f;
    bool _killTriggered = false;

    [Header("Spasm Behavior")]
    [SerializeField] AudioSource spasmAudio;
    //[SerializeField] CameraShake cameraShake;
    [SerializeField] float spasmDistance = 10f;
    [SerializeField] float spasmCooldown = 5f;


    float _roamTimer;
    float _originalStopDist;
    float _angleToPlayer;
    bool _playerInTrigger;
    Color _colorOrig;
    Vector3 _playerDir;
    Vector3 _spawnPos;
    int _currentHP;

    int _patrolIndex = 0;
    float _patrolPauseTimer = 0f;
    bool _hasSpasmed = false;
    float _spasmTimer = 0f;

    void Start()
    {
        _colorOrig = model.material.color;
        _spawnPos = transform.position;
        _originalStopDist = agent.stoppingDistance;
        _currentHP = enemyData.maxHP;

        

        if (spawnVFX != null )
        {
            spawnVFX.SetActive(true);
            Destroy(spawnVFX, 5f);
        }

        if (enemyData.canPatrol && enemyData.patrolPoints.Length > 0)
        {
            _isPatrolling = true;
            agent.SetDestination(enemyData.patrolPoints[_patrolIndex].position);
        }
    }

    // Update is called once per frame
    void Update()
    {
      
        UpdatedLocomotionAnim();

        switch (enemyData.EnemyType)
        {
            case EnemyType.Reaper:
                if (_isActive) HandleReaperMovement(); 
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
        bool isReady = _stalkTimer >= enemyData.maxStalkTime * 0.5f;
        animator.SetBool("IsReady", isReady);
    }

    void HandleReaperMovement()
    {
        if (_killTriggered) return;

        _stalkTimer += Time.deltaTime;

        float t = Mathf.Clamp01(_stalkTimer / enemyData.maxStalkTime);
        float currentSpeed = Mathf.Lerp(enemyData.minSpeed, enemyData.maxSpeed, t);
        agent.speed = currentSpeed;

        Transform player = gamemanager.instance.player.transform;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (_hasSpasmed)
            _spasmTimer += Time.deltaTime;

        if (distanceToPlayer > spasmDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("IsSpasming", false);

            if (_hasSpasmed && _spasmTimer >= spasmCooldown)
            {
                _hasSpasmed = false;
                _spasmTimer = 0f;
            }
        }
        else if (!_hasSpasmed)
        {
            agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool("IsSpasming", true);
            _hasSpasmed = true;
            _spasmTimer = 0f;

            if (spasmAudio != null && !spasmAudio.isPlaying)
                spasmAudio.Play();

           // if (cameraShake != null)
              //  cameraShake.Shake(0.5f, 0.3f);
        }

        transform.LookAt(player);
        Debug.DrawRay(headPos.position, player.position - headPos.position);

        if (_stalkTimer >= enemyData.maxStalkTime)
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
        animator.SetTrigger(enemyData.killTrigger);
        gamemanager.instance.OnPlayerKilledByReaper();
    }

    public void OnSpawnFinish()
    {
        agent.enabled = true;
        _isActive = true;
        _stalkTimer = 0f;

        animator.SetBool("hasSpawned", true);
    }

    void HandleDefaultMovement()
    {
        if (_playerInTrigger && !CanSeePlayer())
        {
            RoamAndPatrolCheck();
        }
        else if (!_playerInTrigger)
        {
            RoamAndPatrolCheck();
        }
    }

    bool CanSeePlayer()
    {
        if (gamemanager.instance.isStealthed)
            return false;

        _playerDir = gamemanager.instance.player.transform.position - headPos.position;
        Vector3 playerPos = gamemanager.instance.player.transform.position;
        _angleToPlayer = Vector3.Angle(_playerDir, transform.forward);

        Debug.DrawRay(headPos.position, _playerDir);

        RaycastHit rayHit;
        if (Physics.Raycast(headPos.position, _playerDir, out rayHit, enemyData.detectionRadius, enemyData.lineOfSightMask))
        {
            if (_angleToPlayer <= enemyData.FOV && rayHit.collider.CompareTag("Player"))
            {
                agent.SetDestination(playerPos);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    FaceTarget();
                }

                agent.stoppingDistance = _originalStopDist;
                return true;
            }
        }

        agent.stoppingDistance = 0;
        return false;
    }

    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(_playerDir.x, 0, _playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * enemyData.faceTargetSpeed);
    }

  
    void RoamAndPatrolCheck()
    {
        if (enemyData.canPatrol && enemyData.patrolPoints.Length > 0)
        {
            HandlePatrol();
            return;
        }

        _roamTimer += Time.deltaTime;
        if (_roamTimer >= enemyData.roamPauseTimer && agent.remainingDistance <= 0.01f)
        {
            BeginRoam();
        }
    }

    void BeginRoam()
    {
        _roamTimer = 0;
        agent.stoppingDistance = 0;

        NavMeshHit meshHit;
        Vector3 randomPos = Random.insideUnitSphere * enemyData.roamDist + _spawnPos;
        NavMesh.SamplePosition(randomPos, out meshHit, enemyData.roamDist, 1);
        agent.SetDestination(meshHit.position);
    }

    void HandlePatrol()
    {
    
        if (agent.remainingDistance <= 0.1f)
        {
            _patrolPauseTimer += Time.deltaTime;

            if (_patrolPauseTimer >= enemyData.patrolPauseTime)
            {
                _patrolPauseTimer = 0f;
                _patrolIndex++;

                if (_patrolIndex >= enemyData.patrolPoints.Length)
                {
                    _patrolIndex = enemyData.loopPatrol ? 0 : enemyData.patrolPoints.Length - 1;
                }

                agent.SetDestination(enemyData.patrolPoints[_patrolIndex].position);
            }
        }    
    }

    public void takeDamage(int amount)
    {
        if (_currentHP <= 0) return;

        _currentHP -= amount;
        StartCoroutine(FlashRed());
        agent.SetDestination(gamemanager.instance.player.transform.position);

        animator.SetTrigger(enemyData.damageTrigger);
        if (_currentHP <= 0)
            OnEnemyDeath();
    }

    IEnumerator StalkSpasmRoutine()
    {
        while (_isActive && !_killTriggered)
        {
            float delay = Random.Range(4f, 10f);
            yield return new WaitForSeconds(delay);

            animator.SetTrigger(enemyData.spasmTrigger);
        }
    }

    IEnumerator FlashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = _colorOrig;
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
            _playerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = false;
            agent.stoppingDistance = 0f;
        }
    }
}