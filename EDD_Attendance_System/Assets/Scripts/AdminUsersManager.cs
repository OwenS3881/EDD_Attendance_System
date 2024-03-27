using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class AdminUsersManager : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScroll;
    [SerializeField] private GameObject[] otherScrolls;

    [Header("Teacher Creation Fields")]
    [SerializeField] private TMP_InputField teacherIdInput;
    [SerializeField] private TMP_InputField teacherNameInput;

    [Header("Student Creation Fields")]
    [SerializeField] private TMP_InputField studentIdInput;
    [SerializeField] private TMP_InputField studentNameInput;
    [SerializeField] private TMP_InputField[] studentClassesInput;

    private void OnEnable()
    {
        mainScroll.SetActive(true);
        foreach (GameObject scroll in otherScrolls)
        {
            scroll.SetActive(false);
        }
    }

    //Teacher Creation Methods
    public void CreateTeacher()
    {
        if (string.IsNullOrEmpty(teacherIdInput.text))
        {
            DesktopGraphics.instance.DisplayMessage("Please enter a teacher ID");
            return;
        }

        if (string.IsNullOrEmpty(teacherNameInput.text))
        {
            DesktopGraphics.instance.DisplayMessage("Please enter a teacher name");
            return;
        }

        if (AdminHomeManager.instance.currentData.teacherList.Contains(Int32.Parse(teacherIdInput.text)))
        {
            DesktopGraphics.instance.DisplayMessage("Teacher ID already exists");
            return;
        }

        DesktopGraphics.instance.Loading(true);
        Database.instance.ReadData(teacherIdInput.text, new Database.ReadDataCallback<TeacherInfoData>(VerifyCreateTeacherId));
    }

    private void VerifyCreateTeacherId(TeacherInfoData output)
    {
        if (output == null && string.IsNullOrEmpty(output.teacherName))
        {
            TeacherInfoData newTeacher = new TeacherInfoData(Int32.Parse(teacherIdInput.text), teacherNameInput.text, new List<ListWrapper<string>>(), Int32.Parse(Database.instance.GetUsername()));
            Database.instance.SaveDataToFirebase(newTeacher);
            DesktopGraphics.instance.Loading(false);
            DesktopGraphics.instance.DisplayMessage("Success");
            OnEnable();
        }
        else
        {
            DesktopGraphics.instance.Loading(false);
            DesktopGraphics.instance.DisplayMessage("Teacher ID already exists");
        }
    }

    //Student Creation Methods
    public void CreateStudent()
    {
        DesktopGraphics.instance.Loading(true);

        if (string.IsNullOrEmpty(studentIdInput.text))
        {
            DesktopGraphics.instance.DisplayMessage("Please enter a student ID");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        if (string.IsNullOrEmpty(studentNameInput.text))
        {
            DesktopGraphics.instance.DisplayMessage("Please enter a student name");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        if (AdminHomeManager.instance.currentData.studentList.Contains(Int32.Parse(studentIdInput.text)))
        {
            DesktopGraphics.instance.DisplayMessage("Student ID already exists");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        DesktopGraphics.instance.Loading(true);
        Database.instance.ReadData(studentIdInput.text, new Database.ReadDataCallback<StudentInfoData>(VerifyStudentCreateId));
    }

    private void VerifyStudentCreateId(StudentInfoData output)
    {
        if (output == null && string.IsNullOrEmpty(output.studentName))
        {
            int[] teacherIds = new int[7];
            for (int i = 0; i < studentClassesInput.Length; i++)
            {
                if (string.IsNullOrEmpty(studentClassesInput[i].text))
                {
                    DesktopGraphics.instance.DisplayMessage($"Please enter a teacher for period {i + 1}");
                    DesktopGraphics.instance.Loading(false);
                    return;
                }
                if (!AdminHomeManager.instance.currentData.teacherList.Contains(Int32.Parse(studentClassesInput[i].text)))
                {
                    DesktopGraphics.instance.DisplayMessage($"Teacher for period {i + 1} does not exist");
                    DesktopGraphics.instance.Loading(false);
                    return;
                }
                teacherIds[i] = Int32.Parse(studentClassesInput[i].text);
            }

            int studentId = Int32.Parse(studentIdInput.text);
            string studentName = studentNameInput.text;

            StudentInfoData newStudent = new StudentInfoData(studentId, studentName, teacherIds, Int32.Parse(Database.instance.GetUsername()));
            Database.instance.SaveDataToFirebase(newStudent);

            StartCoroutine(AddStudentToRoster(studentId, teacherIds, 0));
        }
        else
        {
            DesktopGraphics.instance.Loading(false);
            DesktopGraphics.instance.DisplayMessage("Student ID already exists");
        }
    }

    IEnumerator AddStudentToRoster(int studentId, int[] teacherIds, int index)
    {
        yield return new WaitForSeconds(0.1f);
        Database.instance.ReadData(teacherIds[index].ToString(), new Database.ReadDataCallbackParams<TeacherInfoData>(AddStudentToRosterCallback), new object[] { studentId, teacherIds, index });
    }

    private void AddStudentToRosterCallback(TeacherInfoData output, object[] additionalParams)
    {

        int studentId = (int)additionalParams[0];
        int[] teacherIds = (int[])additionalParams[1];
        int index = (int)additionalParams[2];

        TeacherInfoData updatedInfo;

        if (output == null)
        {
            Debug.LogError("No teacher found with id: " + teacherIds[index]);
            DesktopGraphics.instance.DisplayMessage("An error has occurred");
            DesktopGraphics.instance.Loading(false);
            return;
        }
        else
        {
            updatedInfo = new TeacherInfoData(output);
        }

        updatedInfo.roster[index].Add(studentId.ToString());

        if (updatedInfo.roster[index][0] == "Default")
        {
            updatedInfo.roster[index].internalList.RemoveAt(0);
        }


        Database.instance.SaveDataToFirebase(updatedInfo);

        if (index < 6)
        {
            StartCoroutine(AddStudentToRoster(studentId, teacherIds, index + 1));
        }
        else
        {
            DesktopGraphics.instance.Loading(false);
            DesktopGraphics.instance.DisplayMessage("Success");
            OnEnable();
        }
    }
}
