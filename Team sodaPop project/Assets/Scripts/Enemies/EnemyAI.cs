using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;



public class EnemyAI : MonoBehaviour, IDamage
{
    [Header("Core Components")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Animator animator;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
    [SerializeField] Transform healthBarPos;
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
    [SerializeField] float roamDist;
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
    private Vector3 startingPos;


    private UIDocument healthBarDoc;
    private VisualElement healthBar;
    private int maxHealth;
    void Start()
    {
        _colorOrig = model.material.color;
        GameManager.instance.updateGameGoal(1);
        _spawnPos = transform.position;
        _originalStopDist = agent.stoppingDistance;
        maxHealth = HP;





    }



    // Update is called once per frame
    void Update()
    {
        _shootTimer += Time.deltaTime;
        UpdateLocomotionAnim();



        if (_playerInTrigger && !canSeePlayer())
        {
            CheckRoam();
        }
        else if (!_playerInTrigger)
        {
            CheckRoam();
        }

    }
    public void InitializeEnemy()
    {
        Debug.Log("Enemy HP set to: " + HP);

        healthBarDoc = healthBarPos.GetComponent<UIDocument>();
        if (healthBarDoc == null)
        {
            Debug.LogWarning("Health bar UIDocument is not assigned.");
            return;
        }
        healthBarDoc.transform.LookAt(GameManager.instance.playerScript.activeCamera.transform);
        healthBarDoc.transform.Rotate(0, 180, 0);
        healthBarDoc.rootVisualElement.style.width = 100;
        healthBarDoc.rootVisualElement.style.height = 15;
        var root = healthBarDoc.rootVisualElement;
        healthBar = root.Q<ProgressBar>("Health");
        healthBar.style.width = (float)HP / maxHealth * 100f;
        HideHealthBar();

    }
    public void ShowHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.style.display = DisplayStyle.Flex;
        }
    }
    public void HideHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.style.display = DisplayStyle.None;
        }
    }
    public void UpdateEnemyHealthBar()
    {


        if (healthBarDoc == null)
        {
            Debug.LogWarning("Health bar UIDocument is not assigned.");
            return;
        }

        if (healthBar != null)
        {
            ShowHealthBar();
            healthBar.style.width = (float)HP / maxHealth * 100f;
        }
    }
    void UpdateLocomotionAnim()
    {
        float currentSpeed = agent.velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / agent.speed);
        float currentAnimSpeed = animator.GetFloat("Speed");

        animator.SetFloat("Speed", Mathf.Lerp(currentAnimSpeed, normalizedSpeed, Time.deltaTime * animTransSpeed));
    }

    public void LookAtActiveCamera()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            Transform camTransform = GameManager.instance.playerScript.activeCamera.transform;
            Vector3 lookPos = new Vector3(camTransform.position.x, transform.position.y, camTransform.position.z);
            transform.LookAt(lookPos);
        }
    }


    bool canSeePlayer()
    {
        if (GameManager.instance.isStealthed)
        {
            _playerDir = GameManager.instance.player.transform.position - headPos.position;
            _angleToPlayer = Vector3.Angle(_playerDir, transform.forward);
            Debug.DrawRay(headPos.position, _playerDir, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(headPos.position, _playerDir, out hit))
            {
                if (_angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
                {
                    agent.SetDestination(GameManager.instance.player.transform.position);

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
        _roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 randomPos = Random.insideUnitSphere * roamDist;
        randomPos += startingPos;

        NavMeshHit meshHit;
        NavMesh.SamplePosition(randomPos, out meshHit, roamDist, 1);
        agent.SetDestination(meshHit.position);
    }

    void CheckRoam()
    {
        if (_roamTimer >= roamPauseTimer && agent.remainingDistance <= 0.01f)
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

        GameManager.instance.updateGameGoal(-1);

        if (keyPrefab != null)
            Destroy(gameObject);

        Instantiate(keyPrefab, transform.position, Quaternion.identity);

    }

    public void takeDamage(int amount)
    {
        if (HP <= 0)
        {
            Debug.Log("Enemy is supposed to dead already.");
            return;
        }

        HP -= amount;
        UpdateEnemyHealthBar();

        StartCoroutine(flashRed());
        agent.SetDestination(GameManager.instance.player.transform.position);

        if (HP <= 0)
            OnEnemyDeath();
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = _colorOrig;
        HideHealthBar();
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

