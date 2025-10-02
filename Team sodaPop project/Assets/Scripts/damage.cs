using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{

    enum damageType { moving, stationary, DOT, homing, scythePull, laser}
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    [Header("Impact FX")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] GameObject impactEffect;
    [SerializeField] ParticleSystem impactParticles;

    [Header("Laser Settings")]
    [SerializeField] LineRenderer laserRenderer;
    [SerializeField] float laserDuration;
    [SerializeField] float laserRange;
    [SerializeField] LayerMask hitMask;

    bool isDamaging;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (type == damageType.laser)
        {
            SetUpLaserVisuals();
            LaserFire();
            Destroy(gameObject, laserDuration);
        }
        //moving projectiles will disappear after a certain time
        else if(type == damageType.moving || type == damageType.homing || type == damageType.scythePull)
        {
            Destroy(gameObject, destroyTime);

            if(type == damageType.moving || type == damageType.laser)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (type == damageType.homing)
        {
            //checks player position and follows it
           Vector3 targetDir = (gamemanager.instance.player.transform.position - transform.position).normalized;
            rb.linearVelocity = targetDir * speed;
        }

       
    }

    void SetUpLaserVisuals()
    {
        if (laserRenderer != null)
        {
            laserRenderer.enabled = false;
            laserRenderer.startColor = Color.red;
            laserRenderer.endColor = Color.red;

            Material glowMat = new Material(Shader.Find("Unlit/Color"));
            glowMat.color = Color.red;
            laserRenderer.material = glowMat;
        }
    }

    void LaserFire()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        laserRenderer.SetPosition(0, origin);

        RaycastHit laserHit;
        if (Physics.Raycast(origin, direction, out laserHit, laserRange, hitMask))
        {
            laserRenderer.SetPosition(1, laserHit.point);

            IDamage dmg = laserHit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(damageAmount);

            if (impactEffect != null)
                Instantiate(impactEffect, laserHit.point, Quaternion.LookRotation(laserHit.normal));
        }
        else
        {
            laserRenderer.SetPosition(1, origin + direction * laserRange);
        }

        StartCoroutine(FlashLaser());
    }

    IEnumerator FlashLaser()
    {
        laserRenderer.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        laserRenderer.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        

        if(dmg != null && (type == damageType.moving || type == damageType.homing))
        {
            dmg.takeDamage(damageAmount);
        }

        if (impactParticles != null)
            impactParticles.Play();

        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        if ((type == damageType.homing || type == damageType.moving) && explosionPrefab != null)
        {
            Debug.Log("Projectile hit: " + other.name);
          GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);  
            Destroy(explosion, 2f );
        }
       Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.isTrigger) 
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if(dmg != null && type == damageType.DOT)
        {
            if(!isDamaging)
            {
                StartCoroutine(damageother(dmg));
            }
        }
    }

    IEnumerator damageother(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }

   
}
