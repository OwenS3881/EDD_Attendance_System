using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeacherInfoData : BasicData
{
    public int teacherId;
    public string teacherName;
    public List<ListWrapper<string>> roster;

    public TeacherInfoData(TeacherInfoData clone)
    {
        this.fileName = clone.fileName;
        this.teacherId = clone.teacherId;
        this.teacherName = clone.teacherName;
        this.roster = clone.roster;
    }

    public TeacherInfoData(int teacherId, string teacherName, List<ListWrapper<string>> roster)
    {
        this.fileName = teacherId.ToString();
        this.teacherId = teacherId;
        this.teacherName = teacherName;
        this.roster = roster;
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
