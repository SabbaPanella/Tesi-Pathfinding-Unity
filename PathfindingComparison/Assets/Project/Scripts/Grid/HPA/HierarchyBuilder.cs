using System.Collections.Generic;
using UnityEngine;

/// Crea la gerarchia di cluster per HPA*
public static class HierarchyBuilder
{
    //public const int CLUSTER_SIZE = 4;
    
    public  static List<Cluster>              LastBuildClusters { get; private set; }

    // mappa nodo -> nodo partner (l’altro lato della porta)
    public  static readonly Dictionary<Node, Node> GatePartner = new();

    /*──────────────────────────────────────────────────────────────────────────*/

    public static List<Cluster> Build(GridManager grid, int clusterSize,
                                      out Dictionary<Node, Cluster> node2Cl)
    {
        GatePartner.Clear();

        int W = grid.Width, H = grid.Height;

        node2Cl = new Dictionary<Node, Cluster>();
        var list = new List<Cluster>();

        // 1- suddivisione regolare
        int id = 0;
        for (int x = 0; x < W; x += clusterSize)
        for (int y = 0; y < H; y += clusterSize)
        {
            var cl = new Cluster(id++, x, y, clusterSize, W, H);
            list.Add(cl);

            for (int ix = x; ix < Mathf.Min(x + clusterSize, W); ++ix)
            for (int iy = y; iy < Mathf.Min(y + clusterSize, H); ++iy)
            {
                var n = grid.GetNode(ix, iy);
                if (!n.Walkable) continue;

                cl.nodes.Add(n);
                node2Cl[n] = cl;
            }
        }

        // 2- individua le porte (4-dir + diagonali) 
        DetectEntrances(grid, node2Cl);

        LastBuildClusters = list;
        return list;
    }

    /*────────────────────────────── helpers ──────────────────────────────*/

    static void DetectEntrances(GridManager               grid,
                                Dictionary<Node, Cluster> n2c)
    {
        int W = grid.Width, H = grid.Height;

        /* helper locale */
        void AddGate(Node a, Node b)
        {
            if (!a.Walkable || !b.Walkable) return;
            var ca = n2c[a];
            var cb = n2c[b];
            if (ca == cb) return;                     // non è frontiera

            // aggiungi una sola volta ciascun lato nei rispettivi cluster
            if (!ca.entrances.Contains(a)) ca.entrances.Add(a);
            if (!cb.entrances.Contains(b)) cb.entrances.Add(b);

            // salva la mappatura a coppia (A <-> B) 
            GatePartner[a] = b;
            GatePartner[b] = a;
        }

        // verticali x-1 / x 
        for (int x = 1; x < W; ++x)
        for (int y = 0; y < H; ++y)
            AddGate(grid.GetNode(x - 1, y), grid.GetNode(x, y));

        // orizzontali y-1 / y 
        for (int y = 1; y < H; ++y)
        for (int x = 0; x < W; ++x)
            AddGate(grid.GetNode(x, y - 1), grid.GetNode(x, y));

        // diagonali 
        for (int x = 1; x < W; ++x)
        for (int y = 1; y < H; ++y)
        {
            AddGate(grid.GetNode(x - 1, y - 1), grid.GetNode(x, y));    
            AddGate(grid.GetNode(x, y - 1),     grid.GetNode(x - 1, y));
        }
    }
}