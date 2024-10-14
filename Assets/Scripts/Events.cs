using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.EventSystems;

[Serializable]
public class Cards
{
    public Image imagen;
    public OpcionesResultados opcionesResultados;
    public OpcionesCambios opcionesCambios;
    public double valor;
}

public enum OpcionesResultados
{
    PIB, // y
    Tasa_Inflacion, // inf
    Balance_Fiscal // BF
}

public enum OpcionesCambios
{
    Baja,
    Sube
}

public class Events : MonoBehaviour
{
    UI_Screen datosScreen => GameLoadManager.Instance.datosScreen;
    public Cards[] cards = new Cards[18];

    public void SetResultsValue(int index)
    {
        NotificationsManager.Instance.QuestionNotifications("¿Es la carta que se ha destapado para este año en el tablero central del juego?");
        NotificationsManager.Instance.SetYesButton(() =>
        {
            double cambio = (cards[index].opcionesCambios == OpcionesCambios.Baja) ? -cards[index].valor : cards[index].valor;

            switch (cards[index].opcionesResultados)
            {
                case OpcionesResultados.PIB:
                    EconomicModel.Instance.y += cambio;
                    break;
                case OpcionesResultados.Tasa_Inflacion:
                    EconomicModel.Instance.inf += cambio;
                    break;
                case OpcionesResultados.Balance_Fiscal:
                    EconomicModel.Instance.BF += cambio;
                    break;
            }

            UI_System.Instance.SwitchScreens(datosScreen);
        });
    }
    
}