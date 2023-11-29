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

    int shift => EconomicModel.Instance.shift;
    TMP_Text shiftText => EconomicModel.Instance.shiftText;
    TMP_Text shiftText2 => EconomicModel.Instance.shiftText2;
    Button continuarPartidaButton => EconomicModel.Instance.continuarPartidaButton;

    public UI_Screen datosScreen;

    public bool finishedGame;
    public void SetFinishedGame(bool finishedGame) {this.finishedGame = finishedGame; }

    public Button greenPlayerButton;
    public Button bluePlayerButton;
    public Button orangePlayerButton;
    public Button redPlayerButton;

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
        if(shift <= SHIFTS)
        {
            shiftText.text = "Turno "+shift.ToString();
            shiftText2.text = "Indicadores de tu Estado\nTurno "+shift.ToString();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            ExitApp();
    }

    public void NextShift()
    {
        NotificationsManager.Instance.QuestionNotifications("¿Quieres pasar al siguiente turno?");
        NotificationsManager.Instance.SetYesButton(()=>{
            EconomicModel.Instance.SetShift(shift+1);
            UI_System.Instance.SwitchScreens(datosScreen);
        });
    }

    public void ExitApp()
    {
        NotificationsManager.Instance.QuestionNotifications("¿Esta seguro que quiere acabar la partida antes de terminar los 10 turnos?");
        NotificationsManager.Instance.SetYesButton(()=> {
            PlayerPrefs.SetInt("finishedGame", finishedGame ? 1 : 0);

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
