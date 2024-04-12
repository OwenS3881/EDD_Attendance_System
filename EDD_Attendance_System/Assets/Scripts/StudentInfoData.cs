using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class StudentInfoData : BasicData , IComparable
{
    public int studentId;
    public string studentName;
    public int[] classList;
    public int schoolId;
    public List<string> attendanceObjects;

    public StudentInfoData(int studentId, string studentName, int[] classList, int schoolId, List<string> attendanceObjects)
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
