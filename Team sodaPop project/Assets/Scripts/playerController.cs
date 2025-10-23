using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    [SerializeField] private CharacterController controller;

    [SerializeField] private int HP;
    [SerializeField] private int speed;
    [SerializeField] private int sprintMod;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpMod;
    [SerializeField] private int jumpMax;
    [SerializeField] private int gravity;
    [SerializeField] private float pushStrength = 2f;
    [SerializeField] private Animator animate;


    [SerializeField] private List<gunstats> gunList = new();
    [SerializeField] private Transform gunModelParent;
    [SerializeField] private GameObject flashlight;
    [SerializeField] private int shootDamage;
    [SerializeField] private float shootRate;
    [SerializeField] private int shootDist;

    [SerializeField] private AudioSource aud;
    [SerializeField] private AudioClip[] audSteps;
    [Range(0, 1)] [SerializeField] private float audStepsVol;

    private GameObject currentGunModel;
    private int gunListPos;

    private int HPOrig;
    private bool isPlayingSteps;
    private bool isSprinting;
    private bool isTired = false;
    private int jumpCount;

    private Vector3 moveDir;
    private Vector3 playerVel;

    private float shootTimer;
    private int speedOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        HPOrig = HP;
        speedOrig = speed;
        updatePlayerUI();
    }

    // Update is called once per frame
    private void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        if (!gamemanager.instance.isPaused) movement();
        sprint();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
            return;

        var pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

        body.linearVelocity = pushDir * pushStrength;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        StartCoroutine(flashDamage());
        if (HP <= 0) gamemanager.instance.youLose();
    }

    public void getGunStats(gunstats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;
        changeGun();
    }

    private void movement()
    {
        shootTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            if (moveDir.normalized.magnitude > 0.3f && !isPlayingSteps) StartCoroutine(playStep());

            jumpCount = 0;
            playerVel = Vector3.zero;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        // jumping mechanics
        jump();

        controller.Move(playerVel * Time.deltaTime);

        //animation
        var animSpeed = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).magnitude;
        animate.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

        // shooting mechanics
        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= shootRate)
            shoot();

        selectGun();
        reload();
    }

    private void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }

    private void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            jumpSpeed *= jumpMod;
            isSprinting = true;
        }

        if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            jumpSpeed /= jumpMod;
            isSprinting = false;
        }
    }

    private void shoot()
    {
        shootTimer = 0;
        gunList[gunListPos].ammoCur--;
        updatePlayerUI();

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist))
        {
            Debug.Log(hit.collider.name);

            var dmg = hit.collider.GetComponent<IDamage>();

            if (dmg != null) dmg.takeDamage(shootDamage);
        }
    }

    private void reload()
    {
        if (Input.GetButton("Reload"))
            gunList[gunListPos].ammoCur = gunList[gunListPos].ammoMax;
    }

    public void KillPlayer()
    {
        takeDamage(HP);
    }

    public void heal(int amount)
    {
        if (HP < HPOrig)
        {
            HP += amount;
            updatePlayerUI();
            StartCoroutine(flashHeal());
        }
    }

    public void updatePlayerUI()
    {
        gamemanager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;

        if (gunList.Count > 0)
        {
            gamemanager.instance.ammoCur.text = gunList[gunListPos].ammoCur.ToString("F0");
            gamemanager.instance.ammoMax.text = gunList[gunListPos].ammoMax.ToString("F0");
        }
    }

    private IEnumerator flashDamage()
    {
        gamemanager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.playerDamageFlash.SetActive(false);
    }

    private IEnumerator flashHeal()
    {
        gamemanager.instance.playerHealFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.playerHealFlash.SetActive(false);
    }

    public void spawnPlayer()
    {
        controller.transform.position = gamemanager.instance.playerSpawnPos.transform.position;

        HP = HPOrig;
        updatePlayerUI();
    }

    private void selectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
        {
            gunListPos++;
            changeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            changeGun();
        }
    }

    private void changeGun()
    {
        if (currentGunModel != null) Destroy(currentGunModel);

        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;

        var newGun = Instantiate(gunList[gunListPos].gunModel, gunModelParent);

        var col = newGun.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        currentGunModel = newGun;

        updatePlayerUI();
    }

    private void TurnOnFlashlight()
    {
        if (flashlight != null)
            flashlight.SetActive(true);
    }

    private IEnumerator playStep()
    {
        isPlayingSteps = true;
        aud.PlayOneShot(audSteps[Random.Range(0, audSteps.Length)], audStepsVol);

        if (isSprinting)
            yield return new WaitForSeconds(0.3f);
        else
            yield return new WaitForSeconds(0.5f);

        isPlayingSteps = false;
    }
}