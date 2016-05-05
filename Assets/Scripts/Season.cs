using UnityEngine;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class Season
{
    public int index { get; private set; }
    public string dataPath { get; private set; }
    private Data[] data;

    public Season(int index)
    {
        this.index = index;
        dataPath = Application.dataPath + "/../" + WorldManager.instance.fileName + "_" + index + ".txt";

        updateData();
    }

    public void createFile (string interSeasonFilePath)
    {
        int layer = -1;
        int total = -1;
        if (index > 0 && File.Exists(interSeasonFilePath))
        {
            StreamReader reader = new StreamReader(interSeasonFilePath);
            string line = reader.ReadLine();
            layer = int.Parse(line.Split(':')[0]);
            total = int.Parse(line.Split(':')[1]);
        }

        StreamWriter outputStream = new StreamWriter(dataPath, true);
        for (int i = 0; i < 4; i++)
        {
            int value = 0;
            string line = "";
            if (i == layer)
            {
                value = 1;
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

    public void updateData ()
    {
        string[] lines = File.ReadAllLines(dataPath);
        data = new Data[4];
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            data[lines.Length - 1 - i] = new Data(lines[i].Split('|'));
        }
    }

    public void close ()
    {
        File.Delete(dataPath);
        Process p = Process.GetProcessesByName(WorldManager.instance.exeFileName)[index];
        p.CloseMainWindow();
        p.Close();
    }
}
