using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;              
#endif

/// Disegna cluster e ingressi di HPA* anche in Game-view.
[ExecuteAlways]
public class ClusterRenderer : MonoBehaviour
{
    [Header("References")]
    public GridManager grid;
    public Material    mat;                     

    [Header("Colors")]
    public Color fillColor   = new(1, 0, 1, .04f);
    public Color borderColor = new(1, 0, 1, .55f);
    public Color entranceCol = new Color(0, 1, 1, 1);   

    [Header("Sizes")]
    [Range(.1f, .9f)]
    public float entranceScale = .8f;          // lato quad ingresso %

    //------------------------------------------------------------------

    void OnRenderObject()
    {
        if (!CanDraw()) return;
        mat.SetPass(0);

        foreach (var cl in HierarchyBuilder.LastBuildClusters)
        {
            /*if (cl.entrances.Count == 0)
                Debug.Log($"cluster {cl.id} nessuna porta");
            else
                Debug.Log($"cluster {cl.id} porte: {cl.entrances.Count}"); */
            
            DrawCluster(cl);
            DrawEntrances(cl);
        }
    }

    /*────────────────── helpers ──────────────────*/

    bool CanDraw() =>
        grid && mat && HierarchyBuilder.LastBuildClusters != null;

    void DrawCluster(Cluster cl)
    {
        float cs = grid.CellWorldSize;       // lato di una cella in world-space
        // Bottom-left corner (punto di origine, NON centro)
        Vector3 bl = grid.CellCenter(cl.min) - new Vector3(cs * .5f, cs * .5f, 0);

        // Altri tre angoli partendo da bl
        Vector3 br = bl + new Vector3(cs * (cl.max.x - cl.min.x + 1), 0,                 0);
        Vector3 tr = bl + new Vector3(cs * (cl.max.x - cl.min.x + 1),
            cs * (cl.max.y - cl.min.y + 1), 0);
        Vector3 tl = bl + new Vector3(0,
            cs * (cl.max.y - cl.min.y + 1), 0);

        /* ----------- riempi ----------- */
        GL.Begin(GL.TRIANGLES);
        GL.Color(fillColor);
        GL.Vertex(bl); GL.Vertex(br); GL.Vertex(tr);
        GL.Vertex(tr); GL.Vertex(tl); GL.Vertex(bl);
        GL.End();

        /* ----------- bordo ------------ */
        GL.Begin(GL.LINES);
        GL.Color(borderColor);
        GL.Vertex(bl); GL.Vertex(br);
        GL.Vertex(br); GL.Vertex(tr);
        GL.Vertex(tr); GL.Vertex(tl);
        GL.Vertex(tl); GL.Vertex(bl);
        GL.End();
    }

    void DrawEntrances(Cluster cl)
    {
        if (cl.entrances == null || cl.entrances.Count == 0) return;

        float cs   = grid.CellWorldSize;
        float half = entranceScale * .5f * cs;
        float zShift = -0.5f * grid.CellWorldSize;  //  più vicino alla camera

        GL.Begin(GL.QUADS);
        GL.Color(entranceCol);

        foreach (var n in cl.entrances)
        {
            Vector3 c = grid.CellCenter(n.GridPos);

            GL.Vertex(c + new Vector3(-half, -half, zShift));
            GL.Vertex(c + new Vector3( half, -half, zShift));
            GL.Vertex(c + new Vector3( half,  half, zShift));
            GL.Vertex(c + new Vector3(-half,  half, zShift));
        }
        GL.End();
    }
    
    /// Chiamato dallo UI quando la gerarchia viene ricreata
    public void ForceRepaint()
    {
#if UNITY_EDITOR
        SceneView.RepaintAll();
#endif
    }
}