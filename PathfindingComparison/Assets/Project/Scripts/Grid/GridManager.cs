using UnityEngine;
using System.Collections.Generic;


public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int width = 20, height = 12;
    
    public int Width  => width;
    public int Height => height;
    [SerializeField] private float cellWorldSize = 1f;
    public float CellWorldSize => cellWorldSize;   // (solo getter)
    [SerializeField] private Sprite cellSprite;

    [Header("Colors")]
    [SerializeField] private Color walkableColor = Color.white;
    [SerializeField] private Color blockedColor  = Color.black;
    [SerializeField] private Color pathColor     = Color.cyan;

    private Node[,] nodes;
    private Dictionary<Vector2Int, SpriteRenderer> visuals = new();

    public Node StartNode { get; private set; }
    public Node TargetNode { get; private set; }

    void Awake()
    {
        BuildGrid();
        // fissa start / goal alle estremità per il prototipo
        StartNode  = nodes[1, 1];
        TargetNode = nodes[width - 2, height - 2];
        ColorNode(StartNode, Color.green);
        ColorNode(TargetNode, Color.red);
    }

    void BuildGrid()
    {
        nodes = new Node[width, height];

        for (int x = 0; x < width; ++x)
        for (int y = 0; y < height; ++y)
        {
            var pos = new Vector2Int(x, y);
            nodes[x, y] = new Node(pos);

            var go     = new GameObject($"Cell_{x}_{y}");
            go.transform.parent        = transform;
            go.transform.localPosition = (Vector2)pos * cellWorldSize;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite  = cellSprite;
            sr.color   = walkableColor;
            sr.sortingOrder = -1;              // sotto al personaggio
            visuals[pos] = sr;
        }
    }

    // ----- API di utilità -----
    public Node   GetNode(int x,int y)             => nodes[x,y];
    public IEnumerable<Node> AllNodes()            { foreach(var n in nodes) yield return n; }
    public IEnumerable<Node> Neighbours(Node n)    => GetNeighbours(n);

    public void ToggleWalkable(Node n)
    {
        if (n == StartNode || n == TargetNode) return;
        n.Walkable = !n.Walkable;
        ColorNode(n, n.Walkable ? walkableColor : blockedColor);
    }

    public void ShowPath(List<Node> path)
    {
        // reset
        foreach (var n in AllNodes())
            if (n.Walkable && n != StartNode && n != TargetNode)
                ColorNode(n, walkableColor);

        if (path == null) return;
        foreach (var n in path)
            if (n != StartNode && n != TargetNode)
                ColorNode(n, pathColor);
    }

    void ColorNode(Node n, Color c) => visuals[n.GridPos].color = c;
    
    IEnumerable<Node> GetNeighbours(Node n)
    {
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var p = n.GridPos + d;
            if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                yield return nodes[p.x, p.y];
        }
    }
    
    static readonly Vector2Int[] DIR_4 = 
        { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    static readonly Vector2Int[] DIR_8 =
    {
        Vector2Int.up,  Vector2Int.down, Vector2Int.left,  Vector2Int.right,
        Vector2Int.up+Vector2Int.left,    Vector2Int.up+Vector2Int.right,
        Vector2Int.down+Vector2Int.left,  Vector2Int.down+Vector2Int.right
    };

    public IEnumerable<Node> Neighbours(Node n, bool allowDiag = false)
    {
        var dirs = allowDiag ? DIR_8 : DIR_4;

        foreach (var d in dirs)
        {
            var p = n.GridPos + d;
            if (InBounds(p.x, p.y) && nodes[p.x, p.y].Walkable)
                yield return nodes[p.x, p.y];
        }
    }

// utility già usata da JPS
    public bool InBounds(int x,int y) => x>=0 && x<width && y>=0 && y<height;
    public bool IsWalkable(int x,int y) => InBounds(x,y) && nodes[x,y].Walkable;

    /*────────────────────────  WORLD-SPACE UTILITIES ───────────────────────*/

    /// centro della cella che sta a (gx,gy) in coordinate griglia
    public Vector3 CellCenter(int gx, int gy)
    {
        // mezzo passo per arrivare al centro + eventuale offset del GameObject
        return transform.position +
               new Vector3((gx + .5f) * cellWorldSize,
                   (gy + .5f) * cellWorldSize,
                   0f);                 
    }

    /// overload che accetta direttamente il Vector2Int
    public Vector3 CellCenter(Vector2Int gPos) =>
        CellCenter(gPos.x, gPos.y);
    
    public Node WorldToNode(Vector3 worldPos)
    {
        if (nodes == null) return null;       // griglia non ancora creata
        // 1. Trasforma in local-space (origine = GridRoot)
        Vector3 local = worldPos - transform.position;

        // 2. Ricava coordinate di cella intere (floor)
        int gx = Mathf.FloorToInt(local.x / cellWorldSize);
        int gy = Mathf.FloorToInt(local.y / cellWorldSize);

        // 3. Se è dentro i limiti, restituisci il nodo; altrimenti null
        return InBounds(gx, gy) ? nodes[gx, gy] : null;
    }
    
    public Vector2Int ClampToBounds(Vector2 worldPos)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x / cellWorldSize), 0, Width  - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(worldPos.y / cellWorldSize), 0, Height - 1);
        return new Vector2Int(x, y);
    }
    
}

