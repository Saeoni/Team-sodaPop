using System.Collections;
using UnityEngine;

public class damage : MonoBehaviour
{
    [SerializeField] private damageType type;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private int damageAmount;
    [SerializeField] private float damageRate;
    [SerializeField] private int speed;
    [SerializeField] private int destroyTime;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject impactEffect;


    private bool isDamaging;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //moving projectiles will disappear after a certain time
        if (type == damageType.moving || type == damageType.homing || type == damageType.cinematicPull)
        {
            Destroy(gameObject, destroyTime);

            if (type == damageType.moving) rb.linearVelocity = transform.forward * speed;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (type == damageType.homing)
        {
            //checks player position and follows it
            var targetDir = (gamemanager.instance.player.transform.position - transform.position).normalized;
            rb.linearVelocity = targetDir * speed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        var dmg = other.GetComponent<IDamage>();


        if (dmg != null && (type == damageType.moving || type == damageType.homing)) dmg.takeDamage(damageAmount);

        if ((type == damageType.homing || type == damageType.moving) && explosionPrefab != null)
        {
            Debug.Log("Projectile hit: " + other.name);
            var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        var dmg = other.GetComponent<IDamage>();

        if (dmg != null && type == damageType.DOT)
            if (!isDamaging)
                StartCoroutine(damageother(dmg));
    }

    private IEnumerator damageother(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }

    private enum damageType
    {
        moving,
        stationary,
        DOT,
        homing,
        cinematicPull
    }
}