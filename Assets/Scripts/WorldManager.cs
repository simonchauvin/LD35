using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class WorldManager : MonoBehaviour
{
    private static WorldManager _instance;

    public string fileName;
    public GameObject[] layerPrefabs;
    public Material[] layerMaterials;
    public float panelWidth;

    private Transform gameplayFolder;
    private Camera mainCam;
    private World world;
    private GameObject[] layers;
    private float panelHeight;


    public static WorldManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("WorldManager").GetComponent<WorldManager>();
            }
            return _instance;
        }
    }

    void Awake ()
    {
        gameplayFolder = GameObject.Find("Gameplay").transform;
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        
        world = new World(fileName);
        layers = new GameObject[4];
        Vector3 bottomLeft = mainCam.ScreenToWorldPoint(Vector3.zero);
        Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 300));
        float distHor = (topRight.x - bottomLeft.x);
        panelHeight = (topRight.y - bottomLeft.y) / 4;
        mainCam.transform.position = new Vector3(distHor / 2, 0, -20f);

        int numberOfPanels = Mathf.CeilToInt(distHor / panelWidth);

        loadLayer(3, numberOfPanels);
        loadLayer(2, numberOfPanels);
        loadLayer(1, numberOfPanels);
        loadLayer(0, numberOfPanels);
    }

    private void loadLayer (int layerIndex, int numberOfPanels)
    {
        Vector3 pos = Vector3.zero;
        if (layerIndex == 0)
        {
            pos = new Vector3(0f, 0f, 0f);
        }
        else if (layerIndex == 1)
        {
            pos = new Vector3(0f, 0f, 0.1f);
        }
        else if (layerIndex == 2)
        {
            pos = new Vector3(0f, 0f, 0.2f);
        }
        else if (layerIndex == 3)
        {
            pos = new Vector3(0f, 0f, 0.3f);
        }
        layers[layerIndex] = Instantiate(layerPrefabs[layerIndex], pos, Quaternion.identity) as GameObject;
        layers[layerIndex].transform.parent = gameplayFolder;
        layers[layerIndex].GetComponent<MeshRenderer>().material = layerMaterials[world.getIndex() + layerIndex];
        resetLayerMesh(numberOfPanels, layerIndex);
    }

    private void resetLayerMesh (int numberOfPanels, int layerIndex)
    {
        Mesh mesh = layers[layerIndex].GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        Vector3[] vertices = new Vector3[numberOfPanels * 4];
        int[] triangles = new int[numberOfPanels * 2 * 3];
        float xPos = 0;
        float yPos = (-1f - (panelHeight * (4 / 2))) + (panelHeight * layerIndex);
        for (int i = 0; i < vertices.Length; i += 4)
        {
            vertices[i] = new Vector3(xPos, yPos, 0);
            vertices[i + 1] = new Vector3(xPos + panelWidth, yPos, 0);
            vertices[i + 2] = new Vector3(xPos + panelWidth, yPos + panelHeight, 0);
            vertices[i + 3] = new Vector3(xPos, yPos + panelHeight, 0);

            xPos += panelWidth;
        }

        int tI = 0;
        for (int i = 0; i < vertices.Length; i += 4)
        {
            triangles[tI] = i;
            triangles[tI + 1] = i + 2;
            triangles[tI + 2] = i + 1;
            triangles[tI + 3] = i;
            triangles[tI + 4] = i + 3;
            triangles[tI + 5] = i + 2;
            tI +=  6;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
	
	void Update ()
    {
        if (File.Exists(world.getFilePath()))
        {
            updateWorld(world.retrieveData());
        }
    }

    private void updateWorld (Data[] data)
    {
        int index = 0;
        foreach (GameObject layer in layers)
        {
            Mesh mesh = layer.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            float yPos = (-1f - (panelHeight * (4 / 2))) + (panelHeight * index) + panelHeight;
            for (int i = 0; i < vertices.Length; i += 4)
            {
                vertices[i + 2] = new Vector3(mesh.vertices[i + 2].x, yPos + formatHeight(data[index].getHeight(i + 2)), mesh.vertices[i + 2].z);
                vertices[i + 3] = new Vector3(mesh.vertices[i + 3].x, yPos + formatHeight(data[index].getHeight(i + 3)), mesh.vertices[i + 3].z);
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            index++;
        }
    }

    private float formatHeight(float height)
    {
        height = ((height * 5) / 9) - 2.5f;
        if (height > panelHeight * 0.8f)
        {
            height = panelHeight * 0.8f;
        }
        else if (height < -panelHeight * 0.8f)
        {
            height = -panelHeight * 0.8f;
        }
        return height;
    }

    public void deleteWorld ()
    {
        world.delete();
    }
}
