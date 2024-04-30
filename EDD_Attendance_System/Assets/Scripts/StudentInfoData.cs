using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class StudentInfoData : BasicData , IComparable
{
    public string studentId;
    public string studentName;
    public string[] classList;
    public string schoolId;
    public List<string> attendanceObjects;

    public StudentInfoData(string studentId, string studentName, string[] classList, string schoolId, List<string> attendanceObjects)
    {
        this.fileName = studentId.ToString();
        this.studentId = studentId;
        this.studentName = studentName;
        this.classList = classList;
        this.schoolId = schoolId;
        this.attendanceObjects = attendanceObjects;

        if (classList.Length != 7)
        {
            Debug.LogError("Invalid schedule length");
        }
    }

    public int CompareTo(object other)
    {
        StudentInfoData otherData = other as StudentInfoData;

        return this.fileName.CompareTo(otherData.fileName);
    }
}
