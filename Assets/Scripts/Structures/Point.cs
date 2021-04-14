using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Vector2 position;
    public int propIndex;

    public Point(Vector2 position, int propIndex)
    {
        this.position = position;
        this.propIndex = propIndex;
    }
}
