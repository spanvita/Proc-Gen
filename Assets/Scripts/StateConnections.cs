using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; 


public class StateConnections : MonoBehaviour
{

    public MapGenerator mapGen;
    public Transform mapPlane; 



    Vector3 MapToWorld(Vector2Int p)
    {
        float u = (float)p.x / mapGen.mapWidth;
        float v = 1f - (float)p.y / mapGen.mapHeight;


        float planeWidth  = 10f * mapPlane.localScale.x;
        float planeHeight = 10f * mapPlane.localScale.z;

        float x = (u - 0.5f) * planeWidth;
        float z = (v - 0.5f) * planeHeight;

        return mapPlane.position + new Vector3(x, 5f, z);
    }


    void DrawLine(Vector3 a, Vector3 b)
    {
        GameObject lineObj = new GameObject("StateConnection");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        lr.startWidth = 1.5f;
        lr.endWidth = 1.5f;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.black;
        lr.endColor = Color.black;

        lr.useWorldSpace = true;
    }

    void timedelay()
    {
        if (mapGen.stateIds == null)
        {
            Invoke("start", 2f);
        }
        else
        {
            GenConnections();
        }
        
    }

    void GenConnections()
    {
        List<Vector2> neighbors = new List<Vector2>();
        

        for (int y = 0; y < mapGen.mapHeight-1; y++)
        {
            for (int x = 0; x < mapGen.mapWidth-1; x++)
            {
                int stateId = mapGen.stateIds[x, y];
                if (stateId < 0)
                    continue;

                if(stateId!=mapGen.stateIds[x+1,y])
                {
                    if(!neighbors.Contains(new Vector2(stateId, mapGen.stateIds[x + 1, y])))
                        neighbors.Add(new Vector2(stateId, mapGen.stateIds[x + 1, y]));
                    Debug.Log("Connection between " + stateId + " and " + mapGen.stateIds[x + 1, y]);
                }
                if(stateId!=mapGen.stateIds[x,y+1])
                {
                    if(!neighbors.Contains(new Vector2(stateId, mapGen.stateIds[x, y + 1])))
                        neighbors.Add(new Vector2(stateId, mapGen.stateIds[x, y + 1]));
                    Debug.Log("Connection between " + stateId + " and " + mapGen.stateIds[x, y + 1]);   
                }

            }
        }
        

        for (int i = 0; i < neighbors.Count; i++)
        {
            int s1 = (int)neighbors[i].x;
            int s2 = (int)neighbors[i].y;

            Vector2Int c1 = mapGen.capitals[s1];
            Vector2Int c2 = mapGen.capitals[s2];

            Vector3 p1 = MapToWorld(c1);
            Vector3 p2 = MapToWorld(c2);


            DrawLine(p1, p2);

            Debug.Log($"Neighbor Pair: {s1} <-> {s2}");
        }


    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mapGen.stateIds == null)
        {
            Invoke("timedelay", 2f); 
            Debug.LogError("delayed");
        }
        else
        {
            GenConnections();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
