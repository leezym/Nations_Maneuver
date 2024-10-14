using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputValidations : MonoBehaviour
{
    public static InputValidations Instance {get; private set;}
    TMP_InputField textGasto => EconomicModel.Instance.textGasto;
    TMP_InputField textTasaImp => EconomicModel.Instance.textTasaImp;
    TMP_InputField textOma => EconomicModel.Instance.textOma;

    [Header("GASTO")]
    public int limInfGasto = 50;
    public int limSupGasto = 500;
    public int stepGasto = 50; 

    [Header("TASA IMPOSITIVA")]
    public float limInfTasaImp = 0.05f;
    public float limSupTasaImp = 0.25f;
    public float stepTasaImp = 0.02f;

    [Header("BONOS")]
    public int limInfOma = 4000;
    public int limSupOma = 5500;
    public int stepOma = 150;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;    
    }

    public void OnInputValueChanged_Gasto()
    {
        int inputValue;

        if (int.TryParse(textGasto.text, out inputValue))
        {
            inputValue = Mathf.Clamp(inputValue, limInfGasto, limSupGasto);
            inputValue = Mathf.RoundToInt((inputValue - limInfGasto) / stepGasto) * stepGasto + limInfGasto;

            textGasto.text = inputValue.ToString();
        }
        else
        {
            Debug.LogWarning("Input no válido");
        }
    }

    public void OnInputValueChanged_TasaImp()
    {
        float inputValue;

        if (float.TryParse(textTasaImp.text, out inputValue))
        {
            inputValue = Mathf.Clamp(inputValue, limInfTasaImp, limSupTasaImp);

            inputValue = Mathf.Round((inputValue - limInfTasaImp) / stepTasaImp) * stepTasaImp + limInfTasaImp;

            textTasaImp.text = inputValue.ToString("F2");
        }
        else
        {
            Debug.LogWarning("Input no válido");
        }
    }

    public void OnInputValueChanged_Oma()
    {
        int inputValue;

        if (int.TryParse(textOma.text, out inputValue))
        {
            inputValue = Mathf.Clamp(inputValue, limInfOma, limSupOma);

            inputValue = Mathf.RoundToInt((inputValue - limInfOma) / stepOma) * stepOma + limInfOma;

            textOma.text = inputValue.ToString();
        }
        else
        {
            Debug.LogWarning("Input no válido");
        }
    }
}
