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
    public Dictionary<string, ListWrapper<int>> blockSchedule;
    public Dictionary<string, ListWrapper<int>> scheduleOverrides;
    public List<string> offDays;
    public Dictionary<string, string> test;

    public SchoolInfoData(SchoolInfoData clone)
    {
        this.fileName = clone.fileName;
        this.schoolId = clone.schoolId;
        this.schoolName = clone.schoolName;
        this.teacherList = clone.teacherList;
        this.studentList = clone.studentList;
        this.blockSchedule = clone.blockSchedule;
        this.scheduleOverrides = clone.scheduleOverrides;
        this.offDays = clone.offDays;
    }

    public SchoolInfoData(int schoolId, string schoolName, List<string> teacherList, List<string> studentList, Dictionary<string, ListWrapper<int>> blockSchedule, Dictionary<string, ListWrapper<int>> scheduleOverrides, List<string> offDays)
    {
        this.fileName = schoolId.ToString();
        this.schoolId = schoolId;
        this.schoolName = schoolName;
        this.teacherList = teacherList;
        this.studentList = studentList;
        this.blockSchedule = blockSchedule;
        this.scheduleOverrides = scheduleOverrides;
        this.offDays = offDays;

        if (blockSchedule.Count < 7)
        {
            string[] daysOfTheWeek = new string[] {"monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };
            for (int i = 0; i < daysOfTheWeek.Length; i++)
            {
                blockSchedule[daysOfTheWeek[i]] = new ListWrapper<int>();
                for (int j = 1; j <= 7; j++)
                {
                    blockSchedule[daysOfTheWeek[i]].Add(j);
                }
            }
        }
    }
}
