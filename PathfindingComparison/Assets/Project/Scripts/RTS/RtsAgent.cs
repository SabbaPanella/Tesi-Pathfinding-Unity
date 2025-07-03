using UnityEngine;

/// Muove un agente seguendo il gradiente minimo del Cost-Field
/// e applica un leggero steering di separazione per non sovrapporsi
/// ad altri agenti sullo stesso layer.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class RtsAgent : MonoBehaviour
{
    /* ───────────── parametri movimento ───────────── */
    [Header("Movement")]
    [Min(0)] [SerializeField] float speed = 4f;

    [Header("Local-Avoidance")]
    [SerializeField] float      separationRadius  = 0.6f;
    [SerializeField] float      separationWeight  = 1.2f;
    [SerializeField] LayerMask  neighbourMask     = 0;

    /* ───────────── cache ───────────── */
    GridManager grid;
    CostField   cost;
    Rigidbody2D rb;

    // dir tables 
    static readonly int[] DX4 = {  0,  0,  1, -1 };
    static readonly int[] DY4 = {  1, -1,  0,  0 };

    static readonly int[] DX8 = {  0,  0,  1, -1,  1,  1, -1, -1 };
    static readonly int[] DY8 = {  1, -1,  0,  0,  1, -1,  1, -1 };

    int[] dx = DX4, dy = DY4;   // puntatori correnti

    // buffer riutilizzato per evitare allocazioni
    readonly Collider2D[] neighBuf = new Collider2D[16];

    /* ───────────── LIFECYCLE ───────────── */
    void Awake()
    {
        grid = FindFirstObjectByType<GridManager>();
        rb   = GetComponent<Rigidbody2D>();

        rb.gravityScale   = 0f;
        rb.freezeRotation = true;

        CacheCostField();   // iniziale
    }

    void CacheCostField()
    {
        var cfm = FindFirstObjectByType<CostFieldMono>();
        if (cfm == null) { cost = null; return; }

        cost = cfm.Field;

        bool useDiag = cfm.AllowDiag;
        dx = useDiag ? DX8 : DX4;
        dy = useDiag ? DY8 : DY4;
    }

    /* ───────────── UPDATE ───────────── */
    void FixedUpdate()
    {
        if (cost == null || cost.cost == null)
        {
            CacheCostField();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 1- direzione dal cost-field
        Vector2 gradDir = ComputeGradientDir(out bool stuck);
        if (stuck) { rb.linearVelocity = Vector2.zero; return; }

        // 2- direzione di separazione
        Vector2 sepDir = ComputeSeparationDir();

        // 3- blend & apply
        Vector2 finalDir = (gradDir + separationWeight * sepDir).normalized;
        rb.linearVelocity = finalDir * speed;
    }

    /* ───────────── gradiente ───────────── */
    Vector2 ComputeGradientDir(out bool stuck)
    {
        stuck = false;

        Node n = grid.WorldToNode(transform.position);
        if (n == null)                       // se fuori griglia fai warp
        {
            WarpToNearestWalkable();
            stuck = true;
            return Vector2.zero;
        }

        float myCost = cost.cost[n.x, n.y];
        if (float.IsPositiveInfinity(myCost))    // nodo non camminabile
        {
            WarpToNearestWalkable();
            stuck = true;
            return Vector2.zero;
        }

        // cerca il vicino col costo più basso
        float       bestVal = myCost;
        Vector2Int bestAt  = new Vector2Int(n.x, n.y);

        for (int k = 0; k < dx.Length; ++k)
        {
            int nx = n.x + dx[k];
            int ny = n.y + dy[k];
            if (!grid.InBounds(nx, ny)) continue;

            float v = cost.cost[nx, ny];
            if (float.IsPositiveInfinity(v)) continue;

            // 1- costo strettamente minore
            // 2- tie-break: stesso costo ma più vicino al goal (rompo il pareggio così non si blocca più)
            if (v < bestVal || (Mathf.Approximately(v, bestVal) &&
                                CloserToGoal(nx, ny, bestAt)))
            {
                bestVal = v;
                bestAt  = new Vector2Int(nx, ny);
            }
        }

        if (bestAt == n.GridPos) { stuck = true; return Vector2.zero; }

        Vector2 target = grid.CellCenter(bestAt);
        return (target - (Vector2)transform.position).normalized;
    }

    bool CloserToGoal(int x, int y, Vector2Int currentBest)
    {
        // goal GridPos salvato nel CostField
        Node g = cost.goal;
        int  dNew = Mathf.Abs(x - g.x) + Mathf.Abs(y - g.y);
        int  dCur = Mathf.Abs(currentBest.x - g.x) + Mathf.Abs(currentBest.y - g.y);
        return dNew < dCur;
    }

    /* ───────────── separazione ───────────── */
    Vector2 ComputeSeparationDir()
    {
        int n = Physics2D.OverlapCircleNonAlloc(
                    transform.position,
                    separationRadius,
                    neighBuf,
                    neighbourMask);

        if (n == 0) return Vector2.zero;

        Vector2 force = Vector2.zero;
        Vector2 mePos = transform.position;

        for (int i = 0; i < n; ++i)
        {
            var col = neighBuf[i];
            if (col == null || col.gameObject == gameObject) continue;

            Vector2 toMe  = mePos - (Vector2)col.transform.position;
            float   dist  = toMe.magnitude;
            if (dist < 0.001f) continue;

            force += toMe / (dist * dist);   // fall-off 1/r^2
        }

        return force.normalized;
    }
    

    /* ───────────── warp helper ───────────── */
    void WarpToNearestWalkable()
    {
        Node cur = grid.WorldToNode(transform.position);

        // Se anche cur è null (fuori griglia del tutto) parti dal nodo più
        // vicino sul bordo clamped.
        if (cur == null)
        {
            Vector2Int gp = grid.ClampToBounds(transform.position);
            cur = grid.GetNode(gp.x, gp.y);
        }

        if (cur == null) return;   // griglia non inizializzata

        const int R_MAX = 32;      // al massimo 32 celle di raggio
        Node best = null;
        int  bestSq = int.MaxValue;

        for (int r = 0; r <= R_MAX && best == null; ++r)
        {
            // perimetro del quadrato di lato 2r+1 centrato in cur
            for (int dx = -r; dx <= r; ++dx)
            for (int dy = -r; dy <= r; ++dy)
            {
                // salta i punti interni: visitiamo solo il perimetro
                if (System.Math.Abs(dx) != r && System.Math.Abs(dy) != r) continue;

                int nx = cur.x + dx;
                int ny = cur.y + dy;
                if (!grid.InBounds(nx, ny)) continue;

                if (!float.IsPositiveInfinity(cost.cost[nx, ny]))
                {
                    int sq = dx * dx + dy * dy;
                    if (sq < bestSq)
                    {
                        best   = grid.GetNode(nx, ny);
                        bestSq = sq;
                    }
                }
            }
        }

        if (best != null)
        {
            rb.position       = grid.CellCenter(new Vector2Int(best.x, best.y));
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    public void ReSnapToGrid()
    {
        if (grid == null || cost == null) return;
        WarpToNearestWalkable();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan * 0.5f;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
#endif
}