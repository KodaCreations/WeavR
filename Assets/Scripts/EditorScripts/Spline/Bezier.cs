using UnityEngine;

public static class Bezier
{
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float time)
    {

        //return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
        
        time = Mathf.Clamp01(time);
        float oneMinusT = 1f - time;
        return  oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * time * p1 +
                time * time * p2;
    }
    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float time)
    {
        return  2f * (1f - time) * (p1 - p0) +
                2f * time * (p2 - p1);
    }
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * oneMinusT * p0 +
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;
    }
    public static Vector3 GetPointConstantSpeed(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, ref float t, float length)
    {
        float time = t % 1;
        Vector3 v1, v2, v3;

        v1 = -3 * p0 + 9 * p1 - 9 * p2 + 3 * p3; 
        v2 = 6 * p0 - 12 * p1 + 6 * p2;
        v3 = -3 * p0 + 3 * p1;

        time = time + length / Vector3.Magnitude(time * time * v1 + time * v2 + v3);
        t = (int)t + time;

        return GetPoint(p0, p1, p2, p3, time);
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }
}
