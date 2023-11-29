using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputValidations : MonoBehaviour
{
    TMP_InputField textGasto => EconomicModel.Instance.textGasto;
    TMP_InputField textTasaImp => EconomicModel.Instance.textTasaImp;
    TMP_InputField textOma => EconomicModel.Instance.textOma;

    public void OnInputValueChanged_Gasto()
    {
        int inputValue;

        if (int.TryParse(textGasto.text, out inputValue))
        {
            inputValue = Mathf.Clamp(inputValue, 0, 1000);
            inputValue = Mathf.RoundToInt(inputValue / 50.0f) * 50;

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
            inputValue = Mathf.Clamp(inputValue, 0, 1);
            inputValue = Mathf.RoundToInt(inputValue / 0.1f) * 0.1f;

            textTasaImp.text = inputValue.ToString();
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
            inputValue = Mathf.Clamp(inputValue, 0, 10000);
            inputValue = Mathf.RoundToInt(inputValue / 500.0f) * 500;

            textOma.text = inputValue.ToString();
        }
        else
        {
            Debug.LogWarning("Input no válido");
        }
    }
}
