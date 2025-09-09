using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
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
    [SerializeField] Transform player;

    [Header("Enemy Settings")]
    [SerializeField] EnemyType enemyType;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float detectionRadius = 15.0f;
    [SerializeField] float chaseSpeed = 3.5f;
    [SerializeField] float patrolSpeed = 2.0f;
    [SerializeField] int HP = 100;
    [SerializeField] int faceTargetSpeed = 5;

    [Header("Combat Settings")]
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject homingMissile;
    [SerializeField] float shootRate = 1.0f;
    [SerializeField] int damage = 10;
    

    private int patrolIndex = 0;
    float shootTimer;
    bool playerInTrigger;
    Color colorOrig;
    Vector3 playerDir;

   
  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        colorOrig = model.material.color;
        // Gamemaneger.instance.updateGameGoal(1);

        if (enemyType == EnemyType.Patrol)
        {
           agent.speed = chaseSpeed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;
        playerDir = player.position - transform.position;

        switch (enemyType)
        {
            case EnemyType.Stationary:
                handleStationary();
                break;
            case EnemyType.Patrol:
                handlePatrol(); 
                break;
            case EnemyType.Homing:
                handleHoming();
            break;
        }
    }

    void handleStationary()
    {
        if (!playerInTrigger) return;

        float distance = Vector3.Distance(transform.position, player.position);
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

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRadius)
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);

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
    void handleHoming()
    {
        if (!playerInTrigger) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRadius && shootTimer >= shootRate)
        {
            ShootHomingMissile();
        }
    }
    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    void shoot()
    {

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

        // Attach homing logic directly here if needed
        if (player != null)
        {
            StartCoroutine(HomingBehavior(missile.transform));
        }
    }



    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());

        if (HP <= 0)
        {
            //Gamemanager.instance.updateGameGoal(-1);
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

        while (timer < lifetime && player != null)
        {
            Vector3 direction = (player.position - missile.position).normalized;
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

