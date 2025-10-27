using UnityEngine;
using UnityEngine.UI;

public class Raycast : MonoBehaviour
{
    [Header("Raycast Features")] [SerializeField]
    private float raylength = 5;

    [Header("Crosshair")] [SerializeField] private Image crosshair;

    [Header("Input Key")] [SerializeField] private KeyCode interactKey;

    private Camera _camera;

    private NoteController _notecontroller;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Physics.Raycast(_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), transform.forward, out var hit,
                raylength))
        {
            var readableItem = hit.collider.GetComponentInChildren<NoteController>();
            if (readableItem != null)
            {
                _notecontroller = readableItem;
                HighlightCrosshair(true);
            }
            else
            {
                ClearNote();
            }
        }
        else
        {
            ClearNote();
        }

        if (_notecontroller != null)
            if (Input.GetKeyDown(interactKey))
                _notecontroller.ShowNote();
    }

    private void ClearNote()
    {
        if (_notecontroller != null)
        {
            HighlightCrosshair(false);
            _notecontroller = null;
        }
    }

    private void HighlightCrosshair(bool on)
    {
        if (on)
            crosshair.color = Color.green;
        else
            crosshair.color = Color.red;
    }
}