using UnityEngine;

namespace TheWatcher
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] private Light flashlight;
        private bool isOn;


        private void Start()
        {
            if (flashlight == null)
                flashlight = GetComponent<Light>();
            flashlight.enabled = isOn;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Flashlight"))
            {
                isOn = !isOn;
                flashlight.enabled = isOn;
            }
        }
    }
}