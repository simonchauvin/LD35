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
    private Transform dLight;
    private Vector3 dLightRotStart;
    private Vector3 dLightRotEnd;
    private float lightTimer;
    private float maxLightTimer;
    private bool lightRotPhase;
    private World world;
    private GameObject[] layers;
    private float planeWidth;
    private float planeHeight;
    private string exeFilePath;
    private bool nextSeasonUnlocked;
    private Data[] lastData;


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
        dLight = GameObject.FindObjectOfType<Light>().transform;
        dLightRotStart = dLight.rotation.eulerAngles;
        dLightRotEnd = new Vector3(dLight.rotation.eulerAngles.x - 30f, dLight.rotation.eulerAngles.y, dLight.rotation.eulerAngles.z);
        lightTimer = 0f;
        maxLightTimer = 5f + Random.value;
        lightRotPhase = false;
        lastData = new Data[4];

        world = new World();
        if (!File.Exists(world.getNextWorldPath()))
        {
            AudioManager.instance.playMusic(world.getIndex());
        }
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

    public void setLastData (int index, string line)
    {
        lastData[index] = new Data(line.Split('|'));
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
	
    private int getModifiedLayer(Data[] data, Data[] last)
    {
        for (int i = 0; i < data.Length; i++)
        {
            for (int j = 0; j < data[i].length; j++)
            {
                if (data[i].getHeight(j) != last[i].getHeight(j))
                {
                    return i;
                }
            }
        }
        return -1;
    }

	void Update ()
    {
        if (File.Exists(world.getFilePath()))
        {
            Data[] data = world.retrieveData(world.getFilePath());
            int layerToPlayIndex = getModifiedLayer(data, lastData);
            if (layerToPlayIndex >= 0)
            {
                AudioManager.instance.playLayer(layerToPlayIndex);
            }
            updateWorld(data);
            data.CopyTo(lastData, 0);

            float[] totalPerLayer = computeTotalPerLayer(data);
            int biggest = findBiggestLayer(totalPerLayer);
            if (!isThisTheLastWorld() && canUnlockNextSeason(totalPerLayer, biggest))
            {
                if (File.Exists(exeFilePath) && !isThisWorldRunningAlready(world.getNextWorldIndex()) && !nextSeasonUnlocked)
                {
                    nextSeasonUnlocked = true;
                    writeInterSeasonFile(totalPerLayer, biggest);
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

            if (lightRotPhase)
            {
                dLight.rotation = Quaternion.Euler(Vector3.Lerp(dLightRotStart, dLightRotEnd, lightTimer / maxLightTimer));
            }
            else
            {
                dLight.rotation = Quaternion.Euler(Vector3.Lerp(dLightRotEnd, dLightRotStart, lightTimer / maxLightTimer));
            }
            lightTimer += Time.deltaTime;
            if (lightTimer >= maxLightTimer)
            {
                lightTimer = 0;
                lightRotPhase = !lightRotPhase;
            }
            
            if (File.Exists(world.getNextWorldPath()))
            {
                AudioManager.instance.stopMusic();
            }
            else
            {
                AudioManager.instance.playMusic(world.getIndex());
            }
        }
        else
        {
            GameManager.instance.exit();
        }
    }

    private void writeInterSeasonFile (float[] totalPerLayer, int biggest)
    {
        StreamWriter outputStream = new StreamWriter(Application.dataPath + "/inter_season.txt", false);
        
        outputStream.WriteLine(biggest + ":" + totalPerLayer[biggest]);
        outputStream.Flush();
        outputStream.Close();
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

    public float[] computeTotalPerLayer (Data[] data)
    {
        float[] totalPerLayer = new float[4];
        for (int i = 0; i < data.Length; i++)
        {
            for (int j = 0; j < data[i].length; j++)
            {
                totalPerLayer[i] += data[i].getHeight(j);
            }
        }
        return totalPerLayer;
    }

    public int findBiggestLayer (float[] totalPerLayer)
    {
        int biggest = 0;
        for (int i = 1; i < totalPerLayer.Length; i++)
        {
            if (totalPerLayer[biggest] < totalPerLayer[i])
            {
                biggest = i;
            }
        }
        return biggest;
    }

    public bool canUnlockNextSeason(float[] totalPerLayer, int biggest)
    {
        float[] total = new float[totalPerLayer.Length];
        bool ok = false;
        for (int i = 0; i < totalPerLayer.Length; i++)
        {
            total[i] = totalPerLayer[i] + 16;
        }
        for (int i = 0; i < total.Length; i++)
        {
            if (i != biggest)
            {
                if (total[biggest] < total[i] * 2f)
                {
                    return false;
                }
                ok = true;
            }
        }
        if (ok)
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
            float yPos = (-1f - (planeHeight * 2)) + (planeHeight * index) + planeHeight;
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
        return height / 2;
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
