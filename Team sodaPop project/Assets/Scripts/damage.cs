using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{
    enum damageType { moving, stationary, DOT, homing }

    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [Header("Scythe Collider Control")]
    [SerializeField] Collider scytheCollider;
    [SerializeField] float activationDelay = 2f;

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

        if (scytheCollider != null) 
            scytheCollider.enabled = false;

        StartCoroutine(EnableColliderAfterDelay());

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

    IEnumerator EnableColliderAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        if (scytheCollider != null)
            scytheCollider.enabled = true;
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

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 direction = (hitPoint - transform.position);

        if (direction.sqrMagnitude > 0.0001f) // Only rotate if there's a direction
        {
            Quaternion rotation = Quaternion.LookRotation(direction.normalized);
            Instantiate(impactEffect, hitPoint, rotation);
        }
        else
        {
            //  Fallback: spawn with default rotation
            Instantiate(impactEffect, hitPoint, Quaternion.identity);
        }

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