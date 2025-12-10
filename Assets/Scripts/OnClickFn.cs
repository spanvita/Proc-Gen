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


    if (Camera.main == null)
    {
        Debug.LogError("Camera.main is NULL");
        return;
    }

    if (mapGen == null)
    {
        Debug.LogError("mapGen is NULL (drag MapGenerator into OnClickFn inspector!)");
        return;
    }

    if (mapGen.stateIds == null)
    {
        Debug.LogError("mapGen.stateIds is NULL (drawMode != StateMap OR GenerateMap not run)");
        return;
    }

    if (mapGen.ring == null)
    {
        Debug.LogError("mapGen.ring is NULL (you didn't assign 'ring = ...' in GenerateStateMap)");
        return;
    }

    Vector2 mousePos = Mouse.current.position.ReadValue();
    Ray ray = Camera.main.ScreenPointToRay(mousePos);
    RaycastHit hit;

    if (!Physics.Raycast(ray, out hit))
    {
        Debug.LogError("Raycast did NOT hit the mesh!");
        return;
    }

    Vector2 uv = hit.textureCoord;

    int x = Mathf.FloorToInt(uv.x * mapGen.mapWidth);
    int y = Mathf.FloorToInt(uv.y * mapGen.mapHeight);

    x = Mathf.Clamp(x, 0, mapGen.mapWidth - 1);
    y = Mathf.Clamp(y, 0, mapGen.mapHeight - 1);

    int id = mapGen.stateIds[x, y];

    if (id < 0 || id >= mapGen.ring.Length)
    {
        Debug.LogError($"Invalid ID {id}. ring length = {mapGen.ring.Length}");
        ShowPopup("Ocean");
        return;
    }

    int ringNo = mapGen.ring[id];
    
    ShowPopup("State ID: " + id + " ring no. " + ringNo);
}


    void ShowPopup(string msg)
    {
        popupText.text = msg;
        hideTime = Time.time + popupDuration;
    }
}
