// using UnityEngine;
// using UnityEngine.InputSystem;

// [RequireComponent(typeof(Camera))]
// public class PerspectiveZoom : MonoBehaviour
// {
//     public float zoomSpeed = 20f;
//     public float minFov = 20f;
//     public float maxFov = 80f;

//     Camera cam;

//     void Awake()
//     {
//         cam = GetComponent<Camera>();
//         cam.orthographic = false;  //coz I am using perspective camera
//     }

//     void Update()
//     {
//         // Debug.LogError("UPDATE ENTERED");

//         if (Mouse.current == null) return;

//         float scroll = Mouse.current.scroll.ReadValue().y;
//         // Debug.LogError(Mouse.current.scroll.ReadValue().y);

//         if (scroll == 0) return;

//         cam.fieldOfView -= scroll * zoomSpeed * Time.deltaTime;
//         cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFov, maxFov);
//     }
// }



using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class OrthographicZoomToCursor : MonoBehaviour
{
    public float zoomSpeed = 5f;
    public float minSize = 50f;
    public float maxSize = 2000f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        //World position BEFORE zoom (under cursor)
        Vector3 mouseWorldBefore = GetMouseWorldPoint();

        // Zoom (orthographic size)
        cam.orthographicSize -= scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minSize, maxSize);

        //World position AFTER zoom
        Vector3 mouseWorldAfter = GetMouseWorldPoint();

        //Move camera so the point under cursor stays fixed
        Vector3 delta = mouseWorldBefore - mouseWorldAfter;
        cam.transform.position += delta;
    }

    Vector3 GetMouseWorldPoint()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        // Assumes your map is on Y = 0 plane
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
}
