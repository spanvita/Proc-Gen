using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class PerspectiveZoom : MonoBehaviour
{
    public float zoomSpeed = 20f;
    public float minFov = 20f;
    public float maxFov = 80f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = false;  //coz I am using perspective camera
    }

    void Update()
    {
        // Debug.LogError("UPDATE ENTERED");

        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        // Debug.LogError(Mouse.current.scroll.ReadValue().y);

        if (scroll == 0) return;

        cam.fieldOfView -= scroll * zoomSpeed * Time.deltaTime;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFov, maxFov);
    }
}
