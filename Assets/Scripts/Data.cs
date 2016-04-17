using UnityEngine;
using System.Collections;

public class Data
{
    private string[] heights;
    public int length { get; private set; }

    public Data (string[] heights)
    {
        this.heights = heights;
        length = heights.Length;
    }

    public float getHeight (int index)
    {
        float value;
        if (index < heights.Length)
        {
            if (float.TryParse(heights[index], out value))
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > 4)
                {
                    value = 4;
                }
                return value;
            }
        }
        return 0;
    }
}
