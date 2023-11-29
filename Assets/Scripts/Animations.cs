using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{
    public Animator animator;

    public void PlayAnimatorController(string command)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if(parameter.name == command)
            {
                animator.SetBool(command, true);
                PlayerPrefs.SetString("player", command.Substring(4).ToLower());
            }
            else
                animator.SetBool(parameter.name, false);

        }
    }
}
