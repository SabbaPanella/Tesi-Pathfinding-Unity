using UnityEngine;

public class Node
{
    public Vector2Int GridPos;
    public bool       Walkable = true;

    // --- A*  -----------------------------
    
    public int  gCost = int.MaxValue;
    public int  hCost = 0;
    public int  FCost => gCost + hCost;
    public Node CameFrom;

    // --- per JPS ----------------------------------------
    // posizione scomposta
    public int x  => GridPos.x;
    public int y  => GridPos.y;

    // g / h / f con stessi valori dei costi 
    public int g
    {
        get => gCost;
        set => gCost = value;
    }

    public int h
    {
        get => hCost;
        set => hCost = value;
    }

    public int f                        
    {
        get => FCost;
        set { /* ignorato */ }
    }

    public Node parent
    {
        get => CameFrom;
        set => CameFrom = value;
    }

    // camminabilità
    public bool walkable => Walkable;

    // -----------------------------------------------------------
    public Node(Vector2Int gridPos) => GridPos = gridPos;
}