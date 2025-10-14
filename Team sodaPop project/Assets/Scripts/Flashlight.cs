using UnityEngine;

namespace TheWatcher
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] private Light flashlight;
        private bool isOn = false;


        void Start()
        {
            if (flashlight == null)
                flashlight = GetComponent<Light>();
            flashlight.enabled = isOn;
        }

        void Update()
        {
            if (Input.GetButtonDown("Flashlight"))
            {
                isOn = !isOn;
                flashlight.enabled = isOn;
            }
        }
    }
}