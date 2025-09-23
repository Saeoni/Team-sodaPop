using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public class EnemyAI : MonoBehaviour, IDamage
{
    [Header("Core Components")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Animator animator;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject keyPrefab;
    
    [Header("Detection & Chase Settings")]
    [SerializeField] float detectionRadius;
    [SerializeField] float chaseSpeed;
    [SerializeField] float faceTargetSpeed;
    [SerializeField] float FOV;
    [SerializeField] LayerMask lineOfSightMask;

    [Header("Combat Settings")]
    [SerializeField] float shootRate;
    [SerializeField] int HP;
    [SerializeField] int damage;

    [Header("Roaming Settings")]
    [SerializeField] float roamRadius;
    [SerializeField] float distToRoam;
    [SerializeField] float roamPauseTimer;
    [SerializeField] float animTransSpeed;

    float _shootTimer;
    float _roamTimer;
    float _originalStopDist;
    float _angleToPlayer;
    bool _playerInTrigger;
    Color _colorOrig;
    Vector3 _playerDir;
    Vector3 _spawnPos;

    void Start()
    {
        _colorOrig = model.material.color;
        gamemanager.instance.updateGameGoal(1);
        _spawnPos = transform.position;
        _originalStopDist = agent.stoppingDistance;

   
    }

    // Update is called once per frame
    void Update()
    {
        _shootTimer += Time.deltaTime;
        UpdateAnimLocomotionAnim();

        if (_playerInTrigger && !canSeePlayer())
        {
           CheckRoam();
        }
        else if (!_playerInTrigger)
        {
            CheckRoam();
        }
       
    }

    void UpdateAnimLocomotionAnim()
    {
        float currSpeed = agent.velocity.magnitude;
        float normalizedSpeed = currSpeed / agent.speed;
        float animFloat = animator.GetFloat("Speed");

        animator.SetFloat("Speed", Mathf.Lerp(animFloat, normalizedSpeed, Time.deltaTime * animTransSpeed));
    }

    bool canSeePlayer()
    {
        if (!gamemanager.instance.isStealthed)
        {
            _playerDir = gamemanager.instance.player.transform.position - headPos.position;
            _angleToPlayer = Vector3.Angle(_playerDir, transform.forward);
            Debug.DrawRay(headPos.position, _playerDir, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(headPos.position, _playerDir, out hit))
            {
                if (_angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
                {
                    agent.SetDestination(gamemanager.instance.player.transform.position);

                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        faceTarget();
                    }

                    if (_shootTimer >= shootRate)
                    {
                        shoot();
                    }
                    agent.stoppingDistance = _originalStopDist;
                    return true;
                }
            }
        }
        agent.stoppingDistance = 0;
        return false;
    }

    void BeginRoam()
    {
        _roamTimer = 0f;
        agent.stoppingDistance = 0f;

        Vector3 randDirection = Random.insideUnitSphere * distToRoam;
        Vector3 roamOrigin = transform.position; // roam from current position
        Vector3 targetPos = roamOrigin + randDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, distToRoam, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.SetDestination(transform.position);
        }
    }

    void CheckRoam()
    {
        bool isIdle = agent.remainingDistance < 0.01f;
        _roamTimer += Time.deltaTime;   

        if (_roamTimer >= roamPauseTimer && isIdle)
        {
            BeginRoam();
        }
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(_playerDir.x, transform.position.y, _playerDir.z));
      transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

   

    void shoot()
    {
        _shootTimer = 0;
        animator.SetTrigger("Shoot");
    }

    public void CreateBullet()
    {
        Instantiate(bulletPrefab, shootPos.position, transform.rotation);
    }


    void OnEnemyDeath()
    {
<<<<<<< HEAD
        gamemanager.instance.updateGameGoal(-1);

        if (keyPrefab != null) 
        Instantiate(keyPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
=======
            Instantiate(keyPrefab, transform.position, Quaternion.identity);
>>>>>>> d0d32988c654398595ba8b2469cfd0e13c8d4e26
    }

    public void takeDamage(int amount)
    {
        if(HP <= 0)
            return;

        HP -= amount;
        StartCoroutine(flashRed());
        agent.SetDestination(gamemanager.instance.player.transform.position);

        if (HP <= 0)
            OnEnemyDeath();
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = _colorOrig;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInTrigger = false;
        agent.stoppingDistance = 0f;
    }

}

