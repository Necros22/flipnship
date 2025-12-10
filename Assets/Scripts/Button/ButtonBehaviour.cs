using UnityEngine;
using System.Reflection;

public class ButtonBehaviour : MonoBehaviour
{
    public Light buttonPointLight;

    public GameObject targetObject;
    public string targetScriptName;
    public string targetFunctionName;

    public bool callExtraFunctions = false;

    [System.Serializable]
    public class ExtraFunctionCall
    {
        public GameObject extraTargetObject;
        public string extraTargetScriptName;
        public string extraTargetFunctionName;
    }

    public ExtraFunctionCall[] extraFunctions;

    public string activatorTag = "GravityAffected";
    public bool staysOnOnceActivated = false;

    private int activatorCount = 0;
    public bool isActive = false;

    public bool isMultiButton = false;
    public bool isMainButton = false;
    public ButtonBehaviour[] requiredButtons;
    public ButtonBehaviour mainButtonReference;

    private AudioSource buttonSFX;


    private void Start()
    {
        UpdateLightColor();
        GameObject sfxObj = GameObject.Find("ButtonSFX");
        if (sfxObj != null)
            buttonSFX = sfxObj.GetComponent<AudioSource>();
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

    private void ActivateButton()
    {
        isActive = true;
        UpdateLightColor();

        if (buttonSFX != null)
            buttonSFX.Play();

        if (!isMultiButton)
        {
            CallTargetFunction();
            CallExtraFunctionsIfEnabled();
            return;
        }

        if (isMainButton)
        {
            CheckAllButtons();
            return;
        }

        if (isMultiButton && !isMainButton)
            CallExtraFunctionsIfEnabled();
    }

    private void DeactivateButton()
    {
        isActive = false;
        UpdateLightColor();

        if (isMultiButton && isMainButton)
            CheckAllButtons();
    }

    private void UpdateLightColor()
    {
        if (buttonPointLight == null) return;
        buttonPointLight.color = isActive ? Color.green : Color.red;
    }

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
        CallExtraFunctionsIfEnabled();
    }

    private void CallTargetFunction()
    {
        InvokeFunction(targetObject, targetScriptName, targetFunctionName);
    }

    private void InvokeFunction(GameObject obj, string scriptName, string functionName)
    {
        if (obj == null || string.IsNullOrWhiteSpace(scriptName) || string.IsNullOrWhiteSpace(functionName))
            return;

        Component script = obj.GetComponent(scriptName);
        if (script == null)
            return;

        MethodInfo method = script.GetType().GetMethod(functionName);
        if (method == null)
            return;

        method.Invoke(script, null);
    }

    private void CallExtraFunctionsIfEnabled()
    {
        if (!callExtraFunctions) return;
        if (extraFunctions == null) return;

        foreach (var extra in extraFunctions)
            InvokeFunction(extra.extraTargetObject, extra.extraTargetScriptName, extra.extraTargetFunctionName);
    }

    public void ForceResetButton()
    {
        if (!staysOnOnceActivated)
            return;

        if (!isActive)
            return;

        activatorCount = 0;
        isActive = false;
        UpdateLightColor();
    }
}
