using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class StudentViewAttendanceManager : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject[] otherScreens;

    [Header("Day Attendance Fields")]
    [SerializeField] private DateButton dayAttendanceDate;
    [SerializeField] private GameObject classAttendanceViewPrefab;
    [SerializeField] private GameObject classAttendanceViewContentParent;
    private StudentInfoData currentDayStudent;
    private StudentAttendanceEntryData currentDayStudentAttendance;
    private bool foundStudentInfo;
    private bool foundStudentAttendance;

    private void Start()
    {
        mainScreen.SetActive(true);
        foreach (GameObject screen in otherScreens)
        {
            screen.SetActive(false);
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MobileHome");
    }

    //Day Attendance
    public void SelectStudentDaySchedule()
    {
        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<StudentInfoData>(SelectStudentDayScheduleCallbackInfo));
        Database.instance.ReadData(Database.instance.GetUsername() + "*" + dayAttendanceDate.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(SelectStudentDayScheduleCallbackAttendance));
    }

    private void SelectStudentDayScheduleCallbackInfo(StudentInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find StudentInfo");
            return;
        }

        currentDayStudent = output;
        foundStudentInfo = true;
        FoundStudent();
    }

    private void SelectStudentDayScheduleCallbackAttendance(StudentAttendanceEntryData output)
    {
        if (output == null)
        {
            currentDayStudentAttendance = new StudentAttendanceEntryData(Int32.Parse(Database.instance.GetUsername()), dayAttendanceDate.CurrentDate, new List<string>(), new List<string>());
        }
        else
        {
            currentDayStudentAttendance = output;
        }


        foundStudentAttendance = true;
        FoundStudent();
    }

    private void FoundStudent()
    {
        if (!foundStudentAttendance || !foundStudentInfo) return;

        foundStudentAttendance = false;
        foundStudentInfo = false;

        foreach (Transform child in classAttendanceViewContentParent.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 7; i++)
        {

            ClassAttendanceView viewBox = Instantiate(classAttendanceViewPrefab, classAttendanceViewContentParent.transform).GetComponent<ClassAttendanceView>();

            viewBox.SetTeacherText(currentDayStudent.classList[i].ToString());
            AddTeacherNameToIDField(currentDayStudent.classList[i], viewBox);

            if (!currentDayStudentAttendance.presentList.Contains(currentDayStudent.classList[i].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Absent);
            }
            else if (currentDayStudentAttendance.tardyList.Contains(currentDayStudent.classList[i].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Tardy);
            }
            else if (currentDayStudentAttendance.presentList.Contains(currentDayStudent.classList[i].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Present);
            }
        }
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
