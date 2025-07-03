using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshAStar
{
    /// <summary> Restituisce una lista di corner world-space, incluso start & goal. </summary>
    public static List<Vector3> FindPath(Vector3 start, Vector3 goal)
    {
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(start, goal, NavMesh.AllAreas, path)
            || path.status != NavMeshPathStatus.PathComplete)
            return null;

        return new List<Vector3>(path.corners);
    }
}