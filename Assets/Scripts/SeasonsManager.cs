using UnityEngine;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class SeasonsManager : MonoBehaviour
{
    private static SeasonsManager _instance;

    public static SeasonsManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("SeasonsManager").GetComponent<SeasonsManager>();
            }
            return _instance;
        }
    }

    public string dataFileName;
    public string interSeasonFileName;
    public string processFileName;

    private Season[] seasons;
    private int numberOfSeasons;
    private int currentSeasonIndex;
    private int lastSeasonIndex;


    void Start ()
    {
        loadSeasons();

        currentSeasonIndex = numberOfSeasons - 1;

        seasons[currentSeasonIndex].createFile(Application.dataPath + "/" + interSeasonFileName + ".txt");
    }

    private void loadSeasons ()
    {
        seasons = getAllSeasons();
        for (int i = 0; i < seasons.Length; i++)
        {
            if (seasons[i] != null)
            {
                numberOfSeasons++;
            }
        }
    }

    void Update()
    {
        loadSeasons();

        if (File.Exists(seasons[currentSeasonIndex].dataPath))
        {
            for (int i = 0; i < seasons.Length; i++)
            {
                if (seasons[i] != null)
                {
                    seasons[i].updateData();
                }
            }
        }
        else
        {
            GameManager.instance.exit();
        }
    }

    private Season[] getAllSeasons()
    {
        Process[] processes = Process.GetProcessesByName(processFileName);
        Season[] seasons = new Season[processes.Length];
        int index = 0;
        foreach (Process process in processes)
        {
            seasons[index] = new Season(index);
            index++;
        }
        return seasons;
    }
}
