using System.Collections.Generic;
using UnityEngine;
using TMPro;                     // TMP_Dropdown

/// UI di debug per cambiare a runtime la dimensione dei cluster di HPA*.
[RequireComponent(typeof(TMP_Dropdown))]
public class HpaUi : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GridManager      grid;       
    [SerializeField] TMP_Dropdown     dropdown;   
    [SerializeField] ClusterRenderer  renderer;    // ridisegna clusters

    // taglie ammesse per i cluster
    readonly int[] SIZES = { 2, 4, 6 };

    /*──────────────────────────── LIFECYCLE ───────────────────────────*/
    
    void Awake()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();
    }
    
    void Start()
    {
        InitDropdown();
        RebuildHierarchy(dropdown.value);          // build iniziale
    }

    /*──────────────────────────── PRIVATE ─────────────────────────────*/

    /// Imposta l’UI e aggancia il listener.
    void InitDropdown()
    {
        // ripristina l’ultima size salvata (default 4)
        int saved = PlayerPrefs.GetInt("HPA_SIZE", 4);
        int index = System.Array.IndexOf(SIZES, saved);
        dropdown.SetValueWithoutNotify(index >= 0 ? index : 1);

        // ascolta le variazioni dell’utente
        dropdown.onValueChanged.AddListener(RebuildHierarchy);
    }

    /// Ricostruisce la gerarchia di cluster con la size scelta.
    void RebuildHierarchy(int optionIndex)
    {
        if (grid == null) return;                  // safety-check

        int size = SIZES[Mathf.Clamp(optionIndex, 0, SIZES.Length - 1)];
        PlayerPrefs.SetInt("HPA_SIZE", size);     

        // 1️ - build gerarchia
        Dictionary<Node, Cluster> node2Cl;
        var clusters = HierarchyBuilder.Build(grid, size, out node2Cl);

        // 2️ - passa le strutture ad HPA* 
        HPAStar.SetHierarchy(clusters, node2Cl);

        // 3️ - forza il ridisegno (se presente un ClusterRenderer)
        renderer?.ForceRepaint();
    }
}