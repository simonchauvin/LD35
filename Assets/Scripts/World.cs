using UnityEngine;
using System.Collections;
using System.IO;

public class World
{
    private int index;
    private string filePath;
    private StreamWriter outputStream;


    public World (string fileName)
    {
        index = 0;
        do
        {
            filePath = Application.dataPath + "/../" + fileName + "_" + index + ".txt";
            index++;
        }
        while (File.Exists(filePath));
        index--;

        if (index > 3)
        {
            GameManager.instance.exit();
        }

        // Create world file
        outputStream = new StreamWriter(filePath, true);
        for (int i = 0; i < 4; i++)
        {
            outputStream.WriteLine("5|5|5|5|5|5|5|5|5|5|5|5|5|5|5|5");
        }
        outputStream.Flush();
        outputStream.Close();
    }

    public Data[] retrieveData ()
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
        File.Delete(filePath);
    }

    public string getFilePath ()
    {
        return filePath;
    }

    public int getIndex ()
    {
        return index;
    }
}
