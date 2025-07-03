using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;         

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown algorithmDropdown;
    [SerializeField] private Button        startBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private TMP_Text logText;

    void Awake()
    {
        // Costruisco la lista delle opzioni
        var options = new List<string>
        {
            "Grid A*",
            "Jump Point Search",
            "NavMesh A*",
            "HPA*",
            "Flow Field"
        };

        algorithmDropdown.options.Clear();
        algorithmDropdown.AddOptions(options);

        logText.text = "Pathfinding Demo\n\nSabatino Panella";
        
        // wiring pulsanti
        startBtn.onClick.AddListener(StartBenchmark);

        // Collega il Quit-button solo se assegnato
        if (quitBtn != null)
            quitBtn.onClick.AddListener(QuitApp);
    }

    public void StartBenchmark()
    {
        int id = algorithmDropdown.value;
        PlayerPrefs.SetInt("PF_ALGO", id);
        
        string scene = id switch
        {
            0 or 1 => "GridMap",
            2      => "NavMeshMap",
            3      => "Grid_HPA",
            4      => "RTS_Field",
            _      => "GridMap"
        };
        SceneManager.LoadScene(scene);
    }
    
    // quit helper 
    void QuitApp()
    {
#if UNITY_EDITOR
        // Ferma il play se si sta provando da Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Chiude l'eseguibile altrimenti
        Application.Quit();
#endif
    }
}