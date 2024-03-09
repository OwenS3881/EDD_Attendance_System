using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttendanceExcuseRequest
{
    public int studentId;
    public int teacherId;
    public string date;
    public string reason;
    public bool teacherDenied;
    public string imageName;
    public string imageToken;

    public AttendanceExcuseRequest(int studentId, int teacherId, string date, string reason, bool teacherDenied)
    {
        this.studentId = studentId;
        this.teacherId = teacherId;
        this.date = date;
        this.reason = reason;
        this.teacherDenied = teacherDenied;
    }

    public AttendanceExcuseRequest(int studentId, int teacherId, string date, string reason, bool teacherDenied, string imageName, string imageToken)
    {
        this.studentId = studentId;
        this.teacherId = teacherId;
        this.date = date;
        this.reason = reason;
        this.teacherDenied = teacherDenied;
        this.imageName = imageName;
        this.imageToken = imageToken;
    }
}
