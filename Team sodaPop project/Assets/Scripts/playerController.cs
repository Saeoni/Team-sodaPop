using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage, IPickup
{
    [SerializeField] CharacterController controller;
    [SerializeField] CinemachineCamera playerCam;
    [SerializeField] CinemachineCamera thirdPersonCam;
    [SerializeField] Camera miniMapCamera;
    [SerializeField] PlayerData player;
    [SerializeField] PlayerUIController playerUI;

    [SerializeField] GameObject gunModel;
    int speedOrig;
    int HPOrig;
    int jumpCount;
    public int gunListPos;
    public float lensFOVOrig;
    public float zoomFOV = 45f;
    public bool isShooting;

    Vector3 moveDir;
    Vector3 playerVel;

    float shootTimer;
    bool isSprinting;
    bool isTired = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        controller = GetComponent<CharacterController>();

        playerUI = GameManager.instance.player.GetComponent<PlayerUIController>();
        thirdPersonCam.Priority = 11;
        playerCam.Priority = 10;

        PlayerInit();

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(playerCam.transform.position, playerCam.transform.forward * player.ShootDist, Color.red);

        if (!GameManager.instance.isPaused)
        {
            movement();
        }
        sprint();
    }

    void PlayerInit()
    {
        player.ResetPlayerData();

        speedOrig = player.Speed;
        HPOrig = player.MaxHealth;

        gunListPos = 0;

        changeGun();
        player.ResetHealth();
        playerUI.UpdatePlayerUI();

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
            playerVel.y -= player.Gravity * Time.deltaTime;
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(moveDir * player.Speed * Time.deltaTime);

        // jumping mechanics
        jump();

        controller.Move(playerVel * Time.deltaTime);

        // shooting mechanics
        if (Input.GetButton("Fire1") && player.gunStats.Count > 0 && player.gunStats[gunListPos].ammoCur > 0 && shootTimer >= player.ShootRate)
            shoot();

        selectGun();
        reload();

    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < player.JumpMax)
        {
            jumpCount++;
            playerVel.y = player.JumpSpeed;
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            player.Speed *= player.SprintMod;
            player.JumpSpeed *= player.JumpMod;
            isSprinting = true;
        }

        if (Input.GetButtonUp("Sprint"))
        {
            player.Speed /= player.SprintMod;
            player.JumpSpeed /= player.JumpMod;
            isSprinting = false;
        }
    }

    void shoot()
    {
        shootTimer = 0;
        player.gunStats[gunListPos].ammoCur--;
        isShooting = true;
        //updatePlayerUI();
        playerCam.Prioritize();
        RaycastHit hit;
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, player.ShootDist))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(player.ShootDamage);
            }
        }
        // StartCoroutine(playerUI.ShootCamAndScope());
    }

    void reload()
    {
        if (Input.GetButton("Reload"))
            player.gunStats[gunListPos].ammoCur = player.gunStats[gunListPos].ammoMax;
    }

    public void takeDamage(int amount)
    {
        player.CurrentHealth -= amount;

        StartCoroutine(flashDamage());
        if (player.CurrentHealth <= 0)
        {
            GameManager.instance.youLose();
        }
        playerUI.UpdatePlayerUI();
    }

    public void KillPlayer()
    {
        takeDamage((int)player.CurrentHealth);
    }

    public void heal(int amount)
    {
        if (player.CurrentHealth < player.MaxHealth)
        {
            player.FillHealthBar(amount);
            StartCoroutine(flashHeal());
        }

    }


    IEnumerator flashDamage()
    {
        GameManager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerDamageFlash.SetActive(false);
    }
    IEnumerator flashHeal()
    {
        GameManager.instance.playerHealFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerHealFlash.SetActive(false);
    }

    public void spawnPlayer()
    {
        controller.transform.position = GameManager.instance.playerSpawnPos.transform.position;

        player.ResetHealth();
        playerUI.UpdatePlayerUI();

    }

    public string getHPPercent()
    {

        return player.PlayerHPPercent;

    }
    void selectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < player.gunStats.Count - 1)
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
        player.gunStats.Add(gun);
        gunListPos = player.gunStats.Count - 1;
        changeGun();
    }

    void changeGun()
    {
        player.ShootDamage = player.gunStats[gunListPos].shootDamage;
        player.ShootDist = player.gunStats[gunListPos].shootDist;
        player.ShootRate = player.gunStats[gunListPos].shootRate;

        gunModel.GetComponent<MeshFilter>().sharedMesh = player.gunStats[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = player.gunStats[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;

        //updatePlayerUI();
    }
    public void AddEnemy(int amount)
    {
        player.CurrentEnemyCount += amount;
    }

    public void SetPlayerCamFOV(float fov)
    {
        playerCam.Lens.FieldOfView = fov;
    }
    public void setEnemy(int amount)
    {
        player.CurrentEnemyCount = amount;

    }

    public float GetLensView()
    {
        return playerCam.Lens.FieldOfView;
    }
}
