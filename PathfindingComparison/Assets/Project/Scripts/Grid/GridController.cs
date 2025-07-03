using System.Collections.Generic;
using System.Diagnostics;                
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;                    
using Debug = UnityEngine.Debug;

public class GridController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GridManager grid;
    [SerializeField] private AgentMover  agent;

    [Header("HUD")]
    [SerializeField] private TMP_Text algoLabel;
    [SerializeField] private TMP_Text timeLabel;
    [SerializeField] private Toggle    diagToggle;

    // ——————————————————————————————

    void Start()
    {
        // 1️ – nome algoritmo scelto nel MainMenu
        int id = PlayerPrefs.GetInt("PF_ALGO", 0);
        algoLabel.text = id switch
        {
            0 => "Grid A*",
            1 => "Jump Point Search",
            2 => "NavMesh A*",
            3 => "Hierarchical A*",
            4 => "Flow Field",
            5 => "LPA*",
            _ => "Pathfinder"
        };
        
        if (timeLabel != null) timeLabel.text = "";

        // warm-up solo se grid esiste
        if (grid != null)
        {
            AStarGrid.FindPath(grid, grid.StartNode, grid.StartNode, diagToggle != null && diagToggle.isOn);
            JumpPointSearch.FindPath(grid, grid.StartNode, grid.StartNode, diagToggle != null && diagToggle.isOn);
        }
    }

    void Update()
    {
        // Toggle muro
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var n = NodeUnderMouse();
            if (n != null) grid.ToggleWalkable(n);
        }

        // Calcola percorso
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            var sw = Stopwatch.StartNew();

            List<Node> path = null;
            switch (PlayerPrefs.GetInt("PF_ALGO", 0))
            {
                case 0: // A*
                    path = AStarGrid.FindPath(grid, grid.StartNode, grid.TargetNode,
                        diagToggle.isOn);
                    break;
                case 1: // JPS
                    if (diagToggle.isOn)
                        path = JumpPointSearch.FindPath(grid, grid.StartNode, grid.TargetNode, true);
                    else
                        path = AStarGrid.FindPath(grid, grid.StartNode, grid.TargetNode, false);
                    break;
                case 3: // Hierarchical
                    path = HPAStar.FindPath(grid, grid.StartNode, grid.TargetNode);
                    if (path == null)
                    {
                        Debug.LogWarning("HPA* => path NULL");
                        return;
                    }
                    break;
                // altri casi li metterò qua!!!
            }

            sw.Stop();
            
            // tempo in micro-secondi
            double us = sw.ElapsedTicks * 1_000_000.0 / Stopwatch.Frequency;   
            timeLabel.text = $"t = {us:F1} µs";                                

            grid.ShowPath(path);
            if (path != null) agent.FollowPath(path, 1f);
        }
    }

    // ——————————————————————————— utilità ———————————————————————————
    Node NodeUnderMouse()
    {
        Vector3 wpos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        var p = Vector2Int.RoundToInt(wpos);
        return grid.InBounds(p.x, p.y) ? grid.GetNode(p.x, p.y) : null;
    }
    
    public void BackToMenu() => SceneManager.LoadScene("MainMenu");
}