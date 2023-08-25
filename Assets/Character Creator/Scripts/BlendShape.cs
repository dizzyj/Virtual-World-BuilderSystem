using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShape
{
    public int index { get; }
    public float currentValue { get; set; }

    public BlendShape(int index)
    {
        this.index = index;
        currentValue = 0;
    }

} 
