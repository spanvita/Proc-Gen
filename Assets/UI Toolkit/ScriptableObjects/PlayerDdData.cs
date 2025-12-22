using UnityEngine;
[CreateAssetMenu(fileName = "PlayerDdData", menuName = "ScriptableObjects/PlayerDdData"  )]
public class PlayerDdData : ScriptableObject
{
    public string WinnerStateName;
    public string LoserStateName;
    public string leftLine1;
    public string leftLine2;
    public string leftLine3;
    public string rightLine1;
    public string rightLine2;
    public string rightLine3;
    public int leftLine1Value;
    public int leftLine2Value;
    public int leftLine3Value;
    public int rightLine1Value;
    public int rightLine2Value;
    public int rightLine3Value;
    public int leftLine1Points;
    public int leftLine2Points;
    public int leftLine3Points;
    public int rightLine1Points;
    public int rightLine2Points;
    public int rightLine3Points;
    [HideInInspector] public Sprite player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(leftLine1=="striker")
        {
            player=Resources.Load<Sprite>("striker");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
