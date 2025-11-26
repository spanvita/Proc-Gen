
                                                                                                            
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapDisplay : MonoBehaviour 
{

    public static void FloodFill(
    float[,] noiseMap,
    int[,] states,
    int x,
    int y,
    int stateIndex,
    int width,
    int height,
    Color[] colourMap,
    Color[] colors,
    List<List<Vector2Int>> statexys,
    float tolerance)
    {
        float origin = noiseMap[x, y];

        // queue for BFS expansion
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(x, y));
        int pix=0;
        while (q.Count > 0)
        {
            Vector2Int p = q.Dequeue();
            int i = p.x;
            int j = p.y;

             if(pix++>5000)
                return;

            // bounds check
            if (i < 0 || i  >= width || j < 0 || j >= height)
                continue;

            // already filled
            if (states[i, j] != 0)
                continue;

            // noise mismatch
            if (Mathf.Abs(noiseMap[i, j] - origin) > tolerance)
                continue;

            // assign state
            states[i, j] = stateIndex;

            // store all pixels   of this state
            statexys[stateIndex].Add(new Vector2Int(i, j));


            // // assign color
            // colourMap[j * width + i] = colors[stateIndex - 1];

            if (i >= 0 && i < width && j >= 0 && j < height && (j * width + i) >= 0 && (j * width + i) < colourMap.Length)
            colourMap[j * width + i] = colors[stateIndex - 1];


            // enqueue neighbors
            if(i+1<width)
            q.Enqueue(new Vector2Int(i + 1, j));
            if(i-1>=0)
            q.Enqueue(new Vector2Int(i - 1, j));
            if(j+1<height)
            q.Enqueue(new Vector2Int(i, j + 1));
            if(j-1>=0) 
            q.Enqueue(new Vector2Int(i, j - 1));
        }
    }



    void FillInternalHoles(int[,] states, int width, int height, int stateIndex, Color[] colourMap, Color fillColor,List<List<Vector2Int>> statexys)
    {
        // Mark visited pixels
        bool[,] visited = new bool[width, height];

        // Temporary markers for hole area
        List<Vector2Int> holePixels = new List<Vector2Int>();

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        // Loop over all pixels
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Only consider empty pixels
                if (states[x,y] != 0 || visited[x,y])
                    continue;

                holePixels.Clear();
                bool touchesOutside = false;

                // BFS to inspect connected empty region
                Queue<Vector2Int> q = new Queue<Vector2Int>();
                q.Enqueue(new Vector2Int(x, y));
                visited[x,y] = true;
                holePixels.Add(new Vector2Int(x,y));

                while (q.Count > 0)
                {
                    Vector2Int p = q.Dequeue();
                    int px = p.x;
                    int py = p.y;

                    // If empty region touches map boundary → not a hole
                    if (px == 0 || px == width-1 || py == 0 || py == height-1)
                    {
                        touchesOutside = true;
                    }

                    // explore neighbors
                    for (int k = 0; k < 4; k++)
                    {
                        int nx = px + dx[k];
                        int ny = py + dy[k];

                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;

                        if (!visited[nx,ny] && states[nx,ny] == 0)
                        {
                            visited[nx,ny] = true;
                            q.Enqueue(new Vector2Int(nx,ny));
                            holePixels.Add(new Vector2Int(nx,ny));
                        }
                    }
                }

                // If region did NOT touch outer map border → it is a true hole
                if (!touchesOutside)
                {
                    foreach (var p in holePixels)
                    {
                        states[p.x,p.y] = stateIndex;
                        colourMap[p.y * width + p.x] = fillColor;
                        statexys[stateIndex].Add(p);
                    }
                }
            }
        }
    }



	public Renderer textureRender;

	public void DrawNoiseMap(float[,] noiseMap) {
        // Debug.Log("Drawing Noise Map");
		int width = noiseMap.GetLength (0);
		int height = noiseMap.GetLength (1);

        int [,] states=new int [width,height];

        for(int y=0;y<height;y++)
            {
                for(int x=0;x<width;x++)
                {
                    states[x,y]=0;
                }
            }


        
        // Vector2[] statexys=new Vector2[21];

        
        List<List<Vector2Int>> statexys = new List<List<Vector2Int>>();

        for (int i = 0; i <=20; i++)   // supports up to 1000 states
        {
            statexys.Add(new List<Vector2Int>());
        }



        Color[] colors={Color.blue, Color.yellow, Color.gray, Color.green, Color.cyan, Color.magenta, Color.red, Color.orange, Color.white,
                        new Color(0.5f,0.2f,0.7f), new Color(0.1f,0.8f,0.3f), new Color(0.9f,0.6f,0.1f),
                        new Color(0.4f,0.4f,0.4f), new Color(0.2f,0.5f,0.9f), new Color(0.7f,0.3f,0.2f),
                        new Color(0.3f,0.7f,0.4f), new Color(0.8f,0.1f,0.5f), new Color(0.6f,0.9f,0.2f),
                        new Color(0.2f,0.2f,0.8f), new Color(0.9f,0.4f,0.7f), new Color(0.5f,0.9f,0.3f)};

		Texture2D texture = new Texture2D (width, height);

		// Color[] colourMap_draft = new Color[width * height];
        Color[] colourMap = new Color[width * height];
		
        int stateIndex=1;
        {
            for(int y=0;y<height;y++)
            {
                for(int x=0;x<width;x++)
                {   
                    if(states[x,y]==0)
                    {
                        float tolerance=0.25f;
                        FloodFill(noiseMap , states, x, y, stateIndex, width, height, colourMap, colors,  statexys, tolerance);
                        FillInternalHoles(states, width, height, stateIndex, colourMap, colors[stateIndex - 1],statexys);
                        stateIndex++;
                    }
                }
            }
        }

        Vector2[] capital=new Vector2[stateIndex];

        for(int i=1;i<stateIndex;i++)
        {
            if (statexys[i].Count == 0) 
                continue;

            float sumX = 0f;
            float sumY = 0f;

            foreach (var p in statexys[i])
            {
                sumX += p.x;
                sumY += p.y;
            }

            int cx = Mathf.RoundToInt(sumX / statexys[i].Count);
            int cy = Mathf.RoundToInt(sumY / statexys[i].Count);

            // int minX = int.MaxValue, maxX = int.MinValue;
            // int minY = int.MaxValue, maxY = int.MinValue;

            // foreach (var p in statexys[i])
            // {
            //     if (p.x < minX) minX = p.x;
            //     if (p.x > maxX) maxX = p.x;

            //     if (p.y < minY) minY = p.y;
            //     if (p.y > maxY) maxY = p.y;
            // }

            // int cx = (minX + maxX) / 2;
            // int cy = (minY + maxY) / 2;


            capital[i] = new Vector2Int(cx, cy);

            colourMap[cy*width+cx]=Color.black;
            if(cx+1<width )
            colourMap[cy*width+cx+1]=Color.black;
            if(cx-1>=0)
            colourMap[cy*width+cx-1]=Color.black;
            if(cy+1<height)
            colourMap[(cy+1)*width+cx]=Color.black;
            if(cy-1>=0)
            colourMap[(cy-1)*width+cx]=Color.black;

        }



        

        



		texture.SetPixels (colourMap);
		texture.Apply ();

		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (width, 1, height);

        
	}
	
}



 





