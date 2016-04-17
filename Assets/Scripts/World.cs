using UnityEngine;
using System.Collections;
using System.IO;

public class World
{
    private int index;


    public World ()
    {
        index = 0;
        string path;
        do
        {
            path = Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + index + ".txt";
            index++;
        }
        while (File.Exists(path));
        index--;

        if (index != 0 && (index > 3 || !canBeOpened()))
        {
            GameManager.instance.exit();
        }

        // Read inter season file
        string p = Application.dataPath + "/inter_season.txt";
        int layer = -1;
        int total = -1;
        if (File.Exists(p))
        {
            StreamReader reader = new StreamReader(p);
            string line = reader.ReadLine();
            layer = int.Parse(line.Split(':')[0]);
            total = int.Parse(line.Split(':')[1]);
        }

        // Create world file
        StreamWriter outputStream = new StreamWriter(path, true);
        for (int i = 0; i < 4; i++)
        {
            int value = 0;
            string line = "";
            if (i == layer)
            {
                value = total / 32;
            }
            for (int j = 0; j < 16; j++)
            {
                if (j != 15)
                {
                    line += value + "|";
                }
                else
                {
                    line += value;
                }
            }
            outputStream.WriteLine(line);
        }
        outputStream.Flush();
        outputStream.Close();
    }

    private bool canBeOpened ()
    {
        string path = Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + (index - 1) + ".txt";
        if (File.Exists(path))
        {
            float[] totalPerLayer = WorldManager.instance.computeTotalPerLayer(retrieveData(path));
            int biggest = WorldManager.instance.findBiggestLayer(totalPerLayer);
            return WorldManager.instance.canUnlockNextSeason(totalPerLayer, biggest);
        }
        return false;
    }

    public int getNextWorldIndex ()
    {
        if (index + 1 < 4)
        {
            return index + 1;
        }
        return -1;
    }

    public string getNextWorldPath()
    {
        if (getNextWorldIndex() > 0)
        {
            return Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + getNextWorldIndex() + ".txt";
        }
        return "";
    }

    public int getNextWorldIndex(int i)
    {
        if (i + 1 < 4)
        {
            return i + 1;
        }
        return -1;
    }

    public string getNextWorldPath(int i)
    {
        if (getNextWorldIndex(i) > 0)
        {
            return Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + getNextWorldIndex(i) + ".txt";
        }
        return "";
    }

    public Data[] retrieveData (string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        Data[] data = new Data[4];
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            data[lines.Length - 1 - i] = new Data(lines[i].Split('|'));
        }
        return data;
    }

    public void delete ()
    {
        File.Delete(Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + index + ".txt");
    }

    public void delete(int worldIndex)
    {
        File.Delete(Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + worldIndex + ".txt");
    }

    public string getFilePath ()
    {
        return Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + index + ".txt";
    }

    public int getIndex ()
    {
        return index;
    }
}
