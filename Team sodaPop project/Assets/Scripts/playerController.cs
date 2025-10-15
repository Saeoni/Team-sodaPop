using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour, IDamage, IPickup
{
    [SerializeField] CharacterController controller;

    [SerializeField]  int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] float jumpSpeed;
    [SerializeField] float jumpMod;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] float pushStrength = 2f;


    [SerializeField] List<gunstats> gunList = new List<gunstats>();
    [SerializeField] GameObject gunModel;
    [SerializeField] GameObject flashlight;
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;

    [FormerlySerializedAs("HPOrig")] public int hpOrig;
    private int _speedOrig;
    private int _jumpCount;
    private int _gunListPos;

    private Vector3 _moveDir;
    private Vector3 _playerVel;

    private float _shootTimer;
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool _isSprinting;
#pragma warning restore CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool _isTired = false;
#pragma warning restore CS0414 // Field is assigned but its value is never used
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpOrig = HP;
        _speedOrig = speed;
        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        if (!Gamemanager.Instance.isPaused)
        {
            movement();
        }
        sprint();
    }

    void movement()
    {
        _shootTimer += Time.deltaTime;

        if(controller.isGrounded)
        {
            _jumpCount = 0;
            _playerVel = Vector3.zero;
        }
        else
        {
            _playerVel.y -= gravity * Time.deltaTime;
        }

        _moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(_moveDir * speed * Time.deltaTime);

        // jumping mechanics
        jump();

        controller.Move(_playerVel * Time.deltaTime);

        // shooting mechanics
        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[_gunListPos].ammoCur > 0  && _shootTimer >= shootRate)
            Shoot();

        SelectGun();
        Reload();

    }

    void jump()
    {
        if(Input.GetButtonDown("Jump") && _jumpCount < jumpMax)
        {
            _jumpCount++;
            _playerVel.y = jumpSpeed;
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            jumpSpeed *= jumpMod;
            _isSprinting = true;
        }

        if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            jumpSpeed /= jumpMod;
            _isSprinting = false;
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

    private void Shoot()
    {
        _shootTimer = 0;
        gunList[_gunListPos].ammoCur--;
        UpdatePlayerUI();

        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist)) return;
        Debug.Log(hit.collider.name);

        IDamage dmg = hit.collider.GetComponent<IDamage>();

        if(dmg != null)
        {
            dmg.takeDamage(shootDamage);
        }
    }

    private void Reload()
    {
        if (Input.GetButton("Reload"))
            gunList[_gunListPos].ammoCur = gunList[_gunListPos].ammoMax;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        UpdatePlayerUI();
        StartCoroutine(FlashDamage());
        if(HP <= 0)
        {
            Gamemanager.Instance.YouLose();
        }
    }

    public void KillPlayer()
    {
        takeDamage(HP);
    }

    public void heal(int amount)
    {
        if(HP < hpOrig)
        {
            HP += amount;
            UpdatePlayerUI();
            StartCoroutine(FlashHeal());
        }

        else if(HP > hpOrig)
        {
            HP = hpOrig;
            UpdatePlayerUI();
        }
    }

    private void UpdatePlayerUI()
    {
        Gamemanager.Instance.playerHpBar.fillAmount = (float)HP / hpOrig;

        if (gunList.Count <= 0) return;
        Gamemanager.Instance.ammoCur = gunList[_gunListPos].ammoCur;
        Gamemanager.Instance.ammoMax = gunList[_gunListPos].ammoMax;
        HUDController.instance.UpdatePlayerUI();
    }

    private IEnumerator FlashDamage()
    {
        Gamemanager.Instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Gamemanager.Instance.playerDamageFlash.SetActive(false);
    }

    private IEnumerator FlashHeal()
    {
        Gamemanager.Instance.playerHealFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Gamemanager.Instance.playerHealFlash.SetActive(false);
    }

    public void SpawnPlayer()
    {
        controller.transform.position = Gamemanager.Instance.playerSpawnPos.transform.position;

        HP = hpOrig;
        UpdatePlayerUI();

    }

    private void SelectGun()
    {
        if(Input.GetAxis("Mouse ScrollWheel") > 0 && _gunListPos < gunList.Count - 1)
        {
            _gunListPos++;
            ChangeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && _gunListPos > 0)
        {
            _gunListPos--;
            ChangeGun();
        }

    }

    public void getGunStats(gunstats gun)
    {
        gunList.Add(gun);
        _gunListPos = gunList.Count - 1;
        ChangeGun();
    }

    private void ChangeGun()
    {
        shootDamage = gunList[_gunListPos].shootDamage;
        shootDist = gunList[_gunListPos].shootDist;
        shootRate = gunList[_gunListPos].shootRate;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[_gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[_gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;

        UpdatePlayerUI();
    }

    void TurnOnFlashlight()
    {
        if (flashlight != null)
            flashlight.SetActive(true);
    }
}
