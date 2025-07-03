using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathLine : MonoBehaviour
{
    LineRenderer lr;
    void Awake() => lr = GetComponent<LineRenderer>();

    public void Show(List<Vector3> wp)
    {
        if (wp == null) { lr.positionCount = 0; return; }

        lr.positionCount = wp.Count;
        lr.SetPositions(wp.ToArray());
    }
}