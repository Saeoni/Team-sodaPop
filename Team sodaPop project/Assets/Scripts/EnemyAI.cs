using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public class EnemyAI : MonoBehaviour, IDamage
{
    [Header("Core Components")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject keyPrefab;

    [Header("Enemy Settings")]

    [SerializeField] float detectionRadius;
    [SerializeField] float chaseSpeed;
    [SerializeField] float shootRate;
    [SerializeField] float faceTargetSpeed;
    [SerializeField] float FOV;
    [SerializeField] LayerMask lineOfSightMask;
    [SerializeField] int HP;
    [SerializeField] int damage;

    
    float shootTimer;

    float angleToPlayer;

    bool playerInTrigger;

    Color colorOrig;

    Vector3 playerDir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        colorOrig = model.material.color;
        gamemanager.instance.updateGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;
        
        if (playerInTrigger && canSeePlayer())
        {
           
        }
    }

    bool canSeePlayer()
    {
        if (!gamemanager.instance.isStealthed)
        {
            playerDir = gamemanager.instance.player.transform.position - headPos.position;
            angleToPlayer = Vector3.Angle(playerDir, transform.forward);
            Debug.DrawRay(headPos.position, playerDir, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(headPos.position, playerDir, out hit))
            {
                if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
                {
                    agent.SetDestination(gamemanager.instance.player.transform.position);

                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        faceTarget();
                    }

                    if (shootTimer >= shootRate)
                    {
                        shoot();
                    }

                    return true;
                }
            }
        }

        return false;
    }



    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
      transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    void shoot()
    {
        shootTimer = 0;
        Instantiate(bulletPrefab, shootPos.position, transform.rotation);
    }

    /* Ummm I have to go work but will fix later - Amanda
     * 
     * void OnEnemyDeath()
    {
        Instantiate(keyPrefab, transform.position, Quaternion.identity);
    }*/

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

}

