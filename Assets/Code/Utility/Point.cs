using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public int x;
    public int y;

    public Point() { }

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Point operator+ (Point a, Point b)
    {
        return new Point(a.x + b.x, a.y + b.y);
    }

    public static Point operator* (Point a, int b)
    {
        return new Point(a.x * b, a.y * b);
    }

    public static bool operator==(Point a, Point b)
    {
        return (a.x == b.x && a.y == b.y);
    }

    public static bool operator!=(Point a, Point b)
    {
        return !(a == b);
    }

    public static Point Up { get { return new Point(0, 1); } }
    public static Point Down { get { return new Point(0, -1); } }
    public static Point Left { get { return new Point(-1, 0); } }
    public static Point Right { get { return new Point(1, 0); } }
}