//  //NOTES

//  //cotaves =0




























 




// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// public class MapDisplay : MonoBehaviour 
// {

//     public static void FloodFill(
//     float[,] noiseMap,
//     int[,] states,
//     int x,
//     int y,
//     int stateIndex,
//     int width,
//     int height,
//     Color[] colourMap,
//     Color[] colors,
//     Vector2[] statexys,
//     float tolerance)
//     {

//         /////public static void FloodFill(float [,] noiseMap, int[,] states, int x, int y, int stateIndex,  int width, int height, Color[] colourMap, Color[] colors, Vector2[] statexys, float tolerance)
//     // {

//     //     float origin = noiseMap[x, y];

//     //     void Fill(int i, int j) 
//     //     {
            
//     //         {
//     //             if (i < 0 || i >=width || j < 0 || j >= height)
//     //                 return;

//     //             if (states[i, j] != 0)
//     //                 return;

//     //             if (Mathf.Abs(noiseMap[i, j] - origin) > tolerance)
//     //                 return;
                

//     //             states[i, j] = stateIndex;
//     //             statexys[stateIndex] = new Vector2(i, j);
//     //             colourMap[j * width + i] = colors[stateIndex - 1];

                

//     //             Fill(i + 1, j);
//     //             Fill(i - 1, j);
//     //             Fill(i, j + 1);
//     //             Fill(i, j - 1);
//     //         }
//     //     }

//     //     Fill(x, y);
//     // }




//         float origin = noiseMap[x, y];

//         // queue for BFS expansion
//         Queue<Vector2Int> q = new Queue<Vector2Int>();
//         q.Enqueue(new Vector2Int(x, y));
//         int pix=0;
//         while (q.Count > 0)
//         {
//             Vector2Int p = q.Dequeue();
//             int i = p.x;
//             int j = p.y;

