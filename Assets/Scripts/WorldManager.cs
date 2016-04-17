using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class WorldManager : MonoBehaviour
{
    private static WorldManager _instance;

    public string fileName;
    public string exeFileName;
    public GameObject[] layerPrefabs;
    public Material[] layerMaterials;
    public Color[] skiesColor;

    private Transform gameplayFolder;
    private Camera mainCam;
    private World world;
    private GameObject[] layers;
    private float planeWidth;
    private float planeHeight;
    private string exeFilePath;
    private bool nextSeasonUnlocked;


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
        
        world = new World();
        mainCam.backgroundColor = skiesColor[world.getIndex()];
        Vector3 bottomLeft = mainCam.ScreenToWorldPoint(Vector3.zero);
        Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 300));
        planeWidth = (topRight.x - bottomLeft.x) / 16;
        planeHeight = (topRight.y - bottomLeft.y) / 4;
        mainCam.transform.position = new Vector3((topRight.x - bottomLeft.x) / 2, 0, -20f);
        exeFilePath = Application.dataPath + "/../" + exeFileName + ".exe";
        nextSeasonUnlocked = false;

        int planesNumber = 16;
        layers = new GameObject[4];
        loadLayer(3, planesNumber);
        loadLayer(2, planesNumber);
        loadLayer(1, planesNumber);
        loadLayer(0, planesNumber);
    }

    private void loadLayer (int layerIndex, int planesNumber)
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
        layers[layerIndex].GetComponent<MeshRenderer>().material = layerMaterials[(world.getIndex() * 4) + layerIndex];
        resetLayerMesh(planesNumber, layerIndex);
    }

    private void resetLayerMesh (int numberOfPanels, int layerIndex)
    {
        Mesh mesh = layers[layerIndex].GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        Vector3[] vertices = new Vector3[numberOfPanels * 4];
        int[] triangles = new int[numberOfPanels * 6];
        float xPos = 0;
        float yPos = (-1f - (planeHeight * 2)) + (planeHeight * layerIndex);
        for (int i = 0; i < vertices.Length; i += 4)
        {
            vertices[i] = new Vector3(xPos, yPos, 0);
            vertices[i + 1] = new Vector3(xPos + planeWidth, yPos, 0);
            vertices[i + 2] = new Vector3(xPos + planeWidth, yPos + planeHeight, 0);
            vertices[i + 3] = new Vector3(xPos, yPos + planeHeight, 0);

            xPos += planeWidth;
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
            Data[] data = world.retrieveData(world.getFilePath());
            updateWorld(data);
            
            if (!isThisTheLastWorld() && canUnlockNextSeason(data))
            {
                if (File.Exists(exeFilePath) && !isThisWorldRunningAlready(world.getNextWorldIndex()) && !nextSeasonUnlocked)
                {
                    nextSeasonUnlocked = true;
                    System.Diagnostics.Process.Start(exeFilePath);
                }
            }
            else if (nextSeasonUnlocked)
            {
                if (world.getNextWorldIndex() > 0 && File.Exists(world.getNextWorldPath()))
                {
                    nextSeasonUnlocked = false;
                    world.delete(world.getNextWorldIndex());
                }
            }
            else if (isThisTheLastWorld())
            {
                // TODO end the game
                // fade all to black ?
            }
        }
        else
        {
            GameManager.instance.exit();
        }
    }

    private bool isThisWorldRunningAlready (int index)
    {
        if (File.Exists(Application.dataPath + "/../" + fileName + "_" + index + ".txt"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool isThisTheLastWorld ()
    {
        if (world.getNextWorldIndex() > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool canUnlockNextSeason(Data[] data)
    {
        // TODO design
        float total = 0;
        foreach (Data layerData in data)
        {
            for (int i = 0; i < layerData.length; i++)
            {
                total += layerData.getHeight(i);
            }
        }
        if (total > 325)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void updateWorld (Data[] data)
    {
        int index = 0;
        foreach (GameObject layer in layers)
        {
            Mesh mesh = layer.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            float yPos = (-1.2f - (planeHeight * 2)) + (planeHeight * index) + planeHeight;
            for (int i = 0; i < vertices.Length; i += 4)
            {
                vertices[i + 2] = new Vector3(mesh.vertices[i + 2].x, yPos + formatHeight(data[index].getHeight(Mathf.FloorToInt(((i + 2) * 16) / 64))), mesh.vertices[i + 2].z);
                vertices[i + 3] = new Vector3(mesh.vertices[i + 3].x, yPos + formatHeight(data[index].getHeight(Mathf.FloorToInt(((i + 3) * 16) / 64))), mesh.vertices[i + 3].z);
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            index++;
        }
    }

    private float formatHeight(float height)
    {
        float ratio = 1.0f;
        height = ((height * 4) / 9) - planeHeight;
        if (height > planeHeight * ratio)
        {
            height = planeHeight * ratio;
        }
        else if (height < -planeHeight * ratio)
        {
            height = -planeHeight * ratio;
        }
        return height;
    }

    public void deleteWorld ()
    {
        if (world != null)
        {
            int i = world.getIndex();
            while (world.getNextWorldPath(i).Length > 0 && File.Exists(world.getNextWorldPath(i)))
            {
                File.Delete(world.getNextWorldPath(i));
                i++;
            }
            world.delete();
        }
    }
}
