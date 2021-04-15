using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Vector2 position;
    public int propId;

    public Point(Vector2 position, int propId)
    {
        this.position = position;
        this.propId = propId;
    }
}
