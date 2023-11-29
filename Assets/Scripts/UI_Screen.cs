using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CanvasGroup))]
public class UI_Screen : MonoBehaviour
{
    #region Variables
    [Header("MAIN PROPERTIES")]
    public Selectable m_StartSelectable;

    [Header("SCREEN EVENTS")]
    public UnityEvent onScreenStart = new UnityEvent();
    public UnityEvent onScreenClose = new UnityEvent();

    private Animator animator; 
    #endregion

    #region Main Method
    void Awake()
    {
        animator = GetComponent<Animator>();
        if(m_StartSelectable)
        {
            EventSystem.current.SetSelectedGameObject(m_StartSelectable.gameObject);
        }
    }
    #endregion

    #region Helper Method
    public virtual void StartScreen()
    {
        if(onScreenStart != null)
        {
            onScreenStart.Invoke();
        }
        HandleAnimator("show");
    }
    public virtual void CloseScreen()
    {
        if(onScreenClose != null)
        {
            onScreenClose.Invoke();
        }
        HandleAnimator("hide");
    }

    void HandleAnimator(string aTrigger)
    {
        if(animator)
        {
            animator.SetTrigger(aTrigger);
        }
    }
    #endregion
}
