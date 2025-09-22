using UnityEngine;
using System.Collections;

public class pickUp : MonoBehaviour
{
    enum pickupType { health, key, stealth}

    [SerializeField] pickupType type;

    [SerializeField] healthPickup healthKit;


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
        IPickup pickingUp = other.GetComponent<IPickup>();

        if (pickingUp != null && type == pickupType.health)
        {
            pickingUp.pickupHealth(healthKit.healAmount);
            Destroy(gameObject);
        }

        if (pickingUp != null && type == pickupType.key)
        {
            pickingUp.pickupKey();

            Destroy(gameObject);
        }

        // Stealth Pick Up in the works - Timer giving me grief (I know it's something simple I just can't find it)
        //
        //if(type == pickupType.stealth)
        //{
        //    gamemanager.instance.isStealthed = true;
        //    gamemanager.instance.stealthTimer(3.0f);
        //    Destroy(gameObject);
            
        //}
    }
}
