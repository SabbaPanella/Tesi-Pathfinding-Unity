using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(+10)]      // dopo GridManager (che sta a 0)
[ExecuteAlways]

// questa classe mantiene in scena un Cost Field e ne gestisce la ricostruzione quando cambiamo l'arrivo o il flag delle diagonali
public class CostFieldMono : MonoBehaviour
{
    /*──────────────────── Inspector ───────────────────*/
    [Header("References")]
    [SerializeField] private GridManager grid;      // auto-find se nullo
    [SerializeField] private Transform   goalTf;    // l'arrivoo

    [Header("Pathfinding")]
    [SerializeField] private bool allowDiag = false;  

    /*──────────────────── API pubbliche ──────────────*/
    public Transform GoalTf => goalTf;         // read-only, mi piglio un getter
    public CostField Field   { get; private set; }

    /// Stato corrente del flag (solo lettura pubblico)
    public bool AllowDiag => Field?.allowDiag ?? allowDiag;

    /*──────────────────── LIFECYCLE ──────────────────*/
    void Awake()
    {
        // auto-find
        if (grid == null)
            grid = FindFirstObjectByType<GridManager>();

        if (grid == null)
        {
            Debug.LogError("[CostFieldMono] Nessun GridManager in scena.");
            enabled = false;
            return;
        }

        Field = new CostField(grid);
        Field.allowDiag = allowDiag;    // inizializza
    }

    void Start() => Rebuild();          // flood iniziale

#if UNITY_EDITOR
    /// In Inspector (Play-mode) rilancia sempre il rebake
    void OnValidate()
    {
        if (!Application.isPlaying || Field == null) return;

        // se si è cambiato il flag da Inspector…
        if (Field.allowDiag != allowDiag)
        {
            Field.allowDiag = allowDiag;
            Rebuild();
        }
    }
#endif

    /*──────────────────── PUBLIC API ─────────────────*/
    /// Ricostruisce completamente il Cost-Field
    public void Rebuild()
    {
        if (grid == null || Field == null || goalTf == null) return;

        Node goalNode = grid.WorldToNode(goalTf.position);
        if (goalNode == null)
        {
            Debug.LogWarning("[CostFieldMono] Goal fuori dalla griglia.");
            return;
        }

        Field.Flood(goalNode);
        
        foreach (var ag in FindObjectsByType<RtsAgent>(FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            ag.ReSnapToGrid();          // riallinea tutti gli agenti se si sfasano
        }
    }

    /// Permette a UI / gameplay di cambiare il flag a runtime
    public void SetAllowDiag(bool value)
    {
        if (Field == null) return;
        if (Field.allowDiag == value) return;   // nessuna modifica

        allowDiag        = value;   // x Inspector coerente
        Field.allowDiag  = value;
        Rebuild();                  // rebake immediato
    }

    /// Sposta il Goal su un nodo walkable casuale e ricostruisce
    public void RandomizeGoal()
    {
        if (grid == null) return;

        List<Node> walkables = new();
        foreach (var n in grid.AllNodes())
            if (n.Walkable) walkables.Add(n);

        if (walkables.Count == 0) return;

        Node rnd = walkables[Random.Range(0, walkables.Count)];
        goalTf.position = grid.CellCenter(rnd.GridPos);

        Rebuild();
    }
}