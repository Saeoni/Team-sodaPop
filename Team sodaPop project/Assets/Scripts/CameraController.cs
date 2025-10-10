using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] int _sens;
    [SerializeField] int _lockVertMin, _lockVertMax;
    [SerializeField] bool _invertY;
    public Camera cam;
    float _rotX;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * _sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * _sens * Time.deltaTime;

        if (_invertY)
        {
            _rotX += mouseY;
        }
        else
        {
            _rotX -= mouseY;
        }

        _rotX = Mathf.Clamp(_rotX, _lockVertMin, _lockVertMax);

        transform.localRotation = Quaternion.Euler(_rotX, 0, 0);

        transform.parent.Rotate(Vector3.up * mouseX);
    }
    
    // Write Scope Logic with in the camera
    
    
}
