using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class UIController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] PlayerDdData initialPlayerData;

    // Cached UI references
    VisualElement root;
    VisualElement fight;
    VisualElement left;
    VisualElement l1;
    VisualElement r1;
    VisualElement l2;
    VisualElement r2;
    VisualElement l3;
    VisualElement r3;
    Label label_left;
    Label label_right;
    Toggle viewFight;

    bool layoutReady = false;
    bool moving1 = true;
    bool moving2 = true;
    bool moving3 = true;
    bool line1Collision = false;
    bool line2Collision = false;
    bool line3Collision = false;
    float l1X, r1X, l2X, r2X, l3X, r3X;

    float labelLeftY, labelRightY;

    void Start()
    {
        CacheUI();
        BindData();
        RegisterLayoutCallback();
        StartCoroutine(InitSpriteAfterBind());
    }

    // --------------------------------------------------
    // UI SETUP
    // --------------------------------------------------

    void CacheUI()
    {
        root = uiDocument.rootVisualElement;

        viewFight = root.Q<Toggle>("viewFight");
        // if (viewFight == null) { Debug.LogError("viewFight missing"); };

        fight = root.Q<VisualElement>("fight");
        // if (fight == null) { Debug.LogError("fight missing"); return; }

        label_left = fight.Q<Label>("label_left");
        label_right = fight.Q<Label>("label_right");

        left = fight.Q<VisualElement>("left");
        // if (left == null) { Debug.LogError("left missing"); return; }

        l1 = left.Q<VisualElement>("l1");
        l2 = left.Q<VisualElement>("l2");
        l3 = left.Q<VisualElement>("l3");
        r1 = left.Q<VisualElement>("r1");
        r2 = left.Q<VisualElement>("r2");
        r3 = left.Q<VisualElement>("r3");

        // if (l1 == null) Debug.LogError("l1 missing");
        // if (l2 == null) Debug.LogError("l2 missing");
        // if (l3 == null) Debug.LogError("l3 missing");
        // if (r1 == null) Debug.LogError("r1 missing");
        // if (r2 == null) Debug.LogError("r2 missing");
        // if (r3 == null) Debug.LogError("r3 missing");
    }

    void BindData()
    {
        fight.dataSource = initialPlayerData;
    }

    void RegisterLayoutCallback()
    {
        // Register ONCE, immediately
        l1.RegisterCallback<GeometryChangedEvent>(OnLayoutReady);
    }

    void OnLayoutReady(GeometryChangedEvent evt)
    {
        layoutReady = true;

        // Prepare movement
        l1.style.position = Position.Absolute;
        r1.style.position = Position.Absolute;
        l2.style.position = Position.Absolute;
        r2.style.position = Position.Absolute;
        l3.style.position = Position.Absolute;
        r3.style.position = Position.Absolute;
        l1X= l1.resolvedStyle.left;
        r1X= r1.resolvedStyle.left;
        l2X= l2.resolvedStyle.left;
        r2X= r2.resolvedStyle.left;
        l3X= l3.resolvedStyle.left;
        r3X= r3.resolvedStyle.left;
        l1.style.left = l1X;
        r1.style.left = r1X;
        l2.style.left = l2X;
        r2.style.left = r2X;    
        l3.style.left = l3X;
        r3.style.left = r3X;

        label_left.style.position = Position.Absolute;
        label_right.style.position = Position.Absolute;

        labelLeftY = label_left.resolvedStyle.top;
        labelRightY = label_right.resolvedStyle.top;

        label_left.style.top = labelLeftY;
        label_right.style.top = labelRightY;


        Debug.Log($"Layout ready → L1: {l1.resolvedStyle.left}, R1: {r1.resolvedStyle.left}");

        // Unregister (fires only once)
        l1.UnregisterCallback<GeometryChangedEvent>(OnLayoutReady);
    }

    // --------------------------------------------------
    // DATA / SPRITE INIT
    // --------------------------------------------------

    IEnumerator InitSpriteAfterBind()
    {
        // Wait until data binding + layout stabilize
        yield return null;
        yield return null;

        if (initialPlayerData.leftLine1 == "striker")
        {
            initialPlayerData.player = Resources.Load<Sprite>("striker");

            if (initialPlayerData.player == null)
                 Debug.Log("Sprite NOT loaded (check Resources/striker.png)");
            else
                Debug.Log("Sprite loaded successfully");
        }
    }

    //-------------------------------------------------

   void Update()
    {
        
        if (viewFight != null)
    {
        fight.visible = viewFight.value; //true or false
    }   

        if (!layoutReady) return;

        if (moving1)
        {
            if (r1X - l1X <= 64f)
            {
                l1.visible = false;
                r1.visible = false;
                moving1 = false;
                line1Collision = true;

                label_left.text = initialPlayerData.leftLine1Points.ToString();
                label_right.text = initialPlayerData.rightLine1Points.ToString();
                ResetLabels();
            }
            else
            {
                l1X += 1f;
                r1X -= 1f;
                l1.style.left = l1X;
                r1.style.left = r1X;
            }
        }

        if (line1Collision && moving2)
        {
            AnimateLabels();

            if (r2X - l2X <= 64f)
            {
                l2.visible = false;
                r2.visible = false;
                moving2 = false;
                line2Collision = true;

                label_left.text = initialPlayerData.leftLine2Points.ToString();
                label_right.text = initialPlayerData.rightLine2Points.ToString();
                ResetLabels();
            }
            else
            {
                l2X += 1f;
                r2X -= 1f;
                l2.style.left = l2X;
                r2.style.left = r2X;
            }
        }

        if (line2Collision && moving3)
        {
            AnimateLabels();

            if (r3X - l3X <= 64f)
            {
                l3.visible = false;
                r3.visible = false;
                moving3 = false;
                line3Collision = true;

                label_left.text = initialPlayerData.leftLine3Points.ToString();
                label_right.text = initialPlayerData.rightLine3Points.ToString();
                ResetLabels();
            }
            else
            {
                l3X += 1f;
                r3X -= 1f;
                l3.style.left = l3X;
                r3.style.left = r3X;
            }
        }
        if(line3Collision)
        {
            AnimateLabels();
        }
    }

    void ResetLabels()
    {
        labelLeftY = 100f;
        labelRightY = 100f;

        label_left.visible = true;
        label_right.visible = true;

        

        label_left.style.top = labelLeftY;
        label_right.style.top = labelRightY;
    }

    void AnimateLabels()
    {
        labelLeftY -= 0.5f;
        labelRightY -= 0.5f;

        label_left.style.top = labelLeftY;
        label_right.style.top = labelRightY;

        if (labelLeftY <= 0f)
        {
            label_left.visible = false;
            label_right.visible = false;
        }
    }

}
