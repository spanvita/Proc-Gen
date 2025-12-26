// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.InputSystem; 


// public class StateConnections : MonoBehaviour
// {

//     public MapGenerator mapGen;
//     public Transform mapPlane; 



//     Vector3 MapToWorld(Vector2Int p)
//     {
//         float u = (float)p.x / mapGen.mapWidth;
//         float v = 1f - (float)p.y / mapGen.mapHeight;


//         float planeWidth  = 10f * mapPlane.localScale.x;
//         float planeHeight = 10f * mapPlane.localScale.z;

//         float x = (u - 0.5f) * planeWidth;
//         float z = (v - 0.5f) * planeHeight;

//         return mapPlane.position + new Vector3(x, 5f, z);
//     }


//     void DrawLine(Vector3 a, Vector3 b)
//     {
//         GameObject lineObj = new GameObject("StateConnection");
//         LineRenderer lr = lineObj.AddComponent<LineRenderer>();

//         lr.positionCount = 2;
//         lr.SetPosition(0, a);
//         lr.SetPosition(1, b);

//         lr.startWidth = 1.5f;
//         lr.endWidth = 1.5f;

//         lr.material = new Material(Shader.Find("Sprites/Default"));
//         lr.startColor = Color.black;
//         lr.endColor = Color.black;

//         lr.useWorldSpace = true;
//     }

//     void timedelay()
//     {
//         if (mapGen.stateIds == null)
//         {
//             Invoke("start", 2f);
//         }
//         else
//         {
//             GenConnections();
//         }
        
//     }

//     void GenConnections()
//     {
//         List<Vector2> neighbors = new List<Vector2>();
        

//         for (int y = 0; y < mapGen.mapHeight-1; y++)
//         {
//             for (int x = 0; x < mapGen.mapWidth-1; x++)
//             {
//                 int stateId = mapGen.stateIds[x, y];
//                 if (stateId < 0)
//                     continue;

//                 if(stateId!=mapGen.stateIds[x+1,y])
//                 {
//                     if(!neighbors.Contains(new Vector2(stateId, mapGen.stateIds[x + 1, y])))
//                         neighbors.Add(new Vector2(stateId, mapGen.stateIds[x + 1, y]));
//                     Debug.Log("Connection between " + stateId + " and " + mapGen.stateIds[x + 1, y]);
//                 }
//                 if(stateId!=mapGen.stateIds[x,y+1])
//                 {
//                     if(!neighbors.Contains(new Vector2(stateId, mapGen.stateIds[x, y + 1])))
//                         neighbors.Add(new Vector2(stateId, mapGen.stateIds[x, y + 1]));
//                     Debug.Log("Connection between " + stateId + " and " + mapGen.stateIds[x, y + 1]);   
//                 }

//             }
//         }
        

//         for (int i = 0; i < neighbors.Count; i++)
//         {
//             int s1 = (int)neighbors[i].x;
//             int s2 = (int)neighbors[i].y;

//             Vector2Int c1 = mapGen.capitals[s1];
//             Vector2Int c2 = mapGen.capitals[s2];

//             Vector3 p1 = MapToWorld(c1);
//             Vector3 p2 = MapToWorld(c2);


//             DrawLine(p1, p2);

//             Debug.Log($"Neighbor Pair: {s1} <-> {s2}");
//         }


//     }
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         if (mapGen.stateIds == null)
//         {
//             Invoke("timedelay", 2f); 
//             Debug.LogError("delayed");
//         }
//         else
//         {
//             GenConnections();
//         }
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }


using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; 


public class StateConnections : MonoBehaviour
{
    public MapGenerator mapGen;
    
    [Header("Connection Line Settings")]
    public Color lineColor = Color.black;
    [Range(1, 10)]
    public int lineWidth = 1;
    
    // APPROACH: Draw directly on the texture instead of using 3D LineRenderers
    
    void Start()
    {
        if (mapGen.stateIds == null)
        {
            Debug.Log("State map not ready, waiting...");
            Invoke("DelayedDraw", 2f); 
        }
        else
        {
            DrawConnectionsOnTexture();
        }
    }

    void DelayedDraw()
    {
        if (mapGen.stateIds == null)
        {
            Invoke("DelayedDraw", 0.5f);
        }
        else
        {
            DrawConnectionsOnTexture();
        }
    }

    void DrawConnectionsOnTexture()
    {
        if (mapGen.latestTexture == null)
        {
            // Debug.LogError("No texture available to draw on!");
            return;
        }

        // Find all neighboring states
        HashSet<Vector2> neighborPairs = new HashSet<Vector2>();
        
        for (int y = 0; y < mapGen.mapHeight - 1; y++)
        {
            for (int x = 0; x < mapGen.mapWidth - 1; x++)
            {
                int stateId = mapGen.stateIds[x, y];
                if (stateId < 0)
                    continue;

                // Check right neighbor
                int rightState = mapGen.stateIds[x + 1, y];
                if (stateId != rightState && rightState >= 0)
                {
                    int s1 = Mathf.Min(stateId, rightState);
                    int s2 = Mathf.Max(stateId, rightState);
                    neighborPairs.Add(new Vector2(s1, s2));
                }

                // Check down neighbor
                int downState = mapGen.stateIds[x, y + 1];
                if (stateId != downState && downState >= 0)
                {
                    int s1 = Mathf.Min(stateId, downState);
                    int s2 = Mathf.Max(stateId, downState);
                    neighborPairs.Add(new Vector2(s1, s2));
                }
            }
        }

        Debug.Log($"Drawing {neighborPairs.Count} connections between states");

        // Get texture pixels
        Color[] pixels = mapGen.latestTexture.GetPixels();

        // Draw line for each connection
        foreach (Vector2 pair in neighborPairs)
        {
            int s1 = (int)pair.x;
            int s2 = (int)pair.y;

            if (s1 >= mapGen.capitals.Length || s2 >= mapGen.capitals.Length)
                continue;

            Vector2Int c1 = mapGen.capitals[s1];
            Vector2Int c2 = mapGen.capitals[s2];

            // Draw line directly on texture using Bresenham's line algorithm
            DrawLineOnTexture(pixels, c1.x, c1.y, c2.x, c2.y, lineColor, mapGen.mapWidth, mapGen.mapHeight);
        }

        // Apply changes to texture
        mapGen.latestTexture.SetPixels(pixels);
        mapGen.latestTexture.Apply();

        Debug.Log("Connections drawn on texture!");
    }

    void DrawLineOnTexture(Color[] pixels, int x0, int y0, int x1, int y1, Color color, int width, int height)
    {
        // Bresenham's line algorithm
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Set pixel (with bounds checking) - draw line with specified width
            for (int offset = -(lineWidth / 2); offset <= (lineWidth / 2); offset++)
            {
                // Draw horizontally offset pixels for thickness
                int px = x0 + offset;
                if (px >= 0 && px < width && y0 >= 0 && y0 < height)
                {
                    int index = y0 * width + px;
                    pixels[index] = color;
                }
                
                // Also draw vertically offset pixels for smoother thick lines
                int py = y0 + offset;
                if (x0 >= 0 && x0 < width && py >= 0 && py < height)
                {
                    int index = py * width + x0;
                    pixels[index] = color;
                }
            }

            // Check if we've reached the end
            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    void Update()
    {
        
    }
}