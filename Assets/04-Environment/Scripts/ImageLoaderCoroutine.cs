using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ImageLoaderCoroutine : MonoBehaviour
{
    private Texture2D imageTexture;
    private Color32[,] imageArray;

    private float[,] redArray;
    private float[,] greenArray;
    private float[,] blueArray;
    private float[,] deformationArray;

    public GameObject prefab;
    public TerrainDeformationMaster terrainDeformationMaster;
    public Terrain myTerrain;
    public AnimationCurve scaleCurve;

    public TerrainLayer[] layers;

    public float delay = 5f;
    public float offDelay = 0.1f;
    private int iteration = 20;

    // Remapping
    public float newMaxValueRed = 0.3f;
    public float newMaxValueBlue = 0.3f;
    public float newMaxValueDeformation = 0.3f;

    public bool visiblePlants;

    public GameObject[,] plants;

    private void Start()
    {
        plants = new GameObject[257, 257];

        StartCoroutine(LoadImage());
    }

    // Stop the coroutine when the script is disabled
    private void OnDisable()
    {
        StopCoroutine(LoadImage());
    }

    private IEnumerator LoadImage()
    {
        while (true)
        {
            // Check if the iteration is a multiple of 20
            if (iteration % 20 == 0)
            {
                // Construct the file name
                string fileName = iteration + "-output.png";
                Debug.Log("Opening file: " + fileName);

                byte[] imageData = File.ReadAllBytes("Assets/05-Communication/Python/frames/CGI/SimulatorData-Visual-Forest/RGB/" + fileName);
                imageTexture = new Texture2D(2, 2);
                imageTexture.LoadImage(imageData);

                // Convert Texture2D to 2D Color32 array
                int width = imageTexture.width;
                int height = imageTexture.height;
                Color[] colors = imageTexture.GetPixels();

                Debug.Log("Width: " + width + " Height: " + height);

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
                            if (plants[x, y] == null)
                            {
                                plants[x, y] = Instantiate(prefab, terrainDeformationMaster.Grid2World(y, terrainDeformationMaster.Get(y, x), x), Quaternion.Euler(-90, 0, Random.Range(0f, 360f)), this.transform);
                            }

                            GameObject plant = plants[x, y];
                            float scaleValue = scaleCurve.Evaluate(greenArray[x, y]);
                            plant.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f) * scaleValue;

                            if (visiblePlants)
                                plant.GetComponent<MeshRenderer>().enabled = true;
                            else
                                plant.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                }

                ChangeTerrainTextureDeformation(terrainDeformationMaster, myTerrain, layers, deformationArray, redArray, blueArray);
            }

            // Increment the iteration number
            iteration = iteration + 20;
            Debug.Log("Summing iteration: " + iteration);

            // Wait for the specified delay before continuing
            yield return new WaitForSeconds(delay);
        }

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

    void ChangeTerrainTextureDeformation(TerrainDeformationMaster terrainDeformationMaster, Terrain myTerrain, TerrainLayer[] layers, float[,] deformationValues, float[,] compressionValues, float[,] accumulationValues)
    {
        TerrainData terrainData = myTerrain.terrainData;

        // Get the current heightmap from the TerrainData
        float[,] heights = terrainDeformationMaster.GetHeightmap();
        float[,] heightsConstant = terrainDeformationMaster.GetConstantHeightmap();

        //Debug.Log("heights[128,128] " + heights[128, 128] * terrainData.heightmapScale.y);

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        float[,,] map = new float[width, height, layers.Length];

        // Loop through the heightmap
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                // Normalize the coordinates to match the size of the compressionValues array
                float u = (float)x / (terrainData.heightmapResolution - 1);
                float v = (float)y / (terrainData.heightmapResolution - 1);
                int valueX = Mathf.RoundToInt(u * (compressionValues.GetLength(0) - 1));
                int valueY = Mathf.RoundToInt(v * (compressionValues.GetLength(1) - 1));

                // Get the compression value and use it to modify the heightmap
                float compressionValue = compressionValues[valueX, valueY];
                float heightModification = compressionValue * -0.05f;
                heights[x, y] = (heightsConstant[x, y] * terrainData.heightmapScale.y + heightModification) / terrainData.heightmapScale.y; // Add the compression value to the original height
            }
        }

        // Apply the modified heightmap to the terrain
        terrainData.SetHeights(0, 0, heights);

        // Texture
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);
                int valueX = Mathf.RoundToInt(u * (deformationValues.GetLength(0) - 1));
                int valueY = Mathf.RoundToInt(v * (deformationValues.GetLength(1) - 1));
                float value = deformationValues[valueX, valueY];

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
