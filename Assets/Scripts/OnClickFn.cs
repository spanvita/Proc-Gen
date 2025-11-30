using UnityEngine;
using UnityEngine.InputSystem;   // NEW INPUT SYSTEM
using TMPro;

public class OnClickFn : MonoBehaviour
{
    public MapGenerator mapGen;
    public TextMeshProUGUI popupText;
    public float popupDuration = 5f;

    private float hideTime = 0f;

    void Update()
    {
        // Hide popup
        if (popupText != null && Time.time > hideTime)
            popupText.text = "";

        // Detect click using New Input System
        if (Mouse.current.leftButton.wasPressedThisFrame)
            HandleClick();
    }

    void HandleClick()
    {
        if (Camera.main == null) return;

        // NEW INPUT SYSTEM mouse position
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector2 uv = hit.textureCoord;

            int x = Mathf.FloorToInt(uv.x * mapGen.mapWidth);
            int y = Mathf.FloorToInt(uv.y * mapGen.mapHeight);

            int id = mapGen.stateIds[x, y];

            ShowPopup("State ID: " + id);
        }
    }

    void ShowPopup(string msg)
    {
        popupText.text = msg;
        hideTime = Time.time + popupDuration;
    }
}
