using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class TeacherInfoData : BasicData , IComparable
{
    public int teacherId;
    public string teacherName;
    public List<ListWrapper<string>> roster;
    public int schoolId;

    public TeacherInfoData(TeacherInfoData clone)
    {
        this.fileName = clone.fileName;
        this.teacherId = clone.teacherId;
        this.teacherName = clone.teacherName;
        this.roster = clone.roster;
        this.schoolId = clone.schoolId;
    }

    public TeacherInfoData(int teacherId, string teacherName, List<ListWrapper<string>> roster, int schoolId)
    {
        this.fileName = teacherId.ToString();
        this.teacherId = teacherId;
        this.teacherName = teacherName;
        this.schoolId = schoolId;
        this.roster = roster;
        if (roster.Count < 7)
        {
            for (int i = 0; i < 7; i++)
            {
                roster.Add(new ListWrapper<string>());
                roster[i].Add("Default");
            }

            foreach (ListWrapper<string> sublist in roster)
            {
                foreach (string entry in sublist.internalList)
                {
                    Debug.Log(entry);
                }
            }
        }
    }

    public int CompareTo(object other)
    {
        TeacherInfoData otherData = other as TeacherInfoData;

        return this.fileName.CompareTo(otherData.fileName);
    }
}
