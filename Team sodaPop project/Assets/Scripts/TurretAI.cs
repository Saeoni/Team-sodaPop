using UnityEngine;
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

    float _shootTimer;
    float _angleToPlayer;
    bool _playerInTrigger;
    Vector3 _dirOfPlayer;
    Color _colorOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _colorOrig = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        _shootTimer += Time.deltaTime;

        if (_playerInTrigger && LineOfSightToPlayer())
        {
            RotateToPlayer();

            if (_shootTimer >= shootRate)
            {
                _shootTimer = 0f;
                if (useMissiles)
                {
                    FireHomingMissiles();
                }
                else
                    FireBullet();
            }
        }
    }

    bool LineOfSightToPlayer()
    {
        Transform player = Gamemanager.Instance.player.transform;
        _dirOfPlayer = player.position - headPos.position;
        _angleToPlayer = Vector3.Angle(_dirOfPlayer, turretHead.forward);

        //Debug.DrawRay(headPos.position, _dirOfPlayer.normalized * detectionRadius, Color.red);

        if (_angleToPlayer > FOV) return false;

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, _dirOfPlayer.normalized, out hit, detectionRadius, lineOfSightMask ))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    void RotateToPlayer()
    {
        Vector3 direction = Gamemanager.Instance.player.transform.position - turretHead.position;
        direction.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        turretHead.rotation = Quaternion.Lerp(turretHead.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void FireBullet()
    {
        Transform player = Gamemanager.Instance.player.transform;
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
            _playerInTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInTrigger = false;
    }

    IEnumerator FlashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = _colorOrig;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(FlashRed());

        if (HP <= 0)
        {
            Gamemanager.Instance.UpdateGameGoal(-1);
            Destroy(gameObject);
        }
    }
}
