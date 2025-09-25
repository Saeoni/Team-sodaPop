using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public class TurretAI : MonoBehaviour, IDamage
{

    [Header("Core Components")]
    [SerializeField] Renderer model;
    [SerializeField] Transform turretHead;
    [SerializeField] Transform headPos;
    [SerializeField] Transform shootPoint;
    [SerializeField] Transform[] missileLaunchPoints;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject homingMissilePrefab;

    [Header("Detection & Combat")]
    [SerializeField] bool useMissiles = false;
    [SerializeField] float detectionRadius = 20f;
    [SerializeField] float shootRate = 1f;
    [SerializeField] float FOV = 90f;
    [SerializeField] int rotationSpeed = 5;
    [SerializeField] int HP = 100;
    [SerializeField] LayerMask lineOfSightMask;

    float shootTimer;
    float angleToPlayer;
    bool playerInTrigger;
    Vector3 dirOfPlayer;
    Color colorOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;

        if (playerInTrigger && lineOfSightToPlayer())
        {
            RotateToPlayer();

            if (shootTimer >= shootRate)
            {
                shootTimer = 0f;
                if (useMissiles)
                {
                    FireHomingMissiles();
                }
                else
                    fireBullet();
            }
        }
    }

    bool lineOfSightToPlayer()
    {
        Transform player = gamemanager.instance.player.transform;
        dirOfPlayer = player.position - headPos.position;
        angleToPlayer = Vector3.Angle(dirOfPlayer, turretHead.forward);

        Debug.DrawRay(headPos.position, dirOfPlayer.normalized * detectionRadius, Color.red);

        if (angleToPlayer > FOV) return false;

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, dirOfPlayer.normalized, out hit, detectionRadius, lineOfSightMask ))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    void RotateToPlayer()
    {
        Vector3 direction = gamemanager.instance.player.transform.position - turretHead.position;
        direction.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        turretHead.rotation = Quaternion.Lerp(turretHead.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void fireBullet()
    {
        Transform player = gamemanager.instance.player.transform;
        Vector3 targetPos = player.position;
        Vector3 directionToPlayer = (targetPos - shootPoint.position);

        Instantiate(bulletPrefab, shootPoint.position, Quaternion.LookRotation(directionToPlayer));    
    }

    void FireHomingMissiles()
    {
       
        if (missileLaunchPoints != null && homingMissilePrefab != null)
        {
            foreach (Transform launchPoint in missileLaunchPoints)
            {
                Instantiate(homingMissilePrefab, launchPoint.position, launchPoint.rotation);
            }
        }
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

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
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
}
