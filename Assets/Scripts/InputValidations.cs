using System;
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
    public double limInfGasto = 50;
    public double limSupGasto = 1000;
    public double stepGasto = 50; 

    [Header("TASA IMPOSITIVA")]
    public int limInfTasaImp = 0;
    public int limSupTasaImp = 1;
    public double stepTasaImp = 0.1;

    [Header("BONOS")]
    public double limInfOma = 5;
    public double limSupOma = 40;
    public double stepOma = 5;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;    
    }

    public void OnInputValueChanged_Gasto()
    {
        double inputValue;

        if (double.TryParse(textGasto.text, out inputValue))
        {
            inputValue = Math.Clamp(inputValue, limInfGasto, limSupGasto);
            inputValue = Math.Round((inputValue - limInfGasto) / stepGasto) * stepGasto + limInfGasto;

            textGasto.text = inputValue.ToString();
        }
        else
        {
            Debug.LogWarning("Input no válido");
        }
    }

    public void OnInputValueChanged_TasaImp()
    {
        double inputValue;

        if (double.TryParse(textTasaImp.text, out inputValue))
        {
            inputValue = Math.Clamp(inputValue, limInfTasaImp, limSupTasaImp);

            inputValue = Math.Round((inputValue - limInfTasaImp) / stepTasaImp) * stepTasaImp + limInfTasaImp;

            textTasaImp.text = inputValue.ToString("F2");
        }
        else
        {
            Debug.LogWarning("Input no válido");
        }
    }

    public void OnInputValueChanged_Oma()
    {
        double inputValue;

        if (double.TryParse(textOma.text, out inputValue))
        {
            inputValue = Math.Clamp(inputValue, limInfOma, limSupOma);

            inputValue = Math.Round((inputValue - limInfOma) / stepOma) * stepOma + limInfOma;

            textOma.text = inputValue.ToString();
        }
        else
        {
            Debug.LogWarning("Input no válido");
        }
    }
}
