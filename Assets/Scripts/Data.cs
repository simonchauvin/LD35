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
        if (actualIndex < heights.Length)
        {
            if (float.TryParse(heights[actualIndex], out value))
            {
                return value;
            }
        }
        return 0;
    }
}
