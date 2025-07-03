using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* su griglia (4-dir o 8-dir a scelta).
///
/// Requisiti minimi della classe Node:
///   int          gCost, hCost;          // costi
///   Node         CameFrom;              // parent
///   int          FCost   => gCost + hCost;
///   Vector2Int   GridPos;               // coordinate discrete
///   bool         Walkable;
///
/// NB: nel costruttore di Node inizializza gCost = int.MaxValue.
/// </summary>
public static class AStarGrid
{
    const int ORTHO = 10;   // passo ortogonale
    const int DIAG  = 14;   // passo diagonale (≈√2 * 10)

    /*───────────────────────────────────────────────────────────────*/
    public static List<Node> FindPath(GridManager grid,
                                      Node         start,
                                      Node         goal,
                                      bool         allowDiag = true)
    {
        /* --------- validazione input --------- */
        if (grid == null || start == null || goal == null ||
            !start.Walkable || !goal.Walkable)
            return null;

        /* --------- reset campi A* --------- */
        foreach (var n in grid.AllNodes())
        {
            n.gCost    = int.MaxValue;
            n.hCost    = 0;
            n.CameFrom = null;
        }

        /* --------- strutture dati --------- */
        var open    = new List<Node>();      // min-heap semplificato con Sort
        var closed  = new HashSet<Node>();
        var touched = new List<Node>();      // nodi da ripulire a fine ricerca

        /* --------- inizializza start --------- */
        start.gCost = 0;
        start.hCost = Heuristic(start, goal, allowDiag);
        open.Add(start);
        touched.Add(start);

        /* =================== loop principale ===================== */
        while (open.Count > 0)
        {
            open.Sort((a, b) => a.FCost.CompareTo(b.FCost));
            var current = open[0];
            open.RemoveAt(0);

            if (current == goal)          // percorso trovato
            {
                var path = Reconstruct(goal);
                Cleanup(touched);         // reset dei flag usati
                return path;
            }

            closed.Add(current);

            foreach (var n in grid.Neighbours(current, allowDiag))
            {
                if (closed.Contains(n) || !n.Walkable) continue;

                int step = (n.GridPos.x == current.GridPos.x ||
                            n.GridPos.y == current.GridPos.y) ? ORTHO : DIAG;

                int tentativeG = current.gCost + step;

                if (tentativeG < n.gCost)          // se è cammino migliore
                {
                    n.CameFrom = current;
                    n.gCost    = tentativeG;
                    n.hCost    = Heuristic(n, goal, allowDiag);

                    if (!open.Contains(n)) open.Add(n);
                    if (!touched.Contains(n)) touched.Add(n);
                }
            }
        }

        /* --------- nessuna soluzione --------- */
        Cleanup(touched);
        return null;
    }

    /*──────────────────────── helpers ─────────────────────────────*/

    // Manhattan (4-dir) oppure Octile (8-dir)
    static int Heuristic(Node a, Node b, bool allowDiag)
    {
        int dx = Mathf.Abs(a.GridPos.x - b.GridPos.x);
        int dy = Mathf.Abs(a.GridPos.y - b.GridPos.y);

        if (allowDiag)
            // Octile distance: 14*min + 10*(max-min)
            return DIAG * Mathf.Min(dx, dy) + ORTHO * Mathf.Abs(dx - dy);
        else
            return ORTHO * (dx + dy);
    }

    static List<Node> Reconstruct(Node n)
    {
        var path = new List<Node>();
        var watchdog = 0;                 // evita loop rari per referenze corrotte

        for (var cur = n; cur != null && watchdog++ < 4096; cur = cur.CameFrom)
            path.Add(cur);

        path.Reverse();
        return path;
    }

    // re-inizializza solo i nodi toccati dall’algoritmo
    static void Cleanup(List<Node> list)
    {
        foreach (var n in list)
        {
            n.gCost    = int.MaxValue;
            n.hCost    = 0;
            n.CameFrom = null;
        }
    }
}