//             // if(pix++>30)
//             //     return;

//             // bounds check
//             if (i < 0 || i >= width || j < 0 || j >= height)
//                 continue;

//             // already filled
//             if (states[i, j] != 0)
//                 continue;

//             // noise mismatch
//             if (Mathf.Abs(noiseMap[i, j] - origin) > tolerance)
//                 continue;

//             // assign state
//             states[i, j] = stateIndex;

//             // store last visited pixel (your original behavior)
//             statexys[stateIndex] = new Vector2(i, j);

//             // // assign color
//             // colourMap[j * width + i] = colors[stateIndex - 1];

//             if (i >= 0 && i < width && j >= 0 && j < height && (j * width + i) >= 0 && (j * width + i) < colourMap.Length)
//             colourMap[j * width + i] = colors[stateIndex - 1];


//             // enqueue neighbors
//             if(i+1<width)
//             q.Enqueue(new Vector2Int(i + 1, j));
//             if(i-1>=0)
//             q.Enqueue(new Vector2Int(i - 1, j));
//             if(j+1<height)
//             q.Enqueue(new Vector2Int(i, j + 1));
//             if(j-1>=0) 
//             q.Enqueue(new Vector2Int(i, j - 1));
//         }
//     }



// 	public Renderer textureRender;

// 	public void DrawNoiseMap(float[,] noiseMap) {
//         // Debug.Log("Drawing Noise Map");
// 		int width = noiseMap.GetLength (0);
// 		int height = noiseMap.GetLength (1);

//         int [,] states=new int [width,height];

//         for(int y=0;y<height;y++)
//             {
//                 for(int x=0;x<width;x++)
//                 {
//                     states[x,y]=0;
//                 }
//             }


        
//         Vector2[] statexys=new Vector2[101];
//         Color[] colors={Color.blue, Color.yellow, Color.gray, Color.green, Color.cyan, Color.magenta, Color.red, Color.black, Color.white,
//                         new Color(0.5f,0.2f,0.7f), new Color(0.1f,0.8f,0.3f), new Color(0.9f,0.6f,0.1f),
//                         new Color(0.4f,0.4f,0.4f), new Color(0.2f,0.5f,0.9f), new Color(0.7f,0.3f,0.2f),
//                         new Color(0.3f,0.7f,0.4f), new Color(0.8f,0.1f,0.5f), new Color(0.6f,0.9f,0.2f),
//                         new Color(0.2f,0.2f,0.8f), new Color(0.9f,0.4f,0.7f), new Color(0.5f,0.9f,0.3f).
//                         Color.blue, Color.yellow, Color.gray, Color.green, Color.cyan, Color.magenta, Color.red, Color.black, Color.white,
//                         new Color(0.5f,0.2f,0.7f), new Color(0.1f,0.8f,0.3f), new Color(0.9f,0.6f,0.1f),
//                         new Color(0.4f,0.4f,0.4f), new Color(0.2f,0.5f,0.9f), new Color(0.7f,0.3f,0.2f),
//                         new Color(0.3f,0.7f,0.4f), new Color(0.8f,0.1f,0.5f), new Color(0.6f,0.9f,0.2f),
//                         new Color(0.2f,0.2f,0.8f), new Color(0.9f,0.4f,0.7f), new Color(0.5f,0.9f,0.3f),
//                         Color.blue, Color.yellow, Color.gray, Color.green, Color.cyan, Color.magenta, Color.red, Color.black, Color.white,
//                         new Color(0.5f,0.2f,0.7f), new Color(0.1f,0.8f,0.3f), new Color(0.9f,0.6f,0.1f),
//                         new Color(0.4f,0.4f,0.4f), new Color(0.2f,0.5f,0.9f), new Color(0.7f,0.3f,0.2f),
//                         new Color(0.3f,0.7f,0.4f), new Color(0.8f,0.1f,0.5f), new Color(0.6f,0.9f,0.2f),
//                         new Color(0.2f,0.2f,0.8f), new Color(0.9f,0.4f,0.7f), new Color(0.5f,0.9f,0.3f),
//                         Color.blue, Color.yellow, Color.gray, Color.green, Color.cyan, Color.magenta, Color.red, Color.black, Color.white,
//                         new Color(0.5f,0.2f,0.7f), new Color(0.1f,0.8f,0.3f), new Color(0.9f,0.6f,0.1f),
//                         new Color(0.4f,0.4f,0.4f), new Color(0.2f,0.5f,0.9f), new Color(0.7f,0.3f,0.2f),
//                         new Color(0.3f,0.7f,0.4f), new Color(0.8f,0.1f,0.5f), new Color(0.6f,0.9f,0.2f),
//                         new Color(0.2f,0.2f,0.8f), new Color(0.9f,0.4f,0.7f), new Color(0.5f,0.9f,0.3f)};

