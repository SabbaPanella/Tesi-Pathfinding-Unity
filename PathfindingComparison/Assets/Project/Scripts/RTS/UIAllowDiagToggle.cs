using UnityEngine;
using UnityEngine.UI;

/// Collega un Toggle UI al flag "Allow Diag" di CostFieldMono.
/// - all'avvio imposta il Toggle sul valore corrente
/// - onValueChanged -> aggiorna il CostField e rifà il flood
[RequireComponent(typeof(Toggle))]
public class UIAllowDiagToggle : MonoBehaviour
{
    Toggle         toggle;
    CostFieldMono  cfm;          // riferimento alla scena

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        cfm    = FindFirstObjectByType<CostFieldMono>();

        if (cfm == null)
        {
            Debug.LogError("[UIAllowDiagToggle] Nessun CostFieldMono trovato in scena.");
            enabled = false;
            return;
        }

        // 1- inizializza lo stato visivo
        toggle.isOn = cfm.AllowDiag;

        // 2-  ascolta l’evento UI
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        cfm.SetAllowDiag(isOn);          // propaga + rebake
    }
}