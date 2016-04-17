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

        // Create world file
        StreamWriter outputStream = new StreamWriter(path, true);
        for (int i = 0; i < 4; i++)
        {
            outputStream.WriteLine("5|5|5|5|5|5|5|5|5|5|5|5|5|5|5|5");
        }
        outputStream.Flush();
        outputStream.Close();
    }

    private bool canBeOpened ()
    {
        string path = Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + (index - 1) + ".txt";
        if (File.Exists(path))
        {
            return WorldManager.instance.canUnlockNextSeason(retrieveData(path));
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
