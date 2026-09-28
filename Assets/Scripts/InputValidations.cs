using System;
using UnityEngine;
using TMPro;

public class InputValidations : Singleton<InputValidations>
{
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

    private void Start()
    {
        textGasto.onEndEdit.AddListener(_ => ValidateStepped(textGasto, limInfGasto, limSupGasto, stepGasto));
        textTasaImp.onEndEdit.AddListener(_ => ValidateTasaImp());
        textOma.onEndEdit.AddListener(_ => ValidateStepped(textOma, limInfOma, limSupOma, stepOma));
    }

    // Referenciados desde los eventos de la escena
    public void OnInputValueChanged_Gasto() { }
    public void OnInputValueChanged_TasaImp() { }
    public void OnInputValueChanged_Oma() { }

    // Acota el valor a [limInf, limSup] y lo ajusta al múltiplo de step más cercano (desde limInf)
    private static void ValidateStepped(TMP_InputField field, double limInf, double limSup, double step)
    {
        if (double.TryParse(field.text, out double inputValue))
        {
            inputValue = Math.Clamp(inputValue, limInf, limSup);
            inputValue = Math.Round((inputValue - limInf) / step) * step + limInf;
            field.text = inputValue.ToString();
        }
        else
        {
            field.text = limInf.ToString();
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
}
