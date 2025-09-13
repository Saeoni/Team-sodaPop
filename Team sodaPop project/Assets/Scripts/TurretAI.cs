using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class TurretAI : MonoBehaviour, IDamage
{
    [Header("Core Components")]
    [SerializeField] Transform turretHead;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPosition;
    [SerializeField] Renderer model;
    

    [Header("Turret Setting")]
    [SerializeField] float detectionRadius;
    [SerializeField] int FOV;
    [SerializeField] float faceTargetSpeed;
    [SerializeField] float shootRate;
    [SerializeField] int HP = 100;

    [Header("Combat Settings")]
    [SerializeField] GameObject homingMissile;
    [SerializeField] int damage;

    Transform player;
    Vector3 playerDir;
    float angleToPlayer;
    bool playerInTrigger;
    float shootTimer;
    Color colorOrig;

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

    bool canSeePlayer()
    {
       
        playerDir = gamemanger.instance.player.transform.position - headPosition.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);
        Debug.DrawRay(headPosition.position, playerDir, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(headPosition.position, playerDir, out hit))
        {
            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= detectionRadius)
                {
                    FaceTarget();
                }
                if (shootTimer >= shootRate)
                {
                    FireHomingMissile();
                }

                return true;
            }
        }
        return false;
    }

    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(turretHead.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    void FireHomingMissile()
    {
        shootTimer = 0;
        if (homingMissile == null)
        {
            Debug.LogWarning("Homing missile prefab not assigned");
            return;
        }

        Instantiate(homingMissile, shootPos.position, transform.rotation);

    }


    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());

        if (HP <= 0)
        {
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
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }
}
