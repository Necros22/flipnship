using UnityEngine;
using System.Reflection;

public class ButtonBehaviour : MonoBehaviour
{
    // ------------------------------------------------------------
    //                       LIGHT REFERENCE
    // ------------------------------------------------------------
    [Header("Luz del Botón (Point Light obligatorio)")]
    public Light buttonPointLight;


    // ------------------------------------------------------------
    //                 TARGET FUNCTION SETTINGS
    // ------------------------------------------------------------
    [Header("Función a Ejecutar (Solo botones normales o principal)")]
    public GameObject targetObject;

    [Tooltip("Nombre del script dentro del targetObject")]
    public string targetScriptName;

    [Tooltip("Nombre de la función que se llamará")]
    public string targetFunctionName;


    // ------------------------------------------------------------
    //                    BASIC BUTTON SETTINGS
    // ------------------------------------------------------------
    [Header("Configuración del Botón")]
    [Tooltip("Tag de objetos que pueden activar este botón")]
    public string activatorTag = "GravityAffected";

    [Tooltip("Si es true, el botón NO se apaga al salir el objeto")]
    public bool staysOnOnceActivated = false;

    private int activatorCount = 0;
    private bool isActive = false;


    // ------------------------------------------------------------
    //                    MULTI BUTTON SYSTEM
    // ------------------------------------------------------------
    [Header("Sistema de Múltiples Botones")]
    public bool isMultiButton = false;

    [Tooltip("Marcar solo si este botón es el PRINCIPAL del sistema")]
    public bool isMainButton = false;

    [Tooltip("Botones requeridos (solo para el botón principal)")]
    public ButtonBehaviour[] requiredButtons;

    [Tooltip("Referencia al botón principal (solo en botones esclavos)")]
    public ButtonBehaviour mainButtonReference;


    // ------------------------------------------------------------
    //                        UNITY METHODS
    // ------------------------------------------------------------
    private void Start()
    {
        UpdateLightColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(activatorTag))
            return;

        activatorCount++;

        if (activatorCount == 1)
            ActivateButton();

        if (isMultiButton && !isMainButton)
            mainButtonReference?.SlaveButtonActivated();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(activatorTag))
            return;

        activatorCount = Mathf.Max(activatorCount - 1, 0);

        if (!staysOnOnceActivated && activatorCount == 0)
            DeactivateButton();

        if (isMultiButton && !isMainButton)
            mainButtonReference?.SlaveButtonDeactivated();
    }


    // ------------------------------------------------------------
    //                  BUTTON STATE HANDLING
    // ------------------------------------------------------------
    private void ActivateButton()
    {
        isActive = true;
        UpdateLightColor();

        // BOTÓN NORMAL
        if (!isMultiButton)
        {
            CallTargetFunction();
            return;
        }

        // BOTÓN PRINCIPAL
        if (isMainButton)
        {
            CheckAllButtons();
            return;
        }
    }

    private void DeactivateButton()
    {
        isActive = false;
        UpdateLightColor();

        if (isMultiButton && isMainButton)
            CheckAllButtons();
    }


    // ------------------------------------------------------------
    //                     UPDATE LIGHT STATE
    // ------------------------------------------------------------
    private void UpdateLightColor()
    {
        if (buttonPointLight == null) return;

        buttonPointLight.color = isActive ? Color.green : Color.red;
    }


    // ------------------------------------------------------------
    //                     MULTI BUTTON — SLAVE CALLS
    // ------------------------------------------------------------
    public void SlaveButtonActivated()
    {
        if (isMainButton)
            CheckAllButtons();
    }

    public void SlaveButtonDeactivated()
    {
        if (isMainButton && !staysOnOnceActivated)
            CheckAllButtons();
    }


    // ------------------------------------------------------------
    //                    MULTI BUTTON — MAIN LOGIC
    // ------------------------------------------------------------
    private void CheckAllButtons()
    {
        if (!isMainButton)
            return;

        if (!isActive)
            return;

        foreach (var b in requiredButtons)
        {
            if (!b.isActive)
                return;
        }

        CallTargetFunction();
    }


    // ------------------------------------------------------------
    //                     CALL TARGET FUNCTION
    // ------------------------------------------------------------
    private void CallTargetFunction()
    {
        if (targetObject == null ||
            string.IsNullOrWhiteSpace(targetScriptName) ||
            string.IsNullOrWhiteSpace(targetFunctionName))
        {
            Debug.LogWarning("ButtonBehaviour: Falta configurar la función objetivo.");
            return;
        }

        Component script = targetObject.GetComponent(targetScriptName);
        if (script == null)
        {
            Debug.LogWarning("Script no encontrado: " + targetScriptName);
            return;
        }

        MethodInfo method = script.GetType().GetMethod(targetFunctionName);
        if (method == null)
        {
            Debug.LogWarning("Método no encontrado: " + targetFunctionName);
            return;
        }

        method.Invoke(script, null);
    }

    // ------------------------------------------------------------
    //                     EXTERNAL RESET FUNCTION
    // ------------------------------------------------------------
    public void ForceResetButton()
    {
        // Solo funciona si el botón usa "permanecer encendido"
        if (!staysOnOnceActivated)
        {
            Debug.Log("ForceResetButton: Este botón no está configurado para permanecer encendido. No se reinicia.");
            return;
        }

        // Solo funciona si el botón está actualmente activo
        if (!isActive)
        {
            Debug.Log("ForceResetButton: El botón ya está apagado. Nada que reiniciar.");
            return;
        }

        // Resetear el botón
        activatorCount = 0;
        isActive = false;
        UpdateLightColor();

        Debug.Log("ForceResetButton: Botón reiniciado manualmente.");
    }

}
