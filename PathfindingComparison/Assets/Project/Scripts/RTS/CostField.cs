using System.Collections.Generic;
using UnityEngine;

/// Campo dei costi costruito con Dijkstra.
/// Supporta movimento a 4 o 8-way (con peso di radice di 2 sulle diagonali).
public class CostField
{
    readonly GridManager grid;
    public  readonly float[,] cost;

    public bool allowDiag = false;        // viene impostato da CostFieldMono
    
    public Node goal { get; private set; }

    readonly int   W, H;
    const float INF = 1e9f;

    /*────────────────── ctor ──────────────────*/
    public CostField(GridManager g)
    {
        grid = g;
        W    = g.Width;
        H    = g.Height;
        cost = new float[W, H];
    }

    /*================= Dijkstra  =================*/
    public void Flood(Node goal)
    {
        this.goal = goal; 
        
        // reset matrice
        for (int x = 0; x < W; ++x)
        for (int y = 0; y < H; ++y)
            cost[x, y] = INF;

        // priority-queue (costo minimo in testa)
        var pq = new MiniPriorityQueue<Node, float>();
        cost[goal.x, goal.y] = 0;
        pq.Enqueue(goal, 0);

        // vettori spostamento
        int[] dx4 = {  1, -1,  0,  0 };
        int[] dy4 = {  0,  0,  1, -1 };

        int[] dx8 = {  1, -1,  0,  0,  1,  1, -1, -1 };
        int[] dy8 = {  0,  0,  1, -1,  1, -1,  1, -1 };

        int[] dx = allowDiag ? dx8 : dx4;
        int[] dy = allowDiag ? dy8 : dy4;

        // loop
        while (pq.TryDequeue(out Node cur, out float curCost))
        {
            if (curCost > cost[cur.x, cur.y]) continue;   // già visitato meglio

            for (int k = 0; k < dx.Length; ++k)
            {
                int nx = cur.x + dx[k];
                int ny = cur.y + dy[k];
                if (!grid.InBounds(nx, ny)) continue;

                Node nb = grid.GetNode(nx, ny);
                if (!nb.Walkable) continue;

                bool diag = (dx[k] != 0 && dy[k] != 0);
                float step = (diag ? 1.4142136f : 1f);    // radice di 2 la calcolo così!
                float newC = curCost + step;

                if (newC < cost[nx, ny])
                {
                    cost[nx, ny] = newC;
                    pq.Enqueue(nb, newC);
                }
            }
        }
    }
}