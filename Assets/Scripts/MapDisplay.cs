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
    Vector2[] statexys,
    float tolerance)
    {

        /////public static void FloodFill(float [,] noiseMap, int[,] states, int x, int y, int stateIndex,  int width, int height, Color[] colourMap, Color[] colors, Vector2[] statexys, float tolerance)
    // {

    //     float origin = noiseMap[x, y];

    //     void Fill(int i, int j) 
    //     {
            
    //         {
    //             if (i < 0 || i >=width || j < 0 || j >= height)
    //                 return;

    //             if (states[i, j] != 0)
    //                 return;

    //             if (Mathf.Abs(noiseMap[i, j] - origin) > tolerance)
    //                 return;
                

    //             states[i, j] = stateIndex;
    //             statexys[stateIndex] = new Vector2(i, j);
    //             colourMap[j * width + i] = colors[stateIndex - 1];

                

    //             Fill(i + 1, j);
    //             Fill(i - 1, j);
    //             Fill(i, j + 1);
    //             Fill(i, j - 1);
    //         }
    //     }

    //     Fill(x, y);
    // }




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

            if(pix++>30)
                return;

            // bounds check
            if (i < 0 || i >= width || j < 0 || j >= height)
                continue;

            // already filled
            if (states[i, j] != 0)
                continue;

            // noise mismatch
            if (Mathf.Abs(noiseMap[i, j] - origin) > tolerance)
                continue;

            // assign state
            states[i, j] = stateIndex;

            // store last visited pixel (your original behavior)
            //statexys[stateIndex] = new Vector2(i, j);

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


        
        Vector2[] statexys=new Vector2[21];
        Color[] colors={Color.blue, Color.yellow, Color.gray, Color.green, Color.cyan, Color.magenta, Color.red, Color.black, Color.white,
                        new Color(0.5f,0.2f,0.7f), new Color(0.1f,0.8f,0.3f), new Color(0.9f,0.6f,0.1f),
                        new Color(0.4f,0.4f,0.4f), new Color(0.2f,0.5f,0.9f), new Color(0.7f,0.3f,0.2f),
                        new Color(0.3f,0.7f,0.4f), new Color(0.8f,0.1f,0.5f), new Color(0.6f,0.9f,0.2f),
                        new Color(0.2f,0.2f,0.8f), new Color(0.9f,0.4f,0.7f), new Color(0.5f,0.9f,0.3f)};

		Texture2D texture = new Texture2D (width, height);

		// Color[] colourMap_draft = new Color[width * height];
        Color[] colourMap = new Color[width * height];
		
        int stateIndex=1;
        while(stateIndex<=20)
        {
            for(int y=0;y<height;y++)
            {
                for(int x=0;x<width;x++)
                {
                    if(states[x,y]==0)
                    {
                        float tolerance=0.05f;
                        FloodFill(noiseMap , states, x, y, stateIndex, width, height, colourMap, colors,  statexys, tolerance);
                        stateIndex++;
                    }
                }
            }
        }
        

        // for (int y = 0; y < height; y++) {
		// 	for (int x = 0; x < width; x++) {

        //         for(int i=1;i<=20;i++)
        //         {
        //             if(noiseMap[x,y]<=i*0.05f)
        //             {
        //                 Color randomColor = new Color(Random.value, Random.value, Random.value, 1f);
        //                 colourMap_draft[y * width + x]=colors[i-1];
        //                 break;
        //             }
        //         }

        //         // if(noiseMap[x,y]<=0.05f)
        //         // {
        //         //     colourMap[y * width + x] = Color.blue;
        //         // }
        //         // else if(noiseMap[x,y]<=0.3f)
        //         // {
        //         //     colourMap[y * width + x] = new Color(0.76f,0.7f,0.5f); //sand color
        //         // }    
        //         // else if(noiseMap[x,y]<=0.6f)
        //         // {
        //         //     colourMap[y * width + x] = Color.gray;
        //         // }
        //         // else
        //         // {   
        //         //     colourMap[y * width + x] = Color.green;
        //         // }


		// 		//colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, noiseMap [x, y]);
		// 	}
		// }



		texture.SetPixels (colourMap);
		texture.Apply ();

		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (width, 1, height);

        // 

        // int pixno=0;
        // while (pixno<25)
        // {

        // }
	}
	
}

// using UnityEngine;
// using System.Collections;

// public class MapDisplay : MonoBehaviour {

// 	public Renderer textureRender;

// 	public void DrawTexture(Texture2D texture) {
// 		textureRender.sharedMaterial.mainTexture = texture;
// 		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height);
// 	}
	
// }