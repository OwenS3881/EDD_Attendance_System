using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StudentViewBox : MonoBehaviour
{
    [SerializeField] private TMP_Text mainText;

    private string studentName = "???";
    public string StudentName
    {
        get { return studentName;  }
        set
        {
            studentName = value;
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
        mainText.text = $"{Id} - {StudentName}";
    }
}
