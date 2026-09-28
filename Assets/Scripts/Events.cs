using System;
using UnityEngine;
using UnityEngine.UI;
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
    Gasto_Publico, // G
    Tasa_Impositiva, // t
    Bonos // M
}

public enum OpcionesCambios
{
    Baja,
    Sube,
    Vende,
    Compra
}

public class Events : Singleton<Events>
{
    UI_Screen datosScreen => GameLoadManager.Instance.datosScreen;
    public TMP_Text eventText;
    public Cards[] cards = new Cards[18];
    public Color[] gameColor = new Color[4];
    Color color;
    Outline[] cardOutlines;
    Color[] originalOutlineColor;

    void Start()
    {
        cardOutlines = new Outline[cards.Length];
        originalOutlineColor = new Color[cards.Length];

        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;
            GameObject carta = cards[index].carta;

            cardOutlines[index] = carta.GetComponent<Outline>();
            originalOutlineColor[index] = cardOutlines[index].effectColor;

            carta.GetComponent<Button>().onClick.AddListener(() => {
                GameLoadManager.Instance.SetAppliedEvent(true);
                SetResultsValue(index);
            });

            // EventTrigger captura también los eventos de drag/scroll y bloquea el ScrollRect,
            // por eso se usa un handler que solo escucha PointerDown/PointerUp
            EventTrigger oldTrigger = carta.GetComponent<EventTrigger>();
            if (oldTrigger != null)
                Destroy(oldTrigger);

            CardPressHandler pressHandler = carta.GetComponent<CardPressHandler>();
            if (pressHandler == null)
                pressHandler = carta.AddComponent<CardPressHandler>();

            pressHandler.onPointerDown = () => cardOutlines[index].effectColor = color;
            pressHandler.onPointerUp = () => cardOutlines[index].effectColor = originalOutlineColor[index];
        }
    }

    public void SetCardOutlineColor(int colorIndex)
    {
        color = gameColor[colorIndex];
    }

    void SetResultsValue(int index)
    {
        NotificationsManager.Instance.QuestionNotifications("¿Es la carta que se ha destapado para este año en el tablero central del juego?");
        NotificationsManager.Instance.SetYesButton(() =>
        {
            Cards card = cards[index];
            bool disminuye = card.opcionesCambios == OpcionesCambios.Baja || card.opcionesCambios == OpcionesCambios.Vende;

            EconomicModel.Instance.ApplyEvent(card.opcionesResultados, disminuye ? -card.valor : card.valor);

            UI_System.Instance.SwitchScreens(datosScreen);
            EconomicModel.Instance.NotifyNewValues();
        });
    }
}

public class CardPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action onPointerDown;
    public Action onPointerUp;

    public void OnPointerDown(PointerEventData eventData) => onPointerDown?.Invoke();

    // Unity también envía PointerUp cuando empieza un drag del ScrollRect, así que el color se restablece al hacer scroll
    public void OnPointerUp(PointerEventData eventData) => onPointerUp?.Invoke();
}