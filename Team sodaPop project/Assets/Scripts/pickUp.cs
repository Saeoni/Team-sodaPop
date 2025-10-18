using UnityEngine;
using System.Collections;

public class pickUp : MonoBehaviour
{

    enum pickupType { health, key, stealth, gun, Note}

    [SerializeField] pickupType type;

    [SerializeField] int healAmount;
    [SerializeField] gunstats gun;


    //    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //    void Start()
    //    {

    //    }

    //    // Update is called once per frame
    //    void Update()
    //    {

    //    }

    private void OnTriggerEnter(Collider other)
    {
        IPickup pickupable = other.GetComponent<IPickup>();
        if (type == pickupType.health)
        {
            other.GetComponent<playerController>().heal(healAmount);
            Destroy(gameObject);
        }

        else if (type == pickupType.key)
        {
            gamemanager.instance.keyCount++;
            gamemanager.instance.updateKeyCount();

            Destroy(gameObject);
        }

        else if (type == pickupType.stealth)
        {
            gamemanager.instance.stealthTimer(10.0f);
            Destroy(gameObject);
        }
        else if(type == pickupType.Note)
        {
            gamemanager.instance.NoteDisplay();
            Destroy(gameObject);
        }

        else if (pickupable != null)
        {
            gun.ammoCur = gun.ammoMax;
            pickupable.getGunStats(gun);
            Destroy(gameObject);
        }

    }
}
