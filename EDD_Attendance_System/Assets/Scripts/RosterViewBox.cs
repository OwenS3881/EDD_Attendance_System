using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RosterViewBox : MonoBehaviour
{
    public TMP_Text mainLabel;
    [SerializeField] private Toggle presentToggle;
    [SerializeField] private Toggle tardyToggle;
    [SerializeField] private Image statusBG;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Color absentColor;
    [SerializeField] private Color tardyColor;
    [SerializeField] private Color presentColor;

    public void OnValueChanged(bool newStatus)
    {
        //When a student is tardy they have to also be present
        if (!presentToggle.isOn && tardyToggle.isOn)
        {
            if (newStatus)
            {
                presentToggle.isOn = true;
            }
            else
            {
                tardyToggle.isOn = false;
            }
        }

        UpdateStatus();
    }


    public void UpdateStatus()
    {
        if (!presentToggle.isOn)
        {
            SetStatus(AttendanceStatus.Absent);
        }
        else if (tardyToggle.isOn && presentToggle.isOn)
        {
            SetStatus(AttendanceStatus.Tardy);
        }
        else if (presentToggle.isOn && !tardyToggle.isOn)
        {
            SetStatus(AttendanceStatus.Present);
        }
    }

    public void SetStatus(AttendanceStatus status)
    {
        if (status == AttendanceStatus.Absent)
        {
            statusText.text = "Absent";
            statusBG.color = absentColor;
            presentToggle.isOn = false;
            tardyToggle.isOn = false;
        }
        else if (status == AttendanceStatus.Tardy)
        {
            statusText.text = "Tardy";
            statusBG.color = tardyColor;
            presentToggle.isOn = true;
            tardyToggle.isOn = true;
        }
        else if (status == AttendanceStatus.Present)
        {
            statusText.text = "Present";
            statusBG.color = presentColor;
            presentToggle.isOn = true;
            tardyToggle.isOn = false;
        }
    }

    public string GetStatus()
    {
        return statusText.text;
    }
}
