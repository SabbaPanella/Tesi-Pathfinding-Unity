#define USE_TMP          // togli o commenta se utilizzi UI.Text normale

using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

#if USE_TMP
using TMPro;              // TextMeshProUGUI
#else
using UnityEngine.UI;     // Text
#endif

/// Lancia un benchmark (A* vs Flood-Fill) e, su richiesta, rimpiazza
/// tutte le unità (“Restart”).  
/// Barra spaziatrice = sposta il goal e riesegue il test.
public class RtsBenchmark : MonoBehaviour
{
    /*──────────────────  REFERENZE DI SCENA  ──────────────────*/
    [Header("Scene References")]
    [SerializeField] GridManager   grid;
    [SerializeField] CostFieldMono costMono;
    [SerializeField] GameObject    agentPrefab;      // prefab della Unit

#if USE_TMP
    [SerializeField] TextMeshProUGUI timeLabel;
#else
    [SerializeField] Text timeLabel;
#endif

    /*──────────────────  PARAMETRI BENCH  ──────────────────*/
    [Header("Benchmark settings")]
    [Min(1)] public int  agents    = 100;
    public  bool allowDiag         = false;

    /*──────────────────  RUNTIME  ──────────────────*/
    readonly List<GameObject> spawned = new();   // unità correntemente vive

    /*──────────────────  LIFECYCLE  ──────────────────*/
    void Awake()
    {
#if UNITY_2022_2_OR_NEWER
        grid     ??= FindFirstObjectByType<GridManager>();
        costMono ??= FindFirstObjectByType<CostFieldMono>();
#else
        grid     ??= FindObjectOfType<GridManager>();
        costMono ??= FindObjectOfType<CostFieldMono>();
#endif

        // se non assegnata prova a trovarla nella scena
        if (timeLabel == null)
        {
            var go = GameObject.Find("TimeLabel");
#if USE_TMP
            timeLabel = go ? go.GetComponent<TextMeshProUGUI>() : null;
#else
            timeLabel = go ? go.GetComponent<Text>()              : null;
#endif
        }
    }

    void Start()
    {
        SpawnAgents();     // prima generazione
        RunBenchmark();    // primo test
    }

    void Update()
    {
        // con barra spaziatrice = sposta il goal & ricostruisci campo + test
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            costMono.RandomizeGoal();
            RunBenchmark();
        }
    }

    /*──────────────────  UI HOOK  ──────────────────*/
    /// Richiamato dal pulsante “Restart” (OnClick).
    public void RestartAgents()
    {
        // 1- distruggi unità esistenti
        foreach (var go in spawned)
            if (go) Destroy(go);
        spawned.Clear();

        // 2- ricrea + nuovo benchmark
        SpawnAgents();
        RunBenchmark();
    }

    /*──────────────────  SPAWN  ──────────────────*/
    void SpawnAgents()
    {
        if (agentPrefab == null)
        {
            Debug.LogError("[RtsBenchmark] Assegna la prefab dell’agente!");
            return;
        }

        Node goal = grid.WorldToNode(costMono.GoalTf.position);
        List<Node> starts = PickRandomStarts(goal);

        foreach (Node n in starts)
        {
            Vector3 pos = grid.CellCenter(n.GridPos);
            var go      = Instantiate(agentPrefab, pos, Quaternion.identity, transform);
            spawned.Add(go);
        }
    }

    /*──────────────────  BENCHMARK  ──────────────────*/
    void RunBenchmark()
    {
        // controlli
        if (grid == null || costMono == null || costMono.GoalTf == null)
        {
            Debug.LogError("[RtsBenchmark] riferimenti mancanti!");
            return;
        }

        // sincronizza il flag sulle diagonali
        costMono.SetAllowDiag(allowDiag);

        Node goalNode = grid.WorldToNode(costMono.GoalTf.position);
        if (goalNode == null)
        {
            Debug.LogError("Goal fuori griglia!");
            return;
        }

        // prepara lista di start casuali (usata sia per A* sia per spawn)
        List<Node> starts = PickRandomStarts(goalNode);

        //A* per tutti gli agenti
        long tA = MeasureMs(() =>
        {
            foreach (var s in starts)
                AStarGrid.FindPath(grid, s, goalNode, allowDiag);
        });

        //Flood-Fill singola passata
        long tF = MeasureMs(() => costMono.Field.Flood(goalNode));

        // Report finale
        float perAgent = tA / (float)agents;
        float ratio    = (tF > 0) ? (tA / (float)tF) : float.PositiveInfinity;

        string report =
            $"<b>[Benchmark]</b> agents:{agents}\n" +
            $"A*   : {tA} ms  (≈ {perAgent:F3} ms/agent)\n" +
            $"Flow : {tF} ms\n" +
            $"Speed-up Flow / (A*×{agents}) = {ratio:F1}×";

        Debug.Log(report);

        if (timeLabel)
        {
#if USE_TMP
            timeLabel.text = report.Replace("<b>", "").Replace("</b>", "");
#else
            timeLabel.text = report;
#endif
        }
    }

    /*──────────────────  UTILS  ──────────────────*/
    static long MeasureMs(System.Action act)
    {
        var sw = Stopwatch.StartNew();
        act?.Invoke();
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    List<Node> PickRandomStarts(Node goal)
    {
        var walkables = new List<Node>();
        foreach (var n in grid.AllNodes())
            if (n.Walkable && n != goal) walkables.Add(n);

        int take = Mathf.Min(agents, walkables.Count);
        var res  = new List<Node>(take);

        while (res.Count < take)
        {
            Node rnd = walkables[Random.Range(0, walkables.Count)];
            if (!res.Contains(rnd))
                res.Add(rnd);
        }
        return res;
    }
}