using UnityEngine;

public class Placing : MonoBehaviour
{
    public int xcord;
    public int ycord;
    public GameObject tree;
    private int scaleFactor = 100;

    void Start()
{
    Debug.LogError("START ENTERED"); // ERROR, not Log

    GameObject treeInstance = Instantiate(
        tree,
        new Vector3(xcord, 0, ycord),
        Quaternion.identity
    );

    Debug.LogError("AFTER INSTANTIATE");

    treeInstance.transform.localScale = Vector3.one * 100;

    Debug.LogError("AFTER SCALE");
}

}
