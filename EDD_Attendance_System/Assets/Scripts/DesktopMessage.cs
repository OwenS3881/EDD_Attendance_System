using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DesktopMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Animator anim;
    [SerializeField] private Button okButton;

    public void Initialize(string message)
    {
        messageText.text = message;
        anim.SetTrigger("Appear");
        okButton.Select();
    }

    public void End()
    {
        anim.SetTrigger("Disappear");
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
