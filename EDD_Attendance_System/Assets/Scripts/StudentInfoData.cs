using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StudentInfoData : BasicData
{
    public int studentId;
    public string studentName;
    public int[] classList;
    public int schoolId;

    public StudentInfoData(int studentId, string studentName, int[] classList, int schoolId)
    {
        this.fileName = studentId.ToString();
        this.studentId = studentId;
        this.studentName = studentName;
        this.classList = classList;
        this.schoolId = schoolId;

        if (classList.Length != 7)
        {
            Debug.LogError("Invalid schedule length");
        }
    }
}