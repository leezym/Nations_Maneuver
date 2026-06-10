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
    public double limInfGasto;
    public double limSupGasto;
    public double stepGasto; 

    [Header("TASA IMPOSITIVA")]
    public double limInfTasaImp;
    public double limSupTasaImp;
    public double stepTasaImp;

    [Header("BONOS")]
    public double limInfOma;
    public double limSupOma;
    public double stepOma;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        textGasto.onEndEdit.AddListener(_ => ValidateGasto());
        textTasaImp.onEndEdit.AddListener(_ => ValidateTasaImp());
        textOma.onEndEdit.AddListener(_ => ValidateOma());
    }

    public void OnInputValueChanged_Gasto() { }
    public void OnInputValueChanged_TasaImp() { }
    public void OnInputValueChanged_Oma() { }

    private void ValidateGasto()
    {
        if (double.TryParse(textGasto.text, out double inputValue))
        {
            inputValue = Math.Clamp(inputValue, limInfGasto, limSupGasto);
            inputValue = Math.Round((inputValue - limInfGasto) / stepGasto) * stepGasto + limInfGasto;
            textGasto.text = inputValue.ToString();
        }
        else
        {
            textGasto.text = limInfGasto.ToString();
        }
    }

    private void ValidateTasaImp()
    {
        if (double.TryParse(textTasaImp.text, out double inputValue))
        {
            inputValue = Math.Clamp(inputValue, limInfTasaImp, limSupTasaImp);
            inputValue = Math.Round(inputValue, 2);
            textTasaImp.text = inputValue.ToString("F2");
        }
        else
        {
            textTasaImp.text = limInfTasaImp.ToString("F2");
        }
    }

    private void ValidateOma()
    {
        if (double.TryParse(textOma.text, out double inputValue))
        {
            inputValue = Math.Clamp(inputValue, limInfOma, limSupOma);
            inputValue = Math.Round((inputValue - limInfOma) / stepOma) * stepOma + limInfOma;
            textOma.text = inputValue.ToString();
        }
        else
        {
            textOma.text = limInfOma.ToString();
        }
    }
}
