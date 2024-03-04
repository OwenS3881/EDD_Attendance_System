using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class AdminStudentAttendanceManager : MonoBehaviour
{
    [SerializeField] private DateButton dateButton;
    [SerializeField] private TMP_InputField studentIdField;
    [SerializeField] private GameObject rosterBoxPrefab;
    [SerializeField] private GameObject rosterBoxContentParent;
    private StudentInfoData studentInfo;
    private StudentAttendanceEntryData currentDayStudentAttendance;
    [SerializeField] private GameObject noClassesGraphic;

    private void Start()
    {

        ClearRosterContainer();

        noClassesGraphic.SetActive(false);
    }

    private void ClearRosterContainer()
    {
        foreach (Transform child in rosterBoxContentParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SelectStudentDaySchedule()
    {
        if (string.IsNullOrEmpty(studentIdField.text))
        {
            DesktopGraphics.instance.DisplayMessage("Please enter a student ID");
            return;
        }

        if (dateButton.CurrentDate == null)
        {
            DesktopGraphics.instance.DisplayMessage("Please select the date");
            return;
        }

        DesktopGraphics.instance.Loading(true);
        Database.instance.ReadData(studentIdField.text, new Database.ReadDataCallback<StudentInfoData>(GetStudentInfo));
    }

    private void GetStudentInfo(StudentInfoData output)
    {
        if (output == null)
        {
            DesktopGraphics.instance.Loading(false);
            Debug.LogWarning("Couldn't find StudentInfo");
            DesktopGraphics.instance.DisplayMessage("Couldn't find Student");
            return;
        }

        studentInfo = output;

        Database.instance.ReadData(studentIdField.text + "*" + dateButton.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(SelectStudentDayScheduleCallbackAttendance));
    }


    private void SelectStudentDayScheduleCallbackAttendance(StudentAttendanceEntryData output)
    {
        if (output == null)
        {
            currentDayStudentAttendance = new StudentAttendanceEntryData(Int32.Parse(studentIdField.text), dateButton.CurrentDate, new List<string>(), new List<string>());
        }
        else
        {
            currentDayStudentAttendance = output;
        }

        FoundStudent();
    }

    private void FoundStudent()
    {
        ClearRosterContainer();

        List<int> periods = GetPeriods(dateButton.CurrentDate);

        foreach (int p in periods)
        {
            RosterViewBox rosterBox = Instantiate(rosterBoxPrefab, rosterBoxContentParent.transform).GetComponent<RosterViewBox>();

            rosterBox.mainLabel.text = studentInfo.classList[p - 1].ToString();

            AddTeacherNameToIDField(studentInfo.classList[p - 1], rosterBox);

            if (!currentDayStudentAttendance.presentList.Contains(studentInfo.classList[p - 1].ToString()))
            {
                rosterBox.SetStatus(AttendanceStatus.Absent);
            }
            else if (currentDayStudentAttendance.tardyList.Contains(studentInfo.classList[p - 1].ToString()))
            {
                rosterBox.SetStatus(AttendanceStatus.Tardy);
            }
            else if (currentDayStudentAttendance.presentList.Contains(studentInfo.classList[p - 1].ToString()))
            {
                rosterBox.SetStatus(AttendanceStatus.Present);
            }
        }

        noClassesGraphic.SetActive(periods.Count == 0);

        DesktopGraphics.instance.Loading(false);
    }

    private List<int> GetPeriods(string date)
    {
        //check for overrides
        foreach (ScheduledPeriods sp in AdminHomeManager.instance.currentData.scheduleOverrides)
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
        foreach (ScheduledPeriods sp in AdminHomeManager.instance.currentData.blockSchedule)
        {
            if (sp.date.ToLower().Equals(dayOfWeek))
            {
                return sp.periods;
            }
        }

        return new List<int>();
    }

    private void AddTeacherNameToIDField(int teacherId, RosterViewBox viewBox)
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

        RosterViewBox viewBox = (RosterViewBox)additionalParams[0];

        viewBox.mainLabel.text = output.teacherId + " - " + output.teacherName;
    }

    public void UpdateDayAttendance()
    {
        if (rosterBoxContentParent.transform.childCount <= 0 || currentDayStudentAttendance == null)
        {
            DesktopGraphics.instance.DisplayMessage("Search for a student before saving");
            return;
        }

        DesktopGraphics.instance.Loading(true);

        currentDayStudentAttendance.presentList.Clear();
        currentDayStudentAttendance.tardyList.Clear();

        List<RosterViewBox> filteredRosterBoxes = new List<RosterViewBox>();
        for (int i = 0; i < rosterBoxContentParent.transform.childCount; i++)
        {
            filteredRosterBoxes.Add(rosterBoxContentParent.transform.GetChild(i).GetComponent<RosterViewBox>());
        }

        filteredRosterBoxes.Sort(delegate (RosterViewBox r1, RosterViewBox r2)
        {
            return r1.mainLabel.text.CompareTo(r2.mainLabel.text);
        });

        for (int i = 0; i < filteredRosterBoxes.Count; i++)
        {
            //get teacher id
            string[] splitLabel = filteredRosterBoxes[i].mainLabel.text.Split(" - ");
            string teacherId = "";
            if (splitLabel.Length == 1 || splitLabel.Length == 2)
            {
                teacherId = splitLabel[0];
            }
            else
            {
                Debug.LogError("Something went pretty wrong");
            }

            //presentToggle
            if (filteredRosterBoxes[i].GetStatus().Equals("Present"))
            {
                if (!currentDayStudentAttendance.presentList.Contains(teacherId))
                {
                    currentDayStudentAttendance.presentList.Add(teacherId);
                }
            }
            else
            {
                if (currentDayStudentAttendance.presentList.Contains(teacherId))
                {
                    currentDayStudentAttendance.presentList.Remove(teacherId);
                }
            }
            //tardyToggle
            if (filteredRosterBoxes[i].GetStatus().Equals("Tardy"))
            {
                if (!currentDayStudentAttendance.tardyList.Contains(teacherId))
                {
                    currentDayStudentAttendance.tardyList.Add(teacherId);
                }
                if (!currentDayStudentAttendance.presentList.Contains(teacherId))
                {
                    currentDayStudentAttendance.presentList.Add(teacherId);
                }
            }
            else
            {
                if (currentDayStudentAttendance.tardyList.Contains(teacherId))
                {
                    currentDayStudentAttendance.tardyList.Remove(teacherId);
                }
            }
        }
        Database.instance.SaveDataToFirebase(currentDayStudentAttendance);
        DesktopGraphics.instance.Loading(false);
        DesktopGraphics.instance.DisplayMessage("Success");
    }
}
