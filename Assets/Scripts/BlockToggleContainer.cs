using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BlockToggleContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image bg;
    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;
    [SerializeField] private int period;
    public int Period
    {
        get
        {
            return period;
        }
        set
        {
            period = value;
            label.text = period.ToString();
        }
    }

    private void Start()
    {
        Period = period;
    }

    public void OnToggle(bool state)
    {
        bg.color = state ? onColor : offColor;
    }

    public bool IsOn()
    {
        return toggle.isOn;
    }


    public void SetState(bool state)
    {
        toggle.isOn = state;
    }
}
