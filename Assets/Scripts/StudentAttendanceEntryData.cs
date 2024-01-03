using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class StudentAttendanceEntryData : BasicData, IComparable
{
    public int studentId;
    public string date;
    public List<string> presentList;
    public List<string> tardyList;

    public StudentAttendanceEntryData(int studentId, string date, List<string> presentList, List<string> tardyList)
    {
        this.fileName = studentId + "*" + date;
        this.studentId = studentId;
        this.date = date;
        this.presentList = presentList;
        this.tardyList = tardyList;
    }

    public int CompareTo(object other)
    {
        StudentAttendanceEntryData otherData = other as StudentAttendanceEntryData;

        return this.fileName.CompareTo(otherData.fileName);
    }
}
