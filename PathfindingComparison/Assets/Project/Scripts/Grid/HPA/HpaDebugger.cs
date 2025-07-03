using UnityEngine;

/// Visualizza cluster (bounding-box) e porte create da HierarchyBuilder
[ExecuteAlways]
public class HpaDebugger : MonoBehaviour
{
    public GridManager grid;
    public Color clusterFill  = new(1, 0, 1, .05f);
    public Color clusterEdge  = new(1, 0, 1, .6f);
    public Color entranceCol  = Color.cyan;

    void OnDrawGizmos()
    {
        if (grid == null) return;

        // recupera stessa size salvata dal dropdown
        int size = PlayerPrefs.GetInt("HPA_SIZE", 4);

        if (HierarchyBuilder.LastBuildClusters == null)
        {
            HierarchyBuilder.Build(grid, size, out _);   // passa size
        }
        
        var clusters = HierarchyBuilder.LastBuildClusters;
        if (clusters == null) return;

        foreach (var cl in clusters)
        {
            // bounding-box pieno
            Gizmos.color = clusterFill;
            Gizmos.DrawCube(cl.WorldCenter(grid), cl.WorldSize(grid));

            // contorno
            Gizmos.color = clusterEdge;
            Gizmos.DrawWireCube(cl.WorldCenter(grid), cl.WorldSize(grid));

            // porte
            Gizmos.color = entranceCol;
            foreach (var e in cl.entrances)
                Gizmos.DrawCube( grid.CellCenter(e.GridPos), Vector3.one * .25f );
        }
    }
}