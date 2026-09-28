using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationsManager : Singleton<NotificationsManager>
{
    Image notificationsMenu;
    public GameObject notificationsBackground;
    public TMP_Text notificationsText;
    public Button notificationsYesButton;
    public Button notificationsNoButton;
    public Button notificationsCloseButton;

    void Start()
    {
        notificationsMenu = GetComponent<Image>();
    }

    public void WarningNotifications(string text) => ShowNotification(text, false);

    public void QuestionNotifications(string text) => ShowNotification(text, true);

    // isQuestion: muestra los botones Sí / No
    void ShowNotification(string text, bool isQuestion)
    {
        SetVisible(true);
        notificationsText.text = text;
        notificationsYesButton.gameObject.SetActive(isQuestion);
        notificationsNoButton.gameObject.SetActive(isQuestion);
    }

    public void SetCloseFunction()
    {
        SetVisible(false);
        notificationsCloseButton.onClick.RemoveAllListeners();
    }

    void SetVisible(bool visible)
    {
        notificationsMenu.raycastTarget = visible;
        notificationsBackground.SetActive(visible);
    }

    public void SetYesButton(Action function)
    {
        notificationsYesButton.onClick.RemoveAllListeners();
        notificationsYesButton.onClick.AddListener(() => function());
    }
}
