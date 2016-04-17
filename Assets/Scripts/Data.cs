using UnityEngine;
using System.Collections;

public class Data
{
    private string[] heights;

    public Data (string[] heights)
    {
        this.heights = heights;
    }

    public float getHeight (int index)
    {
        int actualIndex = Mathf.FloorToInt(index * 16 / 64);
        float value;
        if (float.TryParse(heights[actualIndex], out value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }
}
