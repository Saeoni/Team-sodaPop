using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TheWatcher;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    [SerializeField] CharacterController controller;
    Recoil recoil;

    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] float jumpSpeed;
    [SerializeField] float jumpMod;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] float pushStrength = 2f;
    [SerializeField] Animator animate;


    [SerializeField] List<gunstats> gunList = new List<gunstats>();
    [SerializeField] Transform gunModelParent;
    [SerializeField] GameObject flashlight;
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;

    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audSteps;
    [UnityEngine.Range(0, 1)][SerializeField] float audStepsVol;

    int HPOrig;
    int speedOrig;
    int jumpCount;
    int gunListPos;
    GameObject currentGunModel;

    Vector3 moveDir;
    Vector3 playerVel;

    float shootTimer;
    bool isSprinting;
    bool isTired = false;
    bool isPlayingSteps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        speedOrig = speed;
        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        if (!gamemanager.instance.isPaused)
        {
            movement();
        }
        sprint();
    }

    void movement()
    {
        shootTimer += Time.deltaTime;

        if(controller.isGrounded)
        {
            if (moveDir.normalized.magnitude > 0.3f && !isPlayingSteps)
            {
                StartCoroutine(playStep());
            }

            jumpCount = 0;
            playerVel = Vector3.zero;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(moveDir * speed * Time.deltaTime);

        // jumping mechanics
        jump();

        controller.Move(playerVel * Time.deltaTime);

        //animation
        float animSpeed = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).magnitude;
        animate.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

        // shooting mechanics
        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0  && shootTimer >= shootRate)
            shoot();

        selectGun();
        reload();

    }

    void jump()
    {
        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }

    void sprint()
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

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
            return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

        body.linearVelocity = pushDir * pushStrength;
    }

    void shoot()
    {
        shootTimer = 0;
        gunList[gunListPos].ammoCur--;
        updatePlayerUI();

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist)) 
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if(dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }

        if (recoil)
        {
            recoil.applyRecoil = true;
        }
    }

    void reload()
    {
        if (Input.GetButton("Reload"))
            gunList[gunListPos].ammoCur = gunList[gunListPos].ammoMax;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        StartCoroutine(flashDamage());
        if(HP <= 0)
        {
            gamemanager.instance.youLose();
        }
    }

    public void KillPlayer()
    {
        takeDamage(HP);
    }

    public void heal(int amount)
    {
        if(HP < HPOrig)
        {
            HP += amount;
            updatePlayerUI();
            StartCoroutine(flashHeal());
        }
    }

    public void updatePlayerUI()
    {
        gamemanager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;

        if(gunList.Count > 0)
        {
            gamemanager.instance.ammoCur.text = gunList[gunListPos].ammoCur.ToString("F0");
            gamemanager.instance.ammoMax.text = gunList[gunListPos].ammoMax.ToString("F0");

        }
    }

    IEnumerator flashDamage()
    {
        gamemanager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.playerDamageFlash.SetActive(false);
    }
    IEnumerator flashHeal()
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

    void selectGun()
    {
        if(Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
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

    public void getGunStats(gunstats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;
        changeGun();
    }

    void changeGun()
    {
        if (currentGunModel != null)
        {
            Destroy(currentGunModel);
        }

        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;

        GameObject newGun = Instantiate(gunList[gunListPos].gunModel, gunModelParent);
        
        Collider col = newGun.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        currentGunModel = newGun;
        
        updatePlayerUI();
    }

    void TurnOnFlashlight()
    {
        if (flashlight != null)
            flashlight.SetActive(true);
    }

    IEnumerator playStep()
    {
        isPlayingSteps = true;
        aud.PlayOneShot(audSteps[Random.Range(0, audSteps.Length)], audStepsVol);

        if (isSprinting)
        {
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        isPlayingSteps = false;
    }

    private void LateUpdate()
    {
        if (recoil)
        {
            recoil.applyRecoil = false;
        }
    }
}
