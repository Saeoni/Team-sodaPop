using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] float jumpSpeed;
    [SerializeField] float jumpMod;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;

    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;

    int HPOrig;
    int speedOrig;
    int jumpCount;

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
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        movement();
        sprint();
    }

    void movement()
    {
        shootTimer += Time.deltaTime;

        if(controller.isGrounded)
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
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            shoot();

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

    void shoot()
    {
        shootTimer = 0;

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
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();

        if(HP <= 0)
        {
            gamemanger.instance.youLose();
        }
    }

    public void updatePlayerUI()
    {
        gamemanger.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }
}
