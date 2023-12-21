using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MobileMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Animator anim;

    public void Initialize(string message)
    {
        messageText.text = message;
        anim.SetTrigger("Appear");
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
