using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameLoadManager : Singleton<GameLoadManager>
{
    public static int SHIFTS = 10;

    // Orden = índice de color en Events.gameColor
    static readonly string[] PLAYERS = { "green", "blue", "orange", "red" };

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

    void Start()
    {
        //PlayerPrefs.DeleteAll();
        if (PlayerPrefs.HasKey("player"))
        {
            SelectPlayer(Array.IndexOf(PLAYERS, PlayerPrefs.GetString("player")));
        }
        else
        {
            SelectPlayer(0);
            PlayerPrefs.SetString("player", PLAYERS[0]);
        }

        // TURNOS
        if (PlayerPrefs.HasKey("shift"))
            EconomicModel.Instance.SetShift(PlayerPrefs.GetInt("shift"));

        if (shift == 0)
            continuarPartidaButton.interactable = false;
    }

    void SelectPlayer(int index)
    {
        Button[] playerButtons = { greenPlayerButton, bluePlayerButton, orangePlayerButton, redPlayerButton };
        if (index < 0 || index >= playerButtons.Length)
            return;

        playerButtons[index].onClick.Invoke();
        Events.Instance.SetCardOutlineColor(index);
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
        SetFinishedGame(PlayerPrefs.GetInt("finishedGame") == 1);
        SetAppliedEvent(PlayerPrefs.GetInt("appliedEvent") == 1);

        UI_System.Instance.SwitchScreens(GetAppliedEvent() ? datosScreen : eventosScreen);
    }

    public void NextShift()
    {
        NotificationsManager.Instance.QuestionNotifications("¿Quieres pasar al siguiente turno?");
        NotificationsManager.Instance.SetYesButton(()=>{
            UI_System.Instance.SwitchScreens(eventosScreen);
            UpdateShift();
        });
    }

    public void UpdateShift()
    {
        shiftText.text = "Año " + shift;
        eventText.text = "Fase de Eventos\nAño " + shift;
        shiftText2.text = "Indicadores de tu Estado\nAño " + shift;
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
