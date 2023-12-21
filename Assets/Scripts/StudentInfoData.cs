using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StudentInfoData : BasicData
{
    public int studentId;
    public string studentName;
    public int[] classList;

    public StudentInfoData(int studentId, string studentName, int[] classList)
    {
        this.fileName = studentId.ToString();
        this.studentId = studentId;
        this.studentName = studentName;
        this.classList = classList;

        if (classList.Length != 7)
        {
            Debug.LogError("Invalid schedule length");
        }
    }
}
