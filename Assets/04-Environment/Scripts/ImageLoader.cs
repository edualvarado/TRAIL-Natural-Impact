using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour
{
    private Texture2D imageTexture;
    private Color32[,] imageArray;

    public float[,] redArray;
    public float[,] greenArray;
    public float[,] blueArray;
    public float[,] deformationArray;

    public GameObject prefab;
    public TerrainDeformationMaster terrain;
    public Terrain myTerrain;
    public AnimationCurve scaleCurve;

    public TerrainLayer[] layers;

    // Define the size of the area of influence
    public int areaSize = 3;

    // Remapping
    public float newMaxValueRed = 0.3f;
    public float newMaxValueBlue = 0.3f;
    public float newMaxValueDeformation = 0.3f;

    void Start()
    {
        // Load image from file into a Texture2D object
        byte[] imageData = File.ReadAllBytes("Assets/Maps/summer/144-output.png");
        imageTexture = new Texture2D(2, 2);
        imageTexture.LoadImage(imageData);

        // Convert Texture2D to 2D Color32 array
        int width = imageTexture.width;
        int height = imageTexture.height;
        Color[] colors = imageTexture.GetPixels();

        redArray = new float[width, height];
        greenArray = new float[width, height];
        blueArray = new float[width, height];

        deformationArray = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixelColor = colors[x + y * width];
                redArray[x, y] = pixelColor.r;
                greenArray[x, y] = pixelColor.g;
                blueArray[x, y] = pixelColor.b;

                deformationArray[x, y] = redArray[x, y] + blueArray[x, y];

                if (greenArray[x, y] > 0)
                {
                    GameObject plant = Instantiate(prefab, terrain.Grid2World(x, 1, y), Quaternion.Euler(-90, 0, Random.Range(0f, 360f)), this.transform);

                    float scaleValue = scaleCurve.Evaluate(greenArray[x, y]);

                    plant.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f) * scaleValue;
                }
            }
        }

        //ChangeTerrainTextureRed(myTerrain, layers, redArray);
        //ChangeTerrainTextureBlue(myTerrain, layers, blueArray);
        ChangeTerrainTextureDeformation(myTerrain, layers, deformationArray);

        //RemoveGrassAtPosition(myTerrain, Vector3.zero, 1);
    }

    void ChangeTerrainTextureRed(Terrain terrain, TerrainLayer[] layers, float[,] values)
    {
        TerrainData terrainData = terrain.terrainData;

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        float[,,] map = new float[width, height, layers.Length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);
                int valueX = Mathf.RoundToInt(u * (values.GetLength(0) - 1));
                int valueY = Mathf.RoundToInt(v * (values.GetLength(1) - 1));
                float value = values[valueX, valueY];

                float valueRemap = value / newMaxValueRed;
                valueRemap = Mathf.Clamp01(valueRemap);

                // Set the opacity of the first texture based on the normalized parameter
                map[x, y, 0] = 1f - valueRemap;
                // Set the opacity of the second texture based on the normalized parameter
                map[x, y, 1] = valueRemap;
            }
        }

        terrainData.terrainLayers = layers;
        terrainData.SetAlphamaps(0, 0, map);
    }

    void ChangeTerrainTextureBlue(Terrain terrain, TerrainLayer[] layers, float[,] values)
    {
        TerrainData terrainData = terrain.terrainData;

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        float[,,] map = new float[width, height, layers.Length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);
                int valueX = Mathf.RoundToInt(u * (values.GetLength(0) - 1));
                int valueY = Mathf.RoundToInt(v * (values.GetLength(1) - 1));
                float value = values[valueX, valueY];

                float valueRemap = value / newMaxValueBlue;
                valueRemap = Mathf.Clamp01(valueRemap);

                // Set the opacity of the first texture based on the normalized parameter
                map[x, y, 0] = 1f - valueRemap;
                // Set the opacity of the second texture based on the normalized parameter
                map[x, y, 1] = valueRemap;
            }
        }

        terrainData.terrainLayers = layers;
        terrainData.SetAlphamaps(0, 0, map);
    }

    void ChangeTerrainTextureDeformation(Terrain terrain, TerrainLayer[] layers, float[,] values)
    {
        TerrainData terrainData = terrain.terrainData;

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        float[,,] map = new float[width, height, layers.Length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);
                int valueX = Mathf.RoundToInt(u * (values.GetLength(0) - 1));
                int valueY = Mathf.RoundToInt(v * (values.GetLength(1) - 1));
                float value = values[valueX, valueY];

                float valueRemap = value / newMaxValueDeformation;
                valueRemap = Mathf.Clamp01(valueRemap);

                // Set the opacity of the first texture based on the normalized parameter
                map[x, y, 0] = 1f - valueRemap;
                // Set the opacity of the second texture based on the normalized parameter
                map[x, y, 1] = valueRemap;
            }
        }

        terrainData.terrainLayers = layers;
        terrainData.SetAlphamaps(0, 0, map);
    }

    void RemoveGrassAtPosition(Terrain terrain, Vector3 position, float radius)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;
        int mapX = (int)(((position.x - terrainPos.x) / terrainData.size.x) * terrainData.detailWidth);
        int mapZ = (int)(((position.z - terrainPos.z) / terrainData.size.z) * terrainData.detailHeight);
        int size = (int)(radius / terrainData.size.x * terrainData.detailWidth);

        int[,] detailMap = terrainData.GetDetailLayer(mapX - size / 2, mapZ - size / 2, size, size, 0);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float distance = Vector2.Distance(new Vector2(i, j), new Vector2(size / 2f, size / 2f));
                if (distance <= size / 2f)
                {
                    detailMap[i, j] = 0;
                }
            }
        }
        
        terrainData.SetDetailLayer(mapX - size / 2, mapZ - size / 2, 0, detailMap);
    }
}
