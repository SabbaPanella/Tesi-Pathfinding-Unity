using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.AI;                         
using Debug = UnityEngine.Debug;   

public class NavMeshController : MonoBehaviour
{
    // sfere di start e goal
    [Header("Scene markers")]
    [SerializeField] private Transform startMarker;      
    [SerializeField] private Transform targetMarker;

    [Header("Refs")]
    [SerializeField] private AgentMoverNavMesh agent;
    [SerializeField] private PathLine           line;

    [Header("HUD")]
    [SerializeField] private TMP_Text timeLabel;

    Camera cam;

    // --------------------------------------------------------------------

    void Awake()
    {
        cam = Camera.main;

        // metto l’agente sullo start all’avvio
        if (startMarker != null)
            agent.transform.position = startMarker.position;
    }

    void Update()
    {
        // Ricomputa percorso se barra spaziatrice
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            ComputeAndRun();
    }

    void ComputeAndRun()
    {
        if (startMarker == null || targetMarker == null) return;

        // 1️- proietto i due punti sulla mesh
        NavMeshHit hitStart, hitGoal;
        if (!NavMesh.SamplePosition(startMarker.position, out hitStart, 1f, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(targetMarker.position, out hitGoal, 1f, NavMesh.AllAreas))
        {
            UnityEngine.Debug.LogWarning("Start o Goal fuori della NavMesh");
            return;
        }

        // 2️- calcolo il path via Unity
        var navPath    = new NavMeshPath();
        var sw         = Stopwatch.StartNew();
        bool hasPath   = NavMesh.CalculatePath(hitStart.position, hitGoal.position,
            NavMesh.AllAreas, navPath);
        sw.Stop();
        
        // solito tempo in micro-secondi
        double us = sw.ElapsedTicks * 1_000_000.0 / Stopwatch.Frequency; 
        timeLabel.text = $"t = {us:F1} µs";     

        if (!hasPath || navPath.status != NavMeshPathStatus.PathComplete)
        {
            UnityEngine.Debug.Log("NO PATH con CalculatePath");
            line.Show(null);
            return;
        }

        // 3- conversione di corner in List<Vector3> per il line renderer
        List<Vector3> corners = new List<Vector3>(navPath.corners);

        line.Show(corners);
        agent.FollowPath(corners);
    }
    
    void ResetAgent()
    {
        if (startMarker == null) return;

        agent.WarpTo(startMarker.position); 

        line.Show(null);
        timeLabel.text = "";
    }
    
    public void BackToMenu() => SceneManager.LoadScene("MainMenu");
    
    public void OnRestartBtn() => ResetAgent();
}