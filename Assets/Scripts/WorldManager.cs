using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class WorldManager : MonoBehaviour
{
    private static WorldManager _instance;

    public GameObject layer1Prefab;
    public GameObject layer2Prefab;
    public GameObject layer3Prefab;
    public GameObject layer4Prefab;
    public float panelWidth;

    private World world;
    private int currentWorldIndex;
    private GameObject[] layers;
    private Transform gameplayFolder;
    private Camera mainCam;
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
        world = new World();
        gameplayFolder = GameObject.Find("Gameplay").transform;
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        layers = new GameObject[4];
        Vector3 bottomLeft = mainCam.ScreenToWorldPoint(Vector3.zero);
        Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        float distHor = (topRight.x - bottomLeft.x);
        panelHeight = (topRight.y - bottomLeft.y) / 4;
        mainCam.transform.position = new Vector3(distHor / 2, 0, -20f);

        int numberOfPanels = Mathf.CeilToInt(distHor / panelWidth);

        layers[3] = Instantiate(layer4Prefab, new Vector3(0f, 0f, 0.3f), Quaternion.identity) as GameObject;
        layers[3].transform.parent = gameplayFolder;
        resetLayerMesh(numberOfPanels, 3);
        layers[2] = Instantiate(layer3Prefab, new Vector3(0f, 0f, 0.2f), Quaternion.identity) as GameObject;
        layers[2].transform.parent = gameplayFolder;
        resetLayerMesh(numberOfPanels, 2);
        layers[1] = Instantiate(layer2Prefab, new Vector3(0f, 0f, 0.1f), Quaternion.identity) as GameObject;
        layers[1].transform.parent = gameplayFolder;
        resetLayerMesh(numberOfPanels, 1);
        layers[0] = Instantiate(layer1Prefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
        layers[0].transform.parent = gameplayFolder;
        resetLayerMesh(numberOfPanels, 0);
    }

    private void resetLayerMesh (int numberOfPanels, int layerIndex)
    {
        Mesh mesh = layers[layerIndex].GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        Vector3[] vertices = new Vector3[numberOfPanels * 4];
        int[] triangles = new int[numberOfPanels * 2 * 3];
        float xPos = 0;
        float yPos = (0 - (panelHeight * (4 / 2))) + (panelHeight * layerIndex);
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
        if (File.Exists(world.getFileName()))
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
            float yPos = (0 - (panelHeight * (4 / 2))) + (panelHeight * index) + panelHeight;
            for (int i = 0; i < vertices.Length; i += 4)
            {
                vertices[i + 2] = new Vector3(mesh.vertices[i].x + panelWidth, yPos + formatHeight(data[index].getHeight(i)), mesh.vertices[i].z);
                vertices[i + 3] = new Vector3(mesh.vertices[i].x, yPos + formatHeight(data[index].getHeight(i)), mesh.vertices[i].z);
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            index++;
        }
    }

    private float formatHeight(float height)
    {
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
