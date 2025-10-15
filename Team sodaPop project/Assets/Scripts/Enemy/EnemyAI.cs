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

    protected int CurrentHp;
    protected Vector3 SpawnPos;
    private Color _colorOrig;

    protected bool PlayerInTrigger;
    private protected bool CanSeePlayer;
    private float _angleToPlayer;
    protected Vector3 PlayerDir;
    // ReSharper disable once UnassignedField.Global
    protected Transform PlayerTransform;

    protected virtual void Awake()
    {
        if (agent != null && animator != null && model != null && headPos != null) return;
        Debug.LogError($"Missing component references on {gameObject.name}");
        enabled = false;
    }

    protected virtual void Start()
    {
        CheckLineOfSight();
        Debug.LogWarning($"{gameObject.name} fell out of bounds.");
       
        CurrentHp = enemyData.maxHP;
        SpawnPos = transform.position;
        _colorOrig = model.material.color;

        PlaySpawnVFX();
    }

    protected virtual void Update()
    {
        if (transform.position.y < -50f)
        {
            Destroy(gameObject);
        }
    }

    private void PlaySpawnVFX()
    {
        if (enemyData.spawnVFX != null)
            Instantiate(enemyData.spawnVFX, transform.position, Quaternion.identity);
    }

    private protected virtual void CheckLineOfSight()
    {
        var dirToPlayer = (PlayerTransform.position - headPos.position).normalized;
        var distanceToPlayer = Vector3.Distance(headPos.position, PlayerTransform.position);
        _angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        PlayerDir = dirToPlayer;

        if (_angleToPlayer <= enemyData.FOV / 2f && distanceToPlayer <= enemyData.detectionRadius)
        {
            if (!Physics.Raycast(headPos.position, dirToPlayer, distanceToPlayer, enemyData.lineOfSightMask))
            {
                CanSeePlayer = true;
                FaceTarget(PlayerTransform.position);
                OnPlayerSpotted();
                return;
            }
        }

        CanSeePlayer = false;
    }
    
    protected virtual void OnPlayerSpotted()
    {
        if (enemyData == null) return;

        var player = Gamemanager.Instance.player.transform;
        agent.speed = enemyData.chaseSpeed;
        agent.SetDestination(player.position);

        if (!(agent.remainingDistance <= enemyData.stoppingDist)) return;
        agent.ResetPath();
        Debug.Log($"{gameObject.name} reached stopping distance.");
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
        agent.SetDestination(Gamemanager.Instance.player.transform.position);

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
        Gamemanager.Instance.UpdateGameGoal(-1);

        if (enemyData.keyPrefab)
            Instantiate(enemyData.keyPrefab, transform.position, Quaternion.identity);

        if (enemyData.deathVFX)
            Instantiate(enemyData.deathVFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayerInTrigger = true;

        HandleTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayerInTrigger = false;

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

