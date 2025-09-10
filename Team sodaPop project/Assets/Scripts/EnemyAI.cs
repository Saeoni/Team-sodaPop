using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour, IDamage
{
    public enum EnemyType
    {
        Stationary,
        Patrol,
        Homing
    }

    [Header("Core Components")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
   

    [Header("Enemy Settings")]
    [SerializeField] EnemyType enemyType;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float detectionRadius = 15.0f;
    [SerializeField] float chaseSpeed = 3.5f;
    [SerializeField] float patrolSpeed = 2.0f;
    [SerializeField] int HP = 100;
    [SerializeField] int faceTargetSpeed = 5;
    [SerializeField] int FOV;

    [Header("Combat Settings")]
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject homingMissile;
    [SerializeField] float shootRate = 1.0f;
    [SerializeField] int damage = 10;
    

    private int patrolIndex = 0;

    float shootTimer;

    float angleToPlayer;

    bool playerInTrigger;

    Color colorOrig;

    Vector3 playerDir;


   
  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        colorOrig = model.material.color;
        gamemanger.instance.updateGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;
        
        if (playerInTrigger && canSeePlayer())
        {

        }
    }

    void handleStationary()
    {
        if (!playerInTrigger) return;

        float distance = Vector3.Distance(transform.position, playerDir);
        if (distance <= detectionRadius && shootTimer >= shootRate)
        {
            FaceTarget();
            shoot();
        }
    }
    void handlePatrol()
    {
        if (!playerInTrigger)
        {
            patrol();
        }

        float distance = Vector3.Distance(transform.position, playerDir);
        if (distance <= detectionRadius)
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(playerDir);

            if (agent.remainingDistance <= agent.stoppingDistance)
                FaceTarget();

            if (shootTimer >= shootRate)
                shoot();
        }
        else
        {
            patrol();
        }
    }
    void patrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.speed = patrolSpeed;
        agent.SetDestination(patrolPoints[patrolIndex].position);

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
    }

    bool canSeePlayer()
    {
        Transform player = gamemanger.instance.player.transform;
        playerDir = gamemanger.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);
        Debug.DrawRay(headPos.position, playerDir, Color.red);

        RaycastHit raycastHit;
        if (Physics.Raycast(headPos.position, playerDir, out raycastHit))
        {
            if (angleToPlayer <= FOV && raycastHit.collider.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance > detectionRadius) return false;

                if (enemyType == EnemyType.Patrol || enemyType == EnemyType.Stationary)
                {
                    agent.speed = (enemyType == EnemyType.Patrol) ? chaseSpeed : 0f;
                    agent.SetDestination(player.position);

                    if (agent.remainingDistance <= agent.stoppingDistance)
                        FaceTarget();

                    if (shootTimer >= shootRate)
                        shoot();
                }
                else if (enemyType == EnemyType.Homing && shootTimer >= shootRate)
                {
                    ShootHomingMissile();
                }

                return true;
            }
        }

        return false;
    }
            
    void handleHoming()
    {
        if (!playerInTrigger) return;

        float distance = Vector3.Distance(transform.position, playerDir);
        if (distance <= detectionRadius && shootTimer >= shootRate)
        {
            ShootHomingMissile();
        }
    }
    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    void shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    void ShootHomingMissile()
    {
        shootTimer = 0;

        if (homingMissile == null)
        {
            Debug.LogWarning("Homing missile prefab not assigned.");
            return;
        }

       GameObject missile = Instantiate(homingMissile, shootPos.position, transform.rotation);
       StartCoroutine(HomingBehavior(missile.transform));   
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());

        if (HP <= 0)
        {
            //gamemanger.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    IEnumerator HomingBehavior(Transform missile)
    {
        float lifetime = 5f;
        float timer = 0f;
        float speed = 10f;
        float rotateSpeed = 200f;

        while (timer < lifetime && playerDir != null)
        {
            Vector3 direction = (playerDir - missile.position).normalized;
            Quaternion rotateTo = Quaternion.LookRotation(direction);
            missile.rotation = Quaternion.RotateTowards(missile.rotation, rotateTo, rotateSpeed * Time.deltaTime);
            missile.position += missile.forward * speed * Time.deltaTime;

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(missile.gameObject);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

}

