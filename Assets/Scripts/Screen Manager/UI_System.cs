using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_System : MonoBehaviour
{
    #region Variables
    public static UI_System Instance{get; private set;}

    [Header("MAIN PROPERTIES")]
    public UI_Screen m_StartScreen;

    [Header("SYSTEM EVENTS")]
    public UnityEvent onSwitchedScreen = new UnityEvent();
    [Header("FADER PROPERTIES")]
    public Image m_Fader;
    public float m_FadeInDuration = 1f;
    public float m_FadeOutDuration = 1f;
    public Component[] screens = new Component[0];
    private UI_Screen previousScreen;
    public UI_Screen PreviousScreen{get{return previousScreen;}}
    private UI_Screen currentScreen;
    public UI_Screen CurrentScreen{get{return currentScreen;}}
    #endregion

    #region Main Method
    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
    void Start()
    {
        screens = GetComponentsInChildren<UI_Screen>(true);
        if(m_StartScreen)
        {
            SwitchScreens(m_StartScreen);            
        }
        if(m_Fader)
        {
            m_Fader.gameObject.SetActive(true);
        }
        FadeIn();
    }
    #endregion

    #region Helper Method
    public void SwitchScreens(UI_Screen aScreen)
    {
        if(aScreen)
        {
            if(currentScreen)
            {
                currentScreen.CloseScreen();
                previousScreen = currentScreen;
            }
            currentScreen = aScreen;
            currentScreen.gameObject.SetActive(true);
            currentScreen.StartScreen();

            if(onSwitchedScreen != null)
            {
                onSwitchedScreen.Invoke();
            }
        }
    }

    public void FadeIn()
    {
        if(m_Fader)
        {
            m_Fader.CrossFadeAlpha(0f, m_FadeInDuration, false);
        }
    }

    public void FadeOut()
    {
        if(m_Fader)
        {
            m_Fader.CrossFadeAlpha(1f, m_FadeInDuration, false);
        }
    }
    
    public void GoToPreviousScreen()
    {
        if(previousScreen)
        {
            SwitchScreens(previousScreen);
        }
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(WaitToLoadScene(sceneIndex));
    }

    IEnumerator WaitToLoadScene(int sceneIndex)
    {
        yield return null;
    }
    #endregion
}
