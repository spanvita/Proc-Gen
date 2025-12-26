using UnityEngine;
using UnityEngine.InputSystem;   // NEW INPUT SYSTEM
using TMPro;
using UnityEngine.UIElements;


public class OnClickFn : MonoBehaviour
{
    public MapGenerator mapGen;
    public TextMeshProUGUI popupText;
    public GameObject rightClickMenu;  
    public UIDocument uiDoc;

    private VisualElement root;
    private VisualElement popup;


    public float popupDuration = 5f;

    private float hideTime = 0f;

    void Start()
{
    if (uiDoc == null)
    {
        // Debug.LogError("OnClickFn: uiDoc is null — assign the UIDocument GameObject in the Inspector.");
        return;
    }

    root = uiDoc.rootVisualElement;
    if (root == null)
    {
        // Debug.LogError("OnClickFn: uiDoc.rootVisualElement is null. Make sure the UIDocument has a Source Asset (UXML) assigned.");
        return;
    }

    // IMPORTANT: match the name exactly (case-sensitive)
    // If your VisualElement name in UI Builder is "popup" (lowercase), use "popup" here.
    popup = root.Q<VisualElement>("popup"); // <--- use the exact name from UI Builder

    if (popup == null)
    {
        // Debug.LogError("OnClickFn: couldn't find VisualElement named 'popup'.\n" +
                    //    "Open UI Builder and check the NAME of the root VisualElement (wrench/3-dots menu).");
        // Optional: list all children so you can see what's available:
        foreach (var child in root.Children())
            Debug.Log("root child: " + child.name);
        return;
    }

    // now safe to use
    popup.style.display = DisplayStyle.None;
}



    void Update()
    {
        // Hide popup
        if (popupText != null && Time.time > hideTime)
            popupText.text = "";

        // Detect click using New Input System
        if (Mouse.current.leftButton.wasPressedThisFrame)
            HandleClick();
        if (Mouse.current.rightButton.wasPressedThisFrame)
            ShowUXMLPopup();
        

        
    }

    void HandleClick()
{


    if (Camera.main == null)
    {
        // Debug.LogError("Camera.main is NULL");
        return;
    }

    if (mapGen == null)
    {
        // Debug.LogError("mapGen is NULL (drag MapGenerator into OnClickFn inspector!)");
        return;
    }

    if (mapGen.stateIds == null)
    {
        // Debug.LogError("mapGen.stateIds is NULL (drawMode != StateMap OR GenerateMap not run)");
        return;
    }

    if (mapGen.ring == null)
    {
        // Debug.LogError("mapGen.ring is NULL (you didn't assign 'ring = ...' in GenerateStateMap)");
        return;
    }

    Vector2 mousePos = Mouse.current.position.ReadValue();
    Ray ray = Camera.main.ScreenPointToRay(mousePos);
    RaycastHit hit;

    if (!Physics.Raycast(ray, out hit))
    {
        // Debug.LogError("Raycast did NOT hit the mesh!");
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
        // Debug.LogError($"Invalid ID {id}. ring length = {mapGen.ring.Length}");
        ShowPopup("Ocean");
        return;
    }

    int ringNo = mapGen.ring[id];
    
    ShowPopup("State ID: " + id + " ring no. " + ringNo);
    Debug.LogError("x,z in world coords: " + hit.point);
}


    // void PopUp()
    // {
    //     Debug.Log("Right click detected!");
    //     if (Camera.main == null)
    // {
    //     Debug.LogError("Camera.main is NULL");
    //     return;
    // }

    // if (mapGen == null)
    // {
    //     Debug.LogError("mapGen is NULL (drag MapGenerator into OnClickFn inspector!)");
    //     return;
    // }

    // if (mapGen.stateIds == null)
    // {
    //     Debug.LogError("mapGen.stateIds is NULL (drawMode != StateMap OR GenerateMap not run)");
    //     return;
    // }

    // if (mapGen.ring == null)
    // {
    //     Debug.LogError("mapGen.ring is NULL (you didn't assign 'ring = ...' in GenerateStateMap)");
    //     return;
    // }

    // Vector2 mousePos = Mouse.current.position.ReadValue();
    
    // RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //     rightClickMenu.transform.parent as RectTransform,
    //     mousePos,
    //     null, // main canvas camera (if screen space camera)
    //     out Vector2 localPoint
    // );

    // rightClickMenu.GetComponent<RectTransform>().anchoredPosition = localPoint;

    // // SHOW THE MENU
    // rightClickMenu.SetActive(true);

    // }

    void ShowUXMLPopup()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        popup.style.left = mousePos.x;
        popup.style.top = mousePos.y - popup.resolvedStyle.height;


        popup.style.display = DisplayStyle.Flex;
    }



    void ShowPopup(string msg)
    {
        popupText.text = msg;
        hideTime = Time.time + popupDuration;
    }
}
