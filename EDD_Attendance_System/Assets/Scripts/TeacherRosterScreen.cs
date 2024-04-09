using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TeacherRosterScreen : MonoBehaviour
{
    [SerializeField] private DateButton dateButton;
    [SerializeField] private TMP_InputField periodField;
    [SerializeField] private GameObject rosterViewContainer;
    [SerializeField] private GameObject rosterViewBoxPrefab;
    private TeacherInfoData currentTeacherAttendance;
    private List<StudentAttendanceEntryData> teacherRosterAttendances;

    [SerializeField] private GameObject scrollContent;
    [SerializeField] private float heightPerBox;
    private float scrollContentDefaultHeight = 700f;

    private void Start()
    {
        ClearContainer();
    }

    private void ClearContainer()
    {
        foreach (Transform t in rosterViewContainer.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(rosterViewContainer.transform)) continue;

            Destroy(t.gameObject);
        }

        scrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(scrollContent.GetComponent<RectTransform>().sizeDelta.x, scrollContentDefaultHeight);
    }

    public void FieldUpdated()
    {
        if (dateButton.CurrentDate != null && !string.IsNullOrEmpty(periodField.text))
        {
            SelectTeacherAttendance();
        }
    }

    public void SelectTeacherAttendance()
    {
        currentTeacherAttendance = null;
        teacherRosterAttendances = new List<StudentAttendanceEntryData>();
        ClearContainer();

        if (dateButton.CurrentDate == null)
        {
            DesktopGraphics.instance.DisplayMessage("Please select the date");
            return;
        }

        if (string.IsNullOrEmpty(periodField.text))
        {
            DesktopGraphics.instance.DisplayMessage("Please enter the period");
            return;
        }

        if (Int32.Parse(periodField.text) > 7 || Int32.Parse(periodField.text) < 1)
        {
            DesktopGraphics.instance.DisplayMessage("Invalid period, must be between 1-7");
            return;
        }

        DesktopGraphics.instance.Loading(true);
        
        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<TeacherInfoData>(SelectTeacherAttendanceCallback));
    }

    private void SelectTeacherAttendanceCallback(TeacherInfoData output)
    {
        if (output == null)
        {
            DesktopGraphics.instance.DisplayMessage("No roster found");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        currentTeacherAttendance = output;

        int period = Int32.Parse(periodField.text);
        for (int i = 0; i < currentTeacherAttendance.roster[period - 1].internalList.Count; i++)
        {
            Database.instance.ReadData(currentTeacherAttendance.roster[period - 1].internalList[i] + "*" + dateButton.CurrentDate, new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(SelectTeacherGetStudentCallback), new object[] { Int32.Parse(currentTeacherAttendance.roster[period - 1].internalList[i]) });
        }
    }

    private void SelectTeacherGetStudentCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        int studentId = (int)additionalParams[0];

        StudentAttendanceEntryData student;

        if (output == null)
        {
            student = new StudentAttendanceEntryData(studentId, dateButton.CurrentDate, new List<string>(), new List<string>());
        }
        else
        {
            student = output;
        }

        teacherRosterAttendances.Add(student);
        FoundStudents();
    }

    private void FoundStudents()
    {
        if (teacherRosterAttendances.Count < currentTeacherAttendance.roster[Int32.Parse(periodField.text) - 1].internalList.Count)
        {
            return;
        }

        teacherRosterAttendances.Sort();

        scrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(scrollContent.GetComponent<RectTransform>().sizeDelta.x, (teacherRosterAttendances.Count * heightPerBox) + scrollContentDefaultHeight);

        for (int i = 0; i < teacherRosterAttendances.Count; i++)
        {
            RosterViewBox rosterBox = Instantiate(rosterViewBoxPrefab, rosterViewContainer.transform).GetComponent<RosterViewBox>();

            rosterBox.mainLabel.text = teacherRosterAttendances[i].studentId.ToString();

            AddStudentNameToIDField(teacherRosterAttendances[i].studentId, rosterBox.mainLabel);

            if (!teacherRosterAttendances[i].presentList.Contains(Database.instance.GetUsername()))
            {
                rosterBox.SetStatus(AttendanceStatus.Absent);
            }
            else if (teacherRosterAttendances[i].tardyList.Contains(Database.instance.GetUsername()))
            {
                rosterBox.SetStatus(AttendanceStatus.Tardy);
            }
            else if (teacherRosterAttendances[i].presentList.Contains(Database.instance.GetUsername()))
            {
                rosterBox.SetStatus(AttendanceStatus.Present);
            }
        }
        DesktopGraphics.instance.Loading(false);
    }

    private void AddStudentNameToIDField(int studentId, TMP_Text field)
    {
        Database.instance.ReadData(studentId.ToString(), new Database.ReadDataCallbackParams<StudentInfoData>(AddStudentNameToIDFieldCallback), new object[] { field });
    }

    private void AddStudentNameToIDFieldCallback(StudentInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        TMP_Text field = (TMP_Text)additionalParams[0];

        field.text = output.studentId + " - " + output.studentName;
    }

    public void UpdateTeacherAttendance()
    {
        if (rosterViewContainer.transform.childCount <= 0 || teacherRosterAttendances.Count <= 0 || currentTeacherAttendance == null)
        {
            DesktopGraphics.instance.DisplayMessage("Search for a roster before saving");
            return;
        }

        DesktopGraphics.instance.Loading(true);

        List<RosterViewBox> filteredRosterBoxes = new List<RosterViewBox>();
        for (int i = 0; i < rosterViewContainer.transform.childCount; i++)
        {
            filteredRosterBoxes.Add(rosterViewContainer.transform.GetChild(i).GetComponent<RosterViewBox>());
        }

        filteredRosterBoxes.Sort(delegate (RosterViewBox r1, RosterViewBox r2)
        {
            return r1.mainLabel.text.CompareTo(r2.mainLabel.text);
        });

        string teacherId = Database.instance.GetUsername();

        for (int i = 0; i < teacherRosterAttendances.Count; i++)
        {
            //presentToggle
            if (filteredRosterBoxes[i].GetStatus().Equals("Present"))
            {
                if (!teacherRosterAttendances[i].presentList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].presentList.Add(teacherId);
                }
            }
            else
            {
                if (teacherRosterAttendances[i].presentList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].presentList.Remove(teacherId);
                }
            }
            //tardyToggle
            if (filteredRosterBoxes[i].GetStatus().Equals("Tardy"))
            {
                if (!teacherRosterAttendances[i].tardyList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].tardyList.Add(teacherId);
                }
                if (!teacherRosterAttendances[i].presentList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].presentList.Add(teacherId);
                }
            }
            else
            {
                if (teacherRosterAttendances[i].tardyList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].tardyList.Remove(teacherId);
                }
            }
            SaveAttendanceData(teacherRosterAttendances[i]);
        }

        DesktopGraphics.instance.Loading(false);
        DesktopGraphics.instance.DisplayMessage("Success");
    }

    private void SaveAttendanceData(StudentAttendanceEntryData attendanceData)
    {
        Database.instance.ReadData(attendanceData.studentId.ToString(), new Database.ReadDataCallbackParams<StudentInfoData>(SaveAttendanceDataCallback), new object[] {attendanceData});
    }

    private void SaveAttendanceDataCallback(StudentInfoData output, object[] additionalParams)
    {
        StudentAttendanceEntryData updatedEntry = (StudentAttendanceEntryData)additionalParams[0];

        if (output == null)
        {
            DesktopGraphics.instance.DisplayMessage("An error has occurred");
            return;
        }

        if (!output.attendanceObjects.Contains(updatedEntry.fileName))
        {
            output.attendanceObjects.Add(updatedEntry.fileName);
            Database.instance.SaveDataToFirebase(output);
        }


        Database.instance.SaveDataToFirebase(updatedEntry);
    }
}
