using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAI : MonoBehaviour, IDamage
{
    [Header("Assigned Data")]
    [SerializeField] protected EnemyData enemyData; // Direct reference to subclassed data

    [Header("References")]
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Renderer model;
    [SerializeField] protected Transform headPos; 
    [SerializeField] public Transform leftHandHitSpawn;
    [SerializeField] public Transform rightHandHitSpawn;


    protected Transform PlayerTransform;
    protected int CurrentHp;
    protected Vector3 SpawnPos;
    private Color _colorOrig;

    protected float AngleToPlayer;

    protected bool canSeePlayer { get; private set; }
    protected bool canHearPlayer { get; private set; }
    protected bool playerInTrigger {  get; set; }

    protected virtual void Awake()
    {
        if (gamemanager.instance != null && gamemanager.instance.player != null)
        {
            PlayerTransform = gamemanager.instance.player.transform;
        }
        else
        {
            // Fallback: try to find by tag
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                PlayerTransform = playerObj.transform;
        }
        
        if (agent != null && animator != null && model != null && headPos != null) return;
        Debug.LogError($"Missing component references on {gameObject.name}");
        enabled = false;
    }

    protected virtual void Start()
    {
        
        if (PlayerTransform == null)
        {
            Debug.LogWarning($"{name} could not find player reference. Disabling AI.");
            enabled = false;
            return;
        }
        
        CurrentHp = enemyData.maxHP;
        SpawnPos = transform.position;
        _colorOrig = model.material.color;
    }

    protected virtual void Update()
    {
        // Kill enemies that fall out of the world
        if (transform.position.y < -50f)
        {
            Destroy(gameObject);
            return;
        }

        // Update perception each frame
        if (PlayerTransform == null) return;
        canSeePlayer = CheckLineOfSight();
        canHearPlayer = CheckHearing();
    }

    protected virtual void HandlePerception()
    {
        
    }
    
    protected virtual void OnPlayerSpotted()
    {}
    
    protected bool CheckLineOfSight()
    {
        if (PlayerTransform == null || headPos == null || agent == null) return false;

        Vector3 dirToPlayer = (PlayerTransform.position - headPos.position).normalized;
        float distanceToPlayer = Vector3.Distance(headPos.position, PlayerTransform.position);
        AngleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        if (AngleToPlayer <= enemyData.FOV / 2f && distanceToPlayer <= enemyData.detectionRadius)
        {
            if (!Physics.Raycast(headPos.position, dirToPlayer, distanceToPlayer, enemyData.lineOfSightMask))
            {
                FaceTarget(PlayerTransform.position);

                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(PlayerTransform.position);
                    agent.stoppingDistance = enemyData.stoppingDist;

                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        // Optional: trigger attack or cinematic logic
                    }
                }

                return true;
            }
        }

        return false;
    }
    
    protected bool CheckHearing()
    {
        if (PlayerTransform == null) return false;

        return gamemanager.instance.CanPlayerBeHeard(
            transform.position,
            enemyData.hearingRadius,
            enemyData.aggressionNoiseThreshold
        );
    }

    private void FaceTarget(Vector3 targetPos)
    {
        var direction = (targetPos - transform.position).normalized;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        var targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * enemyData.faceTargetSpeed);
    }

    public virtual void takeDamage(int amount)
    {
        if (CurrentHp <= 0) return;

        CurrentHp -= amount;
        StartCoroutine(FlashRed());
        agent.SetDestination(gamemanager.instance.player.transform.position);

        if (CurrentHp <= 0)
            OnEnemyDeath();
    }

    protected IEnumerator FlashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = _colorOrig;
    }

    protected virtual void OnEnemyDeath()
    {
        gamemanager.instance.updateGameGoal(-1);

        if (enemyData.keyPrefab)
            Instantiate(enemyData.keyPrefab, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = true;

        HandleTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;

        HandleTriggerExit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        HandleTriggerStay(other);
    }

    protected virtual void HandleTriggerEnter(Collider other) { }
    protected virtual void HandleTriggerExit(Collider other) { }
    protected virtual void HandleTriggerStay(Collider other) { }
}