using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class GameLoadManager : MonoBehaviour
{
    public static GameLoadManager Instance {get; private set;}

    public static int SHIFTS = 10;

    int shift => EconomicModel.Instance.GetShift();
    TMP_Text shiftText => EconomicModel.Instance.shiftText;
    TMP_Text shiftText2 => EconomicModel.Instance.shiftText2;
    TMP_Text eventText => Events.Instance.eventText;
    Button continuarPartidaButton => EconomicModel.Instance.continuarPartidaButton;

    [Header("UI Screen")]
    public UI_Screen eventosScreen;
    public UI_Screen datosScreen;

    [Header("UI Players")]
    public Button greenPlayerButton;
    public Button bluePlayerButton;
    public Button orangePlayerButton;
    public Button redPlayerButton;

    bool finishedGame;
    public bool GetFinishedGame() { return finishedGame; }
    public void SetFinishedGame(bool finishedGame) { this.finishedGame = finishedGame; }

    bool appliedEvent;
    public bool GetAppliedEvent() { return appliedEvent; }
    public void SetAppliedEvent(bool appliedEvent) { this.appliedEvent = appliedEvent; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;    
    }

    void Start()
    {
        //PlayerPrefs.DeleteAll();
        if (PlayerPrefs.HasKey("player"))
        {
            switch(PlayerPrefs.GetString("player"))
            {
                case "green":
                    greenPlayerButton.onClick.Invoke();
                break;

                case "blue":
                    bluePlayerButton.onClick.Invoke();
                break;

                case "orange":
                    orangePlayerButton.onClick.Invoke();
                break;

                case "red":
                    redPlayerButton.onClick.Invoke();
                break;
            }
        }
        else
        {
            greenPlayerButton.onClick.Invoke();
            PlayerPrefs.SetString("player", "green");
        }
        
        // TURNOS
        if (PlayerPrefs.HasKey("shift"))
            EconomicModel.Instance.SetShift(PlayerPrefs.GetInt("shift"));

        if (shift == 0)
            continuarPartidaButton.interactable = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ExitApp();
    }

    public void NewGame()
    {        
        EconomicModel.Instance.NewGame();
        SetFinishedGame(false);
        UpdateShift();
    }

    public void ContinueGame()
    {
        WindowGraph.Instance.ContinueGame();
        EconomicModel.Instance.ContinueGame();
        UpdateShift();
        SetFinishedGame(PlayerPrefs.GetInt("finishedGame") == 1 ? true : false);
        SetAppliedEvent(PlayerPrefs.GetInt("appliedEvent") == 1 ? true : false);

        if(GetAppliedEvent())
            UI_System.Instance.SwitchScreens(datosScreen);
        else
            UI_System.Instance.SwitchScreens(eventosScreen);        
    }

    public void NextShift()
    {
        NotificationsManager.Instance.QuestionNotifications("¿Quieres pasar al siguiente turno?");
        NotificationsManager.Instance.SetYesButton(()=>{
            UI_System.Instance.SwitchScreens(eventosScreen);
            shiftText.text = "Turno " + shift.ToString();
            eventText.text = "Fase de Eventos\nAño " + shift.ToString();
            shiftText2.text = "Indicadores de tu Estado\nTurno " + shift.ToString();
        });
    }

    public void UpdateShift()
    {
        shiftText.text = "Año " + shift.ToString();
        eventText.text = "Fase de Eventos\nAño " + shift.ToString();
        shiftText2.text = "Indicadores de tu Estado\nAño " + shift.ToString();
    }
    
    public void ExitApp()
    {
        NotificationsManager.Instance.QuestionNotifications("¿Esta seguro que quiere acabar la partida antes de terminar los 10 turnos?");
        NotificationsManager.Instance.SetYesButton(()=> {
            PlayerPrefs.SetInt("finishedGame", finishedGame ? 1 : 0);
            PlayerPrefs.SetInt("appliedEvent", appliedEvent ? 1 : 0);

            EconomicModel.Instance.SaveLocalData();
            WindowGraph.Instance.SaveLocalData();

            PlayerPrefs.Save();
            Application.Quit();
        });
    }

    void OnApplicationQuit()
    {
        ExitApp();
    }
}
