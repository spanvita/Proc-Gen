using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, StateMap }
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public int octaves;

    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    [Header("Terrain (for Noise/Color)")]
    public TerrainType[] regions;

    [Header("States")]
    public int stateCount = 30;
    [Range(0f, 1f)] public float seaLevel = 0.4f;  
    public Color waterColor = new Color(0.05f, 0.1f, 0.3f);

    // Exposed so click handler can read them
    [HideInInspector] public int[,] stateIds;
    [HideInInspector] public Texture2D latestTexture;
    [HideInInspector] public List<Vector2Int> stateCapitals;

    [Header("Borders")]
    public float borderDistortion = 24f;   
    public float borderNoiseFrequency = 0.15f; 
    public Color stateBorderColor = Color.white;   
    public Color coastBorderColor = Color.white;   
    
    [Header("State Colors")]
    public bool useStrategicMapPalette = true;
    public Color[] strategicPalette; 
    
    [Header("Oceans")]
    public bool generateOceans = true;
    [Range(0, 10)] public int oceanCount = 2;           
    [Range(2, 5)] public int minStatesPerOcean = 2;     
    [Range(2, 6)] public int maxStatesPerOcean = 3;   
    
    void Start()
    {
        GenerateMap();
    }

    void update()
    {
        // displayStateIds();
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.generateNoiseMap(
            mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset
        );

        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            return;
        }

        if (drawMode == DrawMode.ColorMap)
        {
            Color[] colorMap = GenerateTerrainColorMap(noiseMap);
            Texture2D tex = TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight);
            latestTexture = tex;
            display.DrawTexture(tex);
            return;
        }

        if (drawMode == DrawMode.StateMap)
        {
            GenerateStateMap(noiseMap, display);
        }
    }

    Color[] GenerateTerrainColorMap(float[,] noiseMap)
    {
        Color[] colorMap = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        return colorMap;
    }

    void GenerateStateMap(float[,] noiseMap, MapDisplay display)
    {
   
        int totalTiles = mapWidth * mapHeight;
        int actualStateCount = Mathf.Min(stateCount, totalTiles);

        // Random seed tiles over the whole map
        System.Random rnd = new System.Random(seed + 12345);
        List<Vector2Int> seeds = new List<Vector2Int>();
        HashSet<Vector2Int> used = new HashSet<Vector2Int>();

        for (int i = 0; i < actualStateCount; i++)
        {
            Vector2Int pos;
            do
            {
                int x = rnd.Next(0, mapWidth);
                int y = rnd.Next(0, mapHeight);
                pos = new Vector2Int(x, y);
            }
            while (used.Contains(pos));

            used.Add(pos);
            seeds.Add(pos);
        }

        // Random colors per state
        Color[] stateColors = BuildStateColors(actualStateCount);

        stateIds = new int[mapWidth, mapHeight];
        Color[] colorMap = new Color[mapWidth * mapHeight];

        //Voronoi assignment with warped borders – EVERY tile is assigned to some state 
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int idx = y * mapWidth + x;

                int bestState = 0;
                float bestDistSq = float.MaxValue;
                
                float nx = (float)x / mapWidth * borderNoiseFrequency;
                float ny = (float)y / mapHeight * borderNoiseFrequency;

                // PerlinNoise -> [0,1], so convert to [-1,1]
                float noiseX = Mathf.PerlinNoise(nx, ny) * 2f - 1f;
                float noiseY = Mathf.PerlinNoise(nx + 1000f, ny + 1000f) * 2f - 1f;

                float warpX = noiseX * borderDistortion;
                float warpY = noiseY * borderDistortion;

                float distortedX = x + warpX;
                float distortedY = y + warpY;


                for (int s = 0; s < actualStateCount; s++)
                {
                    Vector2Int seedPos = seeds[s];
                    float dx = distortedX - seedPos.x;
                    float dy = distortedY - seedPos.y;
                    float distSq = dx * dx + dy * dy;

                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        bestState = s;
                    }
                }

                stateIds[x, y] = bestState;
                colorMap[idx] = stateColors[bestState];
            }
        }

        // Fix tiny disconnected islands per state
        EnsureStateConnectivity(stateIds, mapWidth, mapHeight, actualStateCount);
        

        



        // Carve “oceans” by turning groups of 2–3 states into water (stateId = -1)
        if (generateOceans && oceanCount > 0)
        {
            CarveOceans(stateIds, mapWidth, mapHeight, actualStateCount);
        }

        // Compute capitals (centroids of remaining land states)
        Vector2Int[] capitals = new Vector2Int[actualStateCount];
        bool[] hasAnyTile = new bool[actualStateCount];

        float[] sumX = new float[actualStateCount];
        float[] sumY = new float[actualStateCount];
        int[] counts = new int[actualStateCount];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int sId = stateIds[x, y];
                if (sId < 0 || sId >= actualStateCount) continue;

                sumX[sId] += x;
                sumY[sId] += y;
                counts[sId]++;
                hasAnyTile[sId] = true;
            }
        }

        List<Vector2Int> capitalsList = new List<Vector2Int>();
        for (int s = 0; s < actualStateCount; s++)
        {
            if (!hasAnyTile[s]) continue; // state was turned into ocean

            float cx = sumX[s] / counts[s];
            float cy = sumY[s] / counts[s];

            float bestDistSq = float.MaxValue;
            Vector2Int best = new Vector2Int(0, 0);

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (stateIds[x, y] != s) continue;

                    float dx = x - cx;
                    float dy = y - cy;
                    float d2 = dx * dx + dy * dy;
                    if (d2 < bestDistSq)
                    {
                        bestDistSq = d2;
                        best = new Vector2Int(x, y);
                    }
                }
            }

            capitals[s] = best;
            capitalsList.Add(best);
        }
        stateCapitals = capitalsList;

        List<List<Vector2Int>> allStates;
        allStates = new List<List<Vector2Int>>();

        for (int i = 0; i < stateCount; i++)
            allStates.Add(new List<Vector2Int>());

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int id = stateIds[x, y];
                if (id >= 0 && id < stateCount)
                {
                    allStates[id].Add(new Vector2Int(x, y));
                }
            }
        }

        

        //Rebuild colorMap from final stateIds (now including oceans)
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int idx = y * mapWidth + x;
                int sId = stateIds[x, y];

                if (sId == -1)
                    colorMap[idx] = waterColor;
                else
                    colorMap[idx] = stateColors[sId];
            }
        }

        int[,] provincetype = new int[mapWidth, mapHeight]; //0=normal,1=dessert,2=mountain (safezone)
        createProvinces(allStates);
        

        void createProvinces(List<List<Vector2Int>>allStates)
        {
            int provinceId = 0;
            foreach (var state in allStates)
            {
                foreach (var tile in state)
                {
                    if(noiseMap[tile.x,tile.y]>0.8f)
                    {
                        provincetype[tile.x,tile.y]=2; //mountain
                        colorMap[tile.y * mapWidth + tile.x] = Color.gray;
                    }
                    else if (noiseMap[tile.x,tile.y]<0.4f)
                    {
                        provincetype[tile.x,tile.y]=1; //dessert
                        colorMap[tile.y * mapWidth + tile.x] = new Color32(237, 201, 175, 255);
                    }
                    else
                    {
                        provincetype[tile.x,tile.y]=0; //normal or grassland
                        colorMap[tile.y * mapWidth + tile.x] = stateColors[stateIds[tile.x,tile.y]];
                    }
                }
                provinceId++;
            }
        }

        // Draw capitals as black crosses
        Color capitalColor = Color.black;

        void SetPixelSafe(int px, int py)
        {
            if (px < 0 || px >= mapWidth || py < 0 || py >= mapHeight) return;
            int i = py * mapWidth + px;
            colorMap[i] = capitalColor;
        }

        foreach (var cap in stateCapitals)
        {
            int cx = cap.x;
            int cy = cap.y;

            SetPixelSafe(cx, cy);
            SetPixelSafe(cx + 1, cy);
            SetPixelSafe(cx - 1, cy);
            SetPixelSafe(cx, cy + 1);
            SetPixelSafe(cx, cy - 1);
        }

        DrawStateBorders(colorMap);
        
        Texture2D tex2D = TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight);
        tex2D.filterMode = FilterMode.Point;

        latestTexture = tex2D;
        display.DrawTexture(tex2D);

        Debug.Log("Generated " + actualStateCount + " states (some may be oceans).");
    }
    


    Color[] BuildStateColors(int stateCount)
    {
        Color[] result = new Color[stateCount];
        
        Color[] defaultPalette = new Color[]
        {
            new Color32(160, 184, 206, 255),
            new Color32(173, 191, 150, 255), 
            new Color32(210, 196, 170, 255), 
            new Color32(203, 178, 191, 255), 
            new Color32(180, 184, 194, 255),
            new Color32(176, 194, 178, 255), 
            new Color32(201, 187, 170, 255), 
            new Color32(193, 197, 176, 255)  
        };

        Color[] palette = defaultPalette;

        // Own palette in the inspector
        if (strategicPalette != null && strategicPalette.Length > 0)
        {
            palette = strategicPalette;
        }

        System.Random rnd = new System.Random(seed + 54321);

        for (int i = 0; i < stateCount; i++)
        {
            Color baseColor = palette[i % palette.Length];
            
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            float dv = (float)(rnd.NextDouble() * 0.10 - 0.05);  
            float ds = (float)(rnd.NextDouble() * 0.06 - 0.03);   
            v = Mathf.Clamp01(v + dv);
            s = Mathf.Clamp01(s + ds);
            result[i] = Color.HSVToRGB(h, s, v);
        }

        return result;
    }
    
    void DrawStateBorders(Color[] colorMap)
    {
        int w = mapWidth;
        int h = mapHeight;

       
        bool[,] stateBorderMask = new bool[w, h];
        bool[,] coastBorderMask = new bool[w, h];
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int s = stateIds[x, y];
                
                void EdgeTo(int nx, int ny)
                {
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) return;

                    int ns = stateIds[nx, ny];
                    if (ns == s) return;          
                    if (ns == -1 && s == -1) return; 
                    
                    if (s == -1 || ns == -1)
                    {
                        coastBorderMask[x, y] = true;
                        coastBorderMask[nx, ny] = true;
                    }
                    else
                    {
                        stateBorderMask[x, y] = true;
                        stateBorderMask[nx, ny] = true;
                    }
                }
                
                EdgeTo(x + 1, y);
                EdgeTo(x - 1, y);
                EdgeTo(x, y + 1);
                EdgeTo(x, y - 1);
            }
        }
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int idx = y * w + x;

                if (coastBorderMask[x, y])
                {
                    colorMap[idx] = coastBorderColor;
                }
                else if (stateBorderMask[x, y])
                {
                    colorMap[idx] = stateBorderColor;
                }
            }
        }
    }

    void EnsureStateConnectivity(int[,] stateIds, int width, int height, int stateCount)
    {
        for (int s = 0; s < stateCount; s++)
        {
            bool[,] visited = new bool[width, height];
            List<List<Vector2Int>> components = new List<List<Vector2Int>>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[x, y]) continue;
                    if (stateIds[x, y] != s) continue;

                    List<Vector2Int> comp = new List<Vector2Int>();
                    Queue<Vector2Int> q = new Queue<Vector2Int>();

                    visited[x, y] = true;
                    q.Enqueue(new Vector2Int(x, y));

                    while (q.Count > 0)
                    {
                        Vector2Int p = q.Dequeue();
                        comp.Add(p);

                        int px = p.x;
                        int py = p.y;

                        TryVisit(px + 1, py,     s, stateIds, visited, width, height, q);
                        TryVisit(px - 1, py,     s, stateIds, visited, width, height, q);
                        TryVisit(px,     py + 1, s, stateIds, visited, width, height, q);
                        TryVisit(px,     py - 1, s, stateIds, visited, width, height, q);
                    }

                    components.Add(comp);
                }
            }

            if (components.Count <= 1)
                continue;

            components.Sort((a, b) => b.Count.CompareTo(a.Count));

            for (int i = 1; i < components.Count; i++)
            {
                foreach (Vector2Int p in components[i])
                {
                    int newState = FindBestNeighbourState(p.x, p.y, s, stateIds, width, height);
                    if (newState >= 0)
                    {
                        stateIds[p.x, p.y] = newState;
                    }
                    else
                    {
                        stateIds[p.x, p.y] = s;
                    }
                }
            }
        }
    }

    void TryVisit(int x, int y, int s,
                  int[,] stateIds, bool[,] visited,
                  int width, int height, Queue<Vector2Int> q)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (visited[x, y]) return;
        if (stateIds[x, y] != s) return;

        visited[x, y] = true;
        q.Enqueue(new Vector2Int(x, y));
    }

    int FindBestNeighbourState(int x, int y, int oldState,
                               int[,] stateIds, int width, int height)
    {
        Dictionary<int, int> counts = new Dictionary<int, int>();

        void Add(int nx, int ny)
        {
            if (nx < 0 || nx >= width || ny < 0 || ny >= height) return;
            int s = stateIds[nx, ny];
            if (s < 0 || s == oldState) return;
            if (!counts.ContainsKey(s)) counts[s] = 0;
            counts[s]++;
        }

        Add(x + 1, y);
        Add(x - 1, y);
        Add(x, y + 1);
        Add(x, y - 1);

        if (counts.Count == 0) return -1;

        int bestState = -1;
        int bestCount = -1;
        foreach (var kvp in counts)
        {
            if (kvp.Value > bestCount)
            {
                bestCount = kvp.Value;
                bestState = kvp.Key;
            }
        }

        return bestState;
    }
    
    void CarveOceans(int[,] stateIds, int width, int height, int stateCount)
    {
        int maxPossibleOceans = Mathf.Max(0, stateCount / minStatesPerOcean);
        int targetOceans = Mathf.Clamp(oceanCount, 0, maxPossibleOceans);
        if (targetOceans == 0) return;

        HashSet<int>[] neighbours = new HashSet<int>[stateCount];
        bool[] hasTiles = new bool[stateCount];
        for (int i = 0; i < stateCount; i++)
            neighbours[i] = new HashSet<int>();
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int s = stateIds[x, y];
                if (s < 0 || s >= stateCount) continue;

                hasTiles[s] = true;

                if (x + 1 < width)
                {
                    int sRight = stateIds[x + 1, y];
                    if (sRight >= 0 && sRight < stateCount && sRight != s)
                    {
                        neighbours[s].Add(sRight);
                        neighbours[sRight].Add(s);
                    }
                }

                if (y + 1 < height)
                {
                    int sUp = stateIds[x, y + 1];
                    if (sUp >= 0 && sUp < stateCount && sUp != s)
                    {
                        neighbours[s].Add(sUp);
                        neighbours[sUp].Add(s);
                    }
                }
            }
        }

        List<int> candidates = new List<int>();
        for (int s = 0; s < stateCount; s++)
        {
            if (hasTiles[s]) candidates.Add(s);
        }
        if (candidates.Count == 0) return;

        System.Random rnd = new System.Random(seed + 9999);

        for (int i = 0; i < candidates.Count; i++)
        {
            int j = rnd.Next(i, candidates.Count);
            int tmp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = tmp;
            candidates[j] = tmp;
        }

        HashSet<int> oceanStates = new HashSet<int>();
        int oceansCreated = 0;
        int index = 0;

        while (oceansCreated < targetOceans && index < candidates.Count)
        {
            int startState = candidates[index++];
            if (oceanStates.Contains(startState)) continue;
            if (!hasTiles[startState]) continue;

            List<int> group = new List<int>();
            group.Add(startState);
            oceanStates.Add(startState);

            Queue<int> frontier = new Queue<int>();
            frontier.Enqueue(startState);

            int desiredSize = rnd.Next(minStatesPerOcean, maxStatesPerOcean + 1);

            while (group.Count < desiredSize && frontier.Count > 0)
            {
                int cur = frontier.Dequeue();

                List<int> neighList = new List<int>(neighbours[cur]);
                for (int i = 0; i < neighList.Count; i++)
                {
                    int j = rnd.Next(i, neighList.Count);
                    int tmp = neighList[i];
                    neighList[i] = neighList[j];
                    neighList[j] = tmp;
                }

                foreach (int nb in neighList)
                {
                    if (group.Count >= desiredSize) break;
                    if (oceanStates.Contains(nb)) continue;
                    if (!hasTiles[nb]) continue;

                    oceanStates.Add(nb);
                    group.Add(nb);
                    frontier.Enqueue(nb);
                }
            }

            if (group.Count >= minStatesPerOcean)
            {
                oceansCreated++;
            }
            else
            {
                foreach (int s in group)
                {
                    oceanStates.Remove(s);
                }
            }
        }

        if (oceanStates.Count == 0) return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int s = stateIds[x, y];
                if (oceanStates.Contains(s))
                {
                    stateIds[x, y] = -1; 
                }
            }
        }
    }
    

    
    void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;

        if (minStatesPerOcean > maxStatesPerOcean)
            minStatesPerOcean = maxStatesPerOcean;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public Color color;
    public float height;
}