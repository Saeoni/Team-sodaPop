using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{
    enum damageType { moving, stationary, DOT, homing }

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

    bool isDamaging;

    void Start()
    {
        if (type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject, destroyTime);

            if (rb != null)
                rb.linearVelocity = transform.forward * speed;
        }
    }

    void Update()
    {
        if (type == damageType.homing)
        {
            Vector3 targetDir = (gamemanager.instance.player.transform.position - transform.position).normalized;
            rb.linearVelocity = targetDir * speed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && (type == damageType.stationary || type == damageType.moving || type == damageType.homing))
        {
            dmg.takeDamage(damageAmount);
        }

        if (impactParticles != null)
            impactParticles.Play();

        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        if ((type == damageType.moving || type == damageType.homing) && explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        if (type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && type == damageType.DOT)
        {
            if (!isDamaging)
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