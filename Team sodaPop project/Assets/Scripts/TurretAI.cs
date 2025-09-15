using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public class TurretAI : MonoBehaviour, IDamage
{

    [Header("Core Components")]
    [SerializeField] Renderer model;
    [SerializeField] Transform turretHead;
    [SerializeField] Transform shootPoint;
    [SerializeField] Transform[] missileLaunchPoints;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject homingMissilePrefab;

    [Header("Detection & Combat")]
    [SerializeField] bool useMissiles = false;
    [SerializeField] float detectionRadius;
    [SerializeField] float shootRate;
    [SerializeField] float FOV;
    [SerializeField] int rotationSpeed;
    [SerializeField] int HP;
    [SerializeField] LayerMask lineOfSightMask;

    float shootTimer;
    float angleToPlayer;
    bool playerInTrigger;
    Vector3 dirOfPlayer;
    Color colorOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool lineOfSightToPlayer()
    {
        dirOfPlayer = gamemanager.instance.player.transform.position - turretHead.position;
        angleToPlayer = Vector3.Angle(dirOfPlayer, turretHead.forward);
        Debug.DrawRay(turretHead.position, dirOfPlayer);

        if (angleToPlayer > FOV) return false;

        RaycastHit hit;
        if (Physics.Raycast(turretHead.position, dirOfPlayer.normalized, out hit, detectionRadius, lineOfSightMask ))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    void RotateToPlayer()
    {
        dirOfPlayer = gamemanager.instance.player.transform.position - turretHead.position;
        Quaternion rot = Quaternion.LookRotation(dirOfPlayer);
        turretHead.rotation = Quaternion.Lerp(turretHead.rotation, rot, Time.deltaTime * rotationSpeed);
    }

    void firBullet()
    {
        Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
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
