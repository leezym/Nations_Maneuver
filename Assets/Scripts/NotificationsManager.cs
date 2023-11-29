using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationsManager : MonoBehaviour
{
    public static NotificationsManager Instance{get; private set;}
    Image notificationsMenu;
    public GameObject notificationsBackground;
    public TMP_Text notificationsText;
    public Button notificationsYesButton;
    public Button notificationsNoButton;
    public Button notificationsCloseButton;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        notificationsMenu = this.gameObject.GetComponent<Image>();
    }

    public void WarningNotifications(string text)
    {
        notificationsMenu.raycastTarget = true;
        notificationsBackground.SetActive(true);
        notificationsText.text = text;
        notificationsYesButton.gameObject.SetActive(false);
        notificationsNoButton.gameObject.SetActive(false);
    }
    public void QuestionNotifications(string text)
    {
        notificationsMenu.raycastTarget = true;
        notificationsBackground.SetActive(true);
        notificationsText.text = text;
        notificationsYesButton.gameObject.SetActive(true);
        notificationsNoButton.gameObject.SetActive(true);
    }

    public void SetCloseFunction()
    {
        notificationsMenu.raycastTarget = false;
        notificationsBackground.SetActive(false);
        notificationsCloseButton.onClick.RemoveAllListeners();
    }

    public void SetYesButton(Action function)
    {
        notificationsYesButton.onClick.RemoveAllListeners();
        notificationsYesButton.onClick.AddListener(()=>{
            function();
        });
    }
}
