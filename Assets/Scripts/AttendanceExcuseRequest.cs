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

    public AttendanceExcuseRequest(int studentId, int teacherId, string date, string reason)
    {
        this.studentId = studentId;
        this.teacherId = teacherId;
        this.date = date;
        this.reason = reason;
    }
}
