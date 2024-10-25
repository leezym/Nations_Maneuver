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
    public GameObject carta;
    public OpcionesResultados opcionesResultados;
    public OpcionesCambios opcionesCambios;
    public double valor;
}

public enum OpcionesResultados
{
    PIB, // y
    Tasa_Inflacion, // inf
    Gasto_Publico, // G
    Tasa_Impositiva // t
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

    void Start()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;
            cards[index].carta.GetComponent<Button>().onClick.AddListener(() => {
                GameLoadManager.Instance.SetAppliedEvent(true);
                SetResultsValue(index);
            });
        }
    }

    public void SetResultsValue(int index)
    {
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
                case OpcionesResultados.Gasto_Publico:
                    EconomicModel.Instance.G += cambio;
                    break;
                case OpcionesResultados.Tasa_Impositiva:
                    EconomicModel.Instance.t += cambio;
                    break;
                    
            }

            UI_System.Instance.SwitchScreens(datosScreen);
            NotificationsManager.Instance.WarningNotifications(
                "Los nuevos valores son:\nPIB: "+EconomicModel.Instance.y.ToString("F2")+
                "\nTasa inflación: "+EconomicModel.Instance.inf.ToString("F2")+
                "\nBalance fiscal: "+EconomicModel.Instance.BF.ToString("F2")+
                "\nGasto público: "+EconomicModel.Instance.G.ToString("F2")+
                "\nTasa impositiva: "+EconomicModel.Instance.t.ToString("F2")
            );
        });
    }
    
}