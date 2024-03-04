using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClassAttendanceView : MonoBehaviour
{
    [SerializeField] private TMP_Text teacherText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image statusBG;

    [SerializeField] private Color absentColor;
    [SerializeField] private Color tardyColor;
    [SerializeField] private Color presentColor;

    public void SetTeacherText(string text)
    {
        teacherText.text = text;
    }

    public void SetStatus(AttendanceStatus status)
    {
        if (status == AttendanceStatus.Absent)
        {
            statusText.text = "Absent";
            statusBG.color = absentColor;
        }
        else if (status == AttendanceStatus.Tardy)
        {
            statusText.text = "Tardy";
            statusBG.color = tardyColor;
        }
        else if (status == AttendanceStatus.Present)
        {
            statusText.text = "Present";
            statusBG.color = presentColor;
        }
    }
}
