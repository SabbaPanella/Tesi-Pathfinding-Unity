using System.Collections.Generic;
using UnityEngine;
using System;

// -----------------------------------------------------------------
//  Wrapper usato nella PQ
// -----------------------------------------------------------------
public struct NodeCostPair : IComparable<NodeCostPair>
{
    public Node  node;
    public int   f;      // chiave di priorità (f = g + h)

    public int CompareTo(NodeCostPair other) => f.CompareTo(other.f);
}

public static class JumpPointSearch
{
    // --------------------------------------------------------------
    //  DIREZIONI
    // --------------------------------------------------------------
    static readonly Vector2Int[] DIR_4 =
    {
        new( 0, 1), new( 1, 0),
        new( 0,-1), new(-1, 0)
    };
    static readonly Vector2Int[] DIR_8 =
    {
        new( 0,  1), new( 1,  1), new( 1, 0), new( 1, -1),
        new( 0, -1), new(-1, -1), new(-1, 0), new(-1,  1)
    };

    // --------------------------------------------------------------
    //  FUNZIONE PRINCIPALE
    // --------------------------------------------------------------
    public static List<Node> FindPath(GridManager grid, Node start, Node goal, bool allowDiag = true)
    {
        // PQ + set di controllo
        var DIRS = allowDiag ? DIR_8 : DIR_4; 
        var open     = new MinPriorityQueue<NodeCostPair>();     
        var openSet  = new HashSet<Node>();                     
        var closed   = new HashSet<Node>();

        start.g = 0;
        start.h = Heu(start, goal);
        start.f = start.h;

        open.Enqueue(new NodeCostPair { node = start, f = start.f });  
        openSet.Add(start);                                            

        while (open.Count > 0)
        {
            // Estrai migliore
            Node current = open.Dequeue().node;    
            openSet.Remove(current);               

            if (current == goal)
                return Reconstruct(goal);

            closed.Add(current);

            foreach (var dir in DIRS)
            {
                Node jump = Jump(grid, current, dir, goal);
                if (jump == null || closed.Contains(jump)) continue;

                int tentativeG = current.g + Cost(current, jump);

                bool inOpen = openSet.Contains(jump);            

                if (tentativeG < jump.g || !inOpen)
                {
                    jump.g      = tentativeG;
                    jump.h      = Heu(jump, goal);
                    jump.f      = jump.g + jump.h;
                    jump.parent = current;

                    if (!inOpen)
                    {
                        open.Enqueue(new NodeCostPair             
                        {
                            node = jump,
                            f    = jump.f
                        });
                        openSet.Add(jump);                       
                    }
                }
            }
        }

        return null; // nessun percorso trovato
    }

    // --------------------------------------------------------------
    //  JUMP RECURSIVO E UTILITIES
    // --------------------------------------------------------------
    static Node Jump(GridManager grid, Node curr, Vector2Int dir, Node goal)
    {
        int nx = curr.x + dir.x;
        int ny = curr.y + dir.y;

        if (!grid.InBounds(nx, ny) || !grid.GetNode(nx, ny).walkable)
            return null;

        Node next = grid.GetNode(nx, ny);
        if (next == goal) return next;

        if (HasForcedNeighbour(grid, next, dir))
            return next;

        if (dir.x != 0 && dir.y != 0)
        {
            if (Jump(grid, next, new Vector2Int(dir.x, 0), goal) != null) return next;
            if (Jump(grid, next, new Vector2Int(0, dir.y), goal) != null) return next;
        }
        return Jump(grid, next, dir, goal);
    }

    static bool HasForcedNeighbour(GridManager g, Node n, Vector2Int d)
    {
        if (d.x != 0 && d.y != 0)
        {
            return (!g.IsWalkable(n.x - d.x, n.y)     && g.IsWalkable(n.x - d.x, n.y + d.y)) ||
                   (!g.IsWalkable(n.x,     n.y - d.y) && g.IsWalkable(n.x + d.x, n.y - d.y));
        }
        else if (d.x != 0)
        {
            return (!g.IsWalkable(n.x, n.y + 1) && g.IsWalkable(n.x + d.x, n.y + 1)) ||
                   (!g.IsWalkable(n.x, n.y - 1) && g.IsWalkable(n.x + d.x, n.y - 1));
        }
        else
        {
            return (!g.IsWalkable(n.x + 1, n.y) && g.IsWalkable(n.x + 1, n.y + d.y)) ||
                   (!g.IsWalkable(n.x - 1, n.y) && g.IsWalkable(n.x - 1, n.y + d.y));
        }
    }

    static int Heu(Node a, Node b) =>
        10 * (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));

    static int Cost(Node a, Node b) =>
        (a.x == b.x || a.y == b.y) ? 10 : 14;

    static List<Node> Reconstruct(Node n)
    {
        var path = new List<Node>();
        for (var cur = n; cur != null; cur = cur.parent) path.Add(cur);
        path.Reverse();
        return path;
    }
}