// 		Texture2D texture = new Texture2D (width, height);

// 		// Color[] colourMap_draft = new Color[width * height];
//         Color[] colourMap = new Color[width * height];
		
//         int stateIndex=1;
//         {
//             for(int y=0;y<height;y++)
//             {
//                 for(int x=0;x<width;x++)
//                 {
//                     if(states[x,y]==0)
//                     {
//                         float tolerance=0.05f;
//                         FloodFill(noiseMap , states, x, y, stateIndex, width, height, colourMap, colors,  statexys, tolerance);
//                         stateIndex++;
//                     }
//                 }
//             }
//         }
        

//         // for (int y = 0; y < height; y++) {
// 		// 	for (int x = 0; x < width; x++) {

//         //         for(int i=1;i<=20;i++)
//         //         {
//         //             if(noiseMap[x,y]<=i*0.05f)
//         //             {
//         //                 Color randomColor = new Color(Random.value, Random.value, Random.value, 1f);
//         //                 colourMap_draft[y * width + x]=colors[i-1];
//         //                 break;
//         //             }
//         //         }

//         //         // if(noiseMap[x,y]<=0.05f)
//         //         // {
//         //         //     colourMap[y * width + x] = Color.blue;
//         //         // }
//         //         // else if(noiseMap[x,y]<=0.3f)
//         //         // {
//         //         //     colourMap[y * width + x] = new Color(0.76f,0.7f,0.5f); //sand color
//         //         // }    
//         //         // else if(noiseMap[x,y]<=0.6f)
//         //         // {
//         //         //     colourMap[y * width + x] = Color.gray;
//         //         // }
//         //         // else
//         //         // {   
//         //         //     colourMap[y * width + x] = Color.green;
//         //         // }


// 		// 		//colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, noiseMap [x, y]);
// 		// 	}
// 		// }



// 		texture.SetPixels (colourMap);
// 		texture.Apply ();

// 		textureRender.sharedMaterial.mainTexture = texture;
// 		textureRender.transform.localScale = new Vector3 (width, 1, height);

//         // 

//         // int pixno=0;
//         // while (pixno<25)
//         // {

//         // }
// 	}
	
// }

// // using UnityEngine;
// // using System.Collections;

// // public class MapDisplay : MonoBehaviour {

// // 	public Renderer textureRender;

// // 	public void DrawTexture(Texture2D texture) {
// // 		textureRender.sharedMaterial.mainTexture = texture;
// // 		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height);
// // 	}
	
// // }

 





//  //NOTES

//  //cotaves =0 





// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// public class MapDisplay : MonoBehaviour 
// {

//     public static void FloodFill(
//     float[,] noiseMap,
//     int[,] states,
//     int x,
//     int y,
//     int stateIndex,
//     int width,
//     int height,
//     Color[] colourMap,
//     Color[] colors,
//     Vector2[] statexys,
//     float tolerance)
//     {

//         /////public static void FloodFill(float [,] noiseMap, int[,] states, int x, int y, int stateIndex,  int width, int height, Color[] colourMap, Color[] colors, Vector2[] statexys, float tolerance)
//     // {

//     //     float origin = noiseMap[x, y];

//     //     void Fill(int i, int j) 
//     //     {
            
//     //         {
//     //             if (i < 0 || i >=width || j < 0 || j >= height)
//     //                 return;

//     //             if (states[i, j] != 0)
//     //                 return;

//     //             if (Mathf.Abs(noiseMap[i, j] - origin) > tolerance)
//     //                 return;
                

//     //             states[i, j] = stateIndex;
//     //             statexys[stateIndex] = new Vector2(i, j);
//     //             colourMap[j * width + i] = colors[stateIndex - 1];

                

//     //             Fill(i + 1, j);
//     //             Fill(i - 1, j);
//     //             Fill(i, j + 1);
//     //             Fill(i, j - 1);
//     //         }
//     //     }

//     //     Fill(x, y);
//     // }




//         float origin = noiseMap[x, y];

//         // queue for BFS expansion
//         Queue<Vector2Int> q = new Queue<Vector2Int>();
//         q.Enqueue(new Vector2Int(x, y));
//         int pix=0;
//         while (q.Count > 0)
//         {
//             Vector2Int p = q.Dequeue();
//             int i = p.x;
//             int j = p.y;

//             if(pix++>30)
//                 return;

