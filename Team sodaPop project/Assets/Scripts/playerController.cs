using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    [SerializeField] CharacterController controller;

    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] float jumpSpeed;
    [SerializeField] float jumpMod;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;


    [SerializeField] List<gunstats> gunList = new List<gunstats>();
    [SerializeField] GameObject gunModel;
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;

    public int HPOrig;
    int speedOrig;
    int jumpCount;
    int gunListPos;

    Vector3 moveDir;
    Vector3 playerVel;

    float shootTimer;
    bool isSprinting;
    bool isTired = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        speedOrig = speed;
        updatePlayerUI();
        gamemanager.instance.playerScript = this;
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

        if (controller.isGrounded)
        {
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

        // shooting mechanics
        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= shootRate)
            shoot();

        selectGun();
        reload();

    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
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

    void shoot()
    {
        shootTimer = 0;
        gunList[gunListPos].ammoCur--;
        updatePlayerUI();

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
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
        if (HP <= 0)
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
        if (HP < HPOrig)
        {
            HP += amount;
            updatePlayerUI();
            StartCoroutine(flashHeal());
        }

        else if (HP > HPOrig)
        {
            HP = HPOrig;
            updatePlayerUI();
        }
    }

    public void updatePlayerUI()
    {


        if (gunList.Count > 0)
        {
            gamemanager.instance.ammoCur = gunList[gunListPos].ammoCur;
            gamemanager.instance.ammoMax = gunList[gunListPos].ammoMax;



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

    public float getHPPercent()
    {

        return (float)HP / HPOrig;



    }
    void selectGun()
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

    public void getGunStats(gunstats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;
        changeGun();
    }

    void changeGun()
    {
        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;

        updatePlayerUI();
    }
}
