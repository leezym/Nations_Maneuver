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
    public static Events Instance {get; private set;}
    UI_Screen datosScreen => GameLoadManager.Instance.datosScreen;
    public TMP_Text eventText;
    public Cards[] cards = new Cards[18];

    void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;    
    }

    public void SetResultsValue()
    {
        int index = 0;
        //Como cada carta va en un areglo, al ser cliqueada identificar a que posición del arreglo pertenece esa carta para asignarle ese valor a index
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
            NotificationsManager.Instance.WarningNotifications("Los nuevos valores son:\nVariación PIB: "+EconomicModel.Instance.y+
                "\nTasa infleación: "+EconomicModel.Instance.inf+"\nBalance fiscal: "+EconomicModel.Instance.BF);
        });
    }
    
}