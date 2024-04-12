using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TeacherViewBox : MonoBehaviour
{
    [SerializeField] private TMP_Text mainText;

    private string teacherName = "???";
    public string TeacherName
    {
        get { return teacherName; }
        set
        {
            teacherName = value;
            SetMainText();
        }
    }

    private string id = "???";
    public string Id
    {
        get { return id; }
        set
        {
            id = value;
            SetMainText();
        }
    }

    private void SetMainText()
    {
        mainText.text = $"{Id} - {TeacherName}";
    }
}
