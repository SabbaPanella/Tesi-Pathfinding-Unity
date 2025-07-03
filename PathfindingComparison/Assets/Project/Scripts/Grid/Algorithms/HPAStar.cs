using System.Collections.Generic;
using UnityEngine;

/// <summary>Hierarchical Path-Finding A* (HPA*)</summary>
public static class HPAStar
{
    static List<Cluster>                       s_clusters;
    static Dictionary<Node, Cluster>           s_node2cl;
    
    /* cache costi intra-cluster  */
    static readonly Dictionary<(Node, Node), int> intraCost = new();
    
    
    
    public static void SetHierarchy(List<Cluster> clusters,
        Dictionary<Node, Cluster> node2Cl)
    {
        s_clusters = clusters;
        s_node2cl  = node2Cl;
    }
    

    /*────────────────────────── FIND PATH ───────────────────────────────*/

    public static List<Node> FindPath(GridManager grid, Node start, Node goal)
    {
        if (s_clusters == null || s_node2cl == null)
        {
            Debug.LogError("HPA* – gerarchia non inizializzata! Chiama SetHierarchy prima.");
            return null;
        }
        
        
        intraCost.Clear();

        // build gerarchia
        //var clusters = HierarchyBuilder.Build(grid, size, out var node2Cl);
        var startCl = s_node2cl[start];
        var goalCl  = s_node2cl[goal];
        
        if (startCl == goalCl)
            return AStarGrid.FindPath(grid, start, goal, true);   // scorciatoia

        /* ---------- A* sul grafo astratto ---------- */
        var open  = new List<AbstractNode>();
        var close = new HashSet<AbstractNode>();

        AbstractNode aStart = new()
        {
            refNode = start,
            cluster = startCl,
            g = 0,
            h = Heuristic(start, goal)
        };
        open.Add(aStart);

        while (open.Count > 0)
        {
            open.Sort((p, q) => p.f.CompareTo(q.f));
            var cur = open[0];
            open.RemoveAt(0);
            close.Add(cur);

            /* goal cluster raggiunto? */
            if (cur.cluster == goalCl)
            {
                var local = AStarGrid.FindPath(grid, cur.refNode, goal, true);
                var high  = Reconstruct(cur);
                high.AddRange(local);
                return high;
            }

            /* espandi: 1) passa da cur.refNode agli altri ingressi del cluster
                        2) SALTA sull’ingresso gemello nell’altro cluster        */
            foreach (var gateA in cur.cluster.entrances)
            {
                if (gateA == cur.refNode) continue;

                /* costo intra-cluster cur → gateA */
                int cIntra = GetIntraCost(grid, cur.refNode, gateA);
                if (cIntra >= int.MaxValue / 2) continue;  // non connessi

                /* nodo gemello nell’altro cluster */
                if (!HierarchyBuilder.GatePartner.TryGetValue(gateA, out var gateB))
                    continue;                              // fail-safe

                int stepCross = 1;                         // costo di attraversamento porta
                int newG = cur.g + cIntra + stepCross;

                var nxt = new AbstractNode
                {
                    refNode = gateB,
                    cluster = s_node2cl[gateB],
                    g       = newG,
                    parent  = cur
                };
                nxt.h = Heuristic(gateB, goal);

                if (close.TryGetValue(nxt, out var old) && old.g <= nxt.g) continue;
                open.Add(nxt);
            }
        }

        Debug.LogWarning("HPA* => path NULL");
        return null;
    }

    /*──────────────────────── helpers ────────────────────────*/

    static int GetIntraCost(GridManager g, Node a, Node b)
    {
        if (intraCost.TryGetValue((a, b), out int c)) return c;

        var local = AStarGrid.FindPath(g, a, b, true);
        if (local == null) return int.MaxValue / 2;

        c = local.Count;
        intraCost[(a, b)] = intraCost[(b, a)] = c;
        return c;
    }

    static int Heuristic(Node a, Node b) =>
        (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 10; // Manhattan

    static List<Node> Reconstruct(AbstractNode n)
    {
        var res  = new List<Node>();
        for (var cur = n; cur != null; cur = cur.parent)
            res.Add(cur.refNode);
        res.Reverse();
        return res;
    }
}

/* nodo astratto */
class AbstractNode
{
    public Node         refNode;
    public Cluster      cluster;
    public int          g, h;
    public int  f => g + h;
    public AbstractNode parent;

    public override bool Equals(object obj) =>
        obj is AbstractNode other && refNode == other.refNode;
    public override int GetHashCode()        => refNode.GetHashCode();
}