//             // bounds check
//             if (i < 0 || i >= width || j < 0 || j >= height)
//                 continue;

//             // already filled
//             if (states[i, j] != 0)
//                 continue;

//             // noise mismatch
//             if (Mathf.Abs(noiseMap[i, j] - origin) > tolerance)
//                 continue;

//             // assign state
//             states[i, j] = stateIndex;

//             // store last visited pixel (your original behavior)
//             statexys[stateIndex] = new Vector2(i, j);

//             // // assign color
//             // colourMap[j * width + i] = colors[stateIndex - 1];

//             if (i >= 0 && i < width && j >= 0 && j < height && (j * width + i) >= 0 && (j * width + i) < colourMap.Length)
//             colourMap[j * width + i] = colors[stateIndex - 1];


//             // enqueue neighbors
//             if(i+1<width)
//             q.Enqueue(new Vector2Int(i + 1, j));
//             if(i-1>=0)
//             q.Enqueue(new Vector2Int(i - 1, j));
//             if(j+1<height)
//             q.Enqueue(new Vector2Int(i, j + 1));
//             if(j-1>=0) 
//             q.Enqueue(new Vector2Int(i, j - 1));
//         }
//     }



// 	public Renderer textureRender;

// 	public void DrawNoiseMap(float[,] noiseMap) {
//         // Debug.Log("Drawing Noise Map");
// 		int width = noiseMap.GetLength (0);
// 		int height = noiseMap.GetLength (1);

//         int [,] states=new int [width,height];

//         for(int y=0;y<height;y++)
//             {
//                 for(int x=0;x<width;x++)
//                 {
//                     states[x,y]=0;
//                 }
//             }


        
//         Vector2[] statexys=new Vector2[21];
//         Color[] colors={Color.blue, Color.yellow, Color.gray, Color.green, Color.cyan, Color.magenta, Color.red, Color.black, Color.white,
//                         new Color(0.5f,0.2f,0.7f), new Color(0.1f,0.8f,0.3f), new Color(0.9f,0.6f,0.1f),
//                         new Color(0.4f,0.4f,0.4f), new Color(0.2f,0.5f,0.9f), new Color(0.7f,0.3f,0.2f),
//                         new Color(0.3f,0.7f,0.4f), new Color(0.8f,0.1f,0.5f), new Color(0.6f,0.9f,0.2f),
//                         new Color(0.2f,0.2f,0.8f), new Color(0.9f,0.4f,0.7f), new Color(0.5f,0.9f,0.3f)};

// 		Texture2D texture = new Texture2D (width, height);

// 		// Color[] colourMap_draft = new Color[width * height];
//         Color[] colourMap = new Color[width * height];
		
//         int stateIndex=1;
//         {
//             for(int y=0;y<height;y++)
//             {
//                 for(int x=0;x<width;x++)
//                 {
//                     if(states[x,y]==0)
//                     {
//                         float tolerance=0.05f;
//                         FloodFill(noiseMap , states, x, y, stateIndex, width, height, colourMap, colors,  statexys, tolerance);
//                         stateIndex++;
//                     }
//                 }
//             }
//         }
        

//         // for (int y = 0; y < height; y++) {
// 		// 	for (int x = 0; x < width; x++) {

//         //         for(int i=1;i<=20;i++)
//         //         {
//         //             if(noiseMap[x,y]<=i*0.05f)
//         //             {
//         //                 Color randomColor = new Color(Random.value, Random.value, Random.value, 1f);
//         //                 colourMap_draft[y * width + x]=colors[i-1];
//         //                 break;
//         //             }
//         //         }

//         //         // if(noiseMap[x,y]<=0.05f)
//         //         // {
//         //         //     colourMap[y * width + x] = Color.blue;
//         //         // }
//         //         // else if(noiseMap[x,y]<=0.3f)
//         //         // {
//         //         //     colourMap[y * width + x] = new Color(0.76f,0.7f,0.5f); //sand color
//         //         // }    
//         //         // else if(noiseMap[x,y]<=0.6f)
//         //         // {
//         //         //     colourMap[y * width + x] = Color.gray;
//         //         // }
//         //         // else
//         //         // {   
//         //         //     colourMap[y * width + x] = Color.green;
//         //         // }


// 		// 		//colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, noiseMap [x, y]);
// 		// 	}
// 		// }



// 		texture.SetPixels (colourMap);
// 		texture.Apply ();

// 		textureRender.sharedMaterial.mainTexture = texture;
// 		textureRender.transform.localScale = new Vector3 (width, 1, height);

//         // 

//         // int pixno=0;
//         // while (pixno<25)
//         // {

//         // }
// 	}
	
// }