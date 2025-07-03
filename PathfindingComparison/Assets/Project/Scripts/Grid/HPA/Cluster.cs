using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contenitore di un blocco di celle della griglia (cluster per HPA*)
/// </summary>
public class Cluster
{
    /*────────────────────────────  dati interni  ───────────────────────────*/
    public readonly int id;

    /// Angolo in basso-sinistra in coordinate griglia
    public readonly Vector2Int min;

    /// Angolo in alto-destra (incluso) in coordinate griglia
    public readonly Vector2Int max;

    /// Celle walkable interne (comodo se servono iterazioni locali)
    public readonly List<Node> nodes = new();

    /// Celle walkable fronte-fronte con cluster adiacenti (porte)
    public readonly List<Node> entrances = new();

    /*────────────────────────────  ctor  ───────────────────────────*/
    public Cluster(int id, int minX, int minY, int side, int gridW, int gridH)
    {
        this.id = id;

        // coord. min (BL) e max (TR) ritagliate ai bordi griglia
        min = new Vector2Int(minX, minY);
        max = new Vector2Int(
            Mathf.Min(minX + side - 1, gridW - 1),
            Mathf.Min(minY + side - 1, gridH - 1));
    }

    /*────────────────────────────  helpers grafici  ───────────────────────────*/

    /// Centro geometrico del cluster in world-space.
    public Vector3 WorldCenter(GridManager g)
    {
        float cs = g.CellWorldSize;
        float cx = (min.x + max.x + 1) * 0.5f * cs;
        float cy = (min.y + max.y + 1) * 0.5f * cs;

        // se il GridManager è ruotato/traslato, trasformiamo nel suo spazio
        return g.transform.TransformPoint(new Vector3(cx, cy, 0f));
    }

    /// Dimensioni world-space del parallelepipedo da disegnare come gizmo.
    /// L'asse Z è volutamente un “foglietto” sottile (0.02 m) per non
    /// dare l'impressione di estrusione in profondità.
    public Vector3 WorldSize(GridManager g)
    {
        float cs = g.CellWorldSize;
        float w  = (max.x - min.x + 1) * cs;
        float h  = (max.y - min.y + 1) * cs;
        return new Vector3(w, h, 0.02f);
    }
}