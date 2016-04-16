using UnityEngine;
using System.Collections;
using System.IO;

public class World
{
    private int index;
    private string fileName;
    private StreamWriter outputStream;


    public World ()
    {
        index = 0;
        do
        {
            fileName = Application.dataPath + "/../world_" + index + ".txt";
            index++;
        }
        while (File.Exists(fileName));
        index--;

        // Create world file
        writeLines(4);
    }

    private void writeLines (int number)
    {
        outputStream = new StreamWriter(fileName, true);
        for (int i = 0; i < number; i++)
        {
            outputStream.WriteLine("0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0");
        }
        outputStream.Flush();
        outputStream.Close();
    }

    public Data[] retrieveData ()
    {
        string[] lines = File.ReadAllLines(fileName);
        /*if (lines.Length < 4)
        {
            writeLines(1);
            lines = File.ReadAllLines(fileName);
        }*/
        Data[] data = new Data[4];
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            data[lines.Length - 1 - i] = new Data(lines[i].Split(','));
        }
        return data;
    }

    public void delete ()
    {
        File.Delete(fileName);
    }

    public string getFileName ()
    {
        return fileName;
    }
}
