using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SchoolInfoData : BasicData
{
    public int schoolId;
    public string schoolName;
    public List<string> teacherList;
    public List<string> studentList;
    public List<ScheduledPeriods> blockSchedule; //contains periods that will occur on each day
    public List<ScheduledPeriods> scheduleOverrides; //set periods for days with alternate schedules; for days off, set the periods list to null/empty
    public List<AttendanceExcuseRequest> excuseRequests;

    public SchoolInfoData(SchoolInfoData clone)
    {
        this.fileName = clone.fileName;
        this.schoolId = clone.schoolId;
        this.schoolName = clone.schoolName;
        this.teacherList = clone.teacherList;
        this.studentList = clone.studentList;
        this.blockSchedule = clone.blockSchedule;
        this.scheduleOverrides = clone.scheduleOverrides;
        this.excuseRequests = clone.excuseRequests;
    }

    public SchoolInfoData(int schoolId, string schoolName, List<string> teacherList, List<string> studentList, List<ScheduledPeriods> blockSchedule, List<ScheduledPeriods> scheduleOverrides, List<AttendanceExcuseRequest> excuseRequests)
    {
        this.fileName = schoolId.ToString();
        this.schoolId = schoolId;
        this.schoolName = schoolName;
        this.teacherList = teacherList;
        this.studentList = studentList;
        this.blockSchedule = blockSchedule;
        this.scheduleOverrides = scheduleOverrides;
        this.excuseRequests = excuseRequests;

        if (blockSchedule.Count < 7)
        {
            string[] daysOfTheWeek = new string[] {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            for (int i = 0; i < daysOfTheWeek.Length; i++)
            {
                blockSchedule.Add(new ScheduledPeriods(daysOfTheWeek[i], new List<int>()));
                for (int j = 1; j <= 7; j++)
                {
                    blockSchedule[i].periods.Add(j);
                }
            }
        }
    }
}
