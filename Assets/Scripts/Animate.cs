using UnityEngine;

public class Animate : MonoBehaviour
{
    public float speed = 1.0f;
    public int x1, y1, x2, y2;

    private Vector3 targetPosition;

    void Start()
    {
        transform.position = new Vector3(x1, 0, y1);

        targetPosition = new Vector3(x2, 0, y2);

       
    Debug.Log("Tree world pos: " + transform.position);


    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            step
        );

        if (transform.position == targetPosition)
        {
            enabled = false; 
        }
    }
}
