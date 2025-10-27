using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] private int sens;
    [SerializeField] private int lockVertMin, lockVertMax;
    [SerializeField] private bool invertY;

    private float rotX;

    // Start is called before the first frame update
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    private void Update()
    {
        var mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        var mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;

        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;

        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        transform.parent.Rotate(Vector3.up * mouseX);
    }
}