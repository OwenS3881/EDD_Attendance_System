using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class ParentAttendanceManager : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject[] otherScreens;

    [Header("Day Attendance Fields")]
    [SerializeField] private DateButton dayAttendanceDate;
    [SerializeField] private GameObject classAttendanceViewPrefab;
    [SerializeField] private GameObject classAttendanceViewContentParent;
    private StudentAttendanceEntryData currentDayStudentAttendance;
    [SerializeField] private GameObject noClassesGraphic;

    private void Start()
    {
        mainScreen.SetActive(true);
        foreach (GameObject screen in otherScreens)
        {
            screen.SetActive(false);
        }

        ClearDayAttendanceContainer();

        noClassesGraphic.SetActive(true);
    }

    public void SelectStudentDaySchedule()
    {
        Database.instance.ReadData(ParentHomeManager.instance.StudentInfo.studentId.ToString() + "*" + dayAttendanceDate.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(SelectStudentDayScheduleCallbackAttendance));
        DesktopGraphics.instance.Loading(true);
    }


    private void SelectStudentDayScheduleCallbackAttendance(StudentAttendanceEntryData output)
    {
        if (output == null)
        {
            currentDayStudentAttendance = new StudentAttendanceEntryData(ParentHomeManager.instance.StudentInfo.studentId, dayAttendanceDate.CurrentDate, new List<string>(), new List<string>());
        }
        else
        {
            currentDayStudentAttendance = output;
        }

        FoundStudent();
    }

    private void ClearDayAttendanceContainer()
    {
        foreach (Transform child in classAttendanceViewContentParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void FoundStudent()
    {
        DesktopGraphics.instance.Loading(false);

        ClearDayAttendanceContainer();

        List<int> periods = GetPeriods(dayAttendanceDate.CurrentDate);

        foreach (int p in periods)
        {

            ClassAttendanceView viewBox = Instantiate(classAttendanceViewPrefab, classAttendanceViewContentParent.transform).GetComponent<ClassAttendanceView>();

            viewBox.SetTeacherText(ParentHomeManager.instance.StudentInfo.classList[p - 1].ToString());
            AddTeacherNameToIDField(ParentHomeManager.instance.StudentInfo.classList[p - 1], viewBox);

            if (!currentDayStudentAttendance.presentList.Contains(ParentHomeManager.instance.StudentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Absent);
            }
            else if (currentDayStudentAttendance.tardyList.Contains(ParentHomeManager.instance.StudentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Tardy);
            }
            else if (currentDayStudentAttendance.presentList.Contains(ParentHomeManager.instance.StudentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Present);
            }
        }

        noClassesGraphic.SetActive(periods.Count == 0);
    }

    private List<int> GetPeriods(string date)
    {
        //check for overrides
        foreach (ScheduledPeriods sp in ParentHomeManager.instance.SchoolData.scheduleOverrides)
        {
            string[] splitDate = sp.date.Split("*");
            if (splitDate.Length == 1) //single date
            {
                if (date.Equals(splitDate[0]))
                {
                    return sp.periods;
                }
            }
            else if (splitDate.Length == 2) //date range
            {
                if (MyFunctions.IsDateInRange(date, splitDate[0], splitDate[1]))
                {
                    return sp.periods;
                }
            }
        }

        //check for block schedule
        string dayOfWeek = MyFunctions.GetDayOfWeek(date).ToLower();
        foreach (ScheduledPeriods sp in ParentHomeManager.instance.SchoolData.blockSchedule)
        {
            if (sp.date.ToLower().Equals(dayOfWeek))
            {
                return sp.periods;
            }
        }

        return new List<int>();
    }

    private void AddTeacherNameToIDField(int teacherId, ClassAttendanceView viewBox)
    {
        Database.instance.ReadData(teacherId.ToString(), new Database.ReadDataCallbackParams<TeacherInfoData>(AddTeacherNameToIDFieldCallback), new object[] { viewBox });
    }

    private void AddTeacherNameToIDFieldCallback(TeacherInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        ClassAttendanceView viewBox = (ClassAttendanceView)additionalParams[0];

        viewBox.SetTeacherText(output.teacherId + " - " + output.teacherName);
    }
}
