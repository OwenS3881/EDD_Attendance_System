using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class AdminCreator : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject[] otherScreens;

    [Header("Teacher Creation Fields")]
    [SerializeField] private TMP_InputField teacherIdInput;
    [SerializeField] private TMP_InputField teacherNameInput;

    [Header("Student Creation methods")]
    [SerializeField] private TMP_InputField studentIdInput;
    [SerializeField] private TMP_InputField studentNameInput;
    [SerializeField] private TMP_InputField[] studentClassesInput;

    [Header("Present Fields")]
    [SerializeField] private TMP_InputField presentTeacherField;
    [SerializeField] private TMP_InputField presentStudentField;
    [SerializeField] private TMP_InputField presentDateField;
    [SerializeField] private Toggle presentTardyToggle;

    [Header("Day Student Schedule Fields")]
    [SerializeField] private TMP_InputField dayScheduleStudentIdField;
    [SerializeField] private TMP_InputField dayScheduleDateField;
    [SerializeField] private StudentInfoData currentDayStudent;
    private StudentAttendanceEntryData currentDayStudentAttendance;
    private bool foundStudentInfo;
    private bool foundStudentAttendance;
    [SerializeField] private GameObject searchMenu;
    [SerializeField] private GameObject editMenu;
    [SerializeField] private TMP_Text editTitle;
    [SerializeField] private ClassAttendance[] classAttendanceParents;

    [Header("Teacher Attendance Fields")]
    [SerializeField] private GameObject teacherSearchMenu;
    [SerializeField] private GameObject teacherEditMenu;
    [SerializeField] private TMP_InputField teacherAttendanceIdField;
    [SerializeField] private TMP_InputField teacherAttendanceDateField;
    [SerializeField] private TMP_InputField teacherAttendancePeriodField;
    [SerializeField] private TMP_Text teacherAttendanceTitle;
    [SerializeField] private GameObject studentsParentContainer;
    [SerializeField] private GameObject studentAttendanceCheckboxPrefab;
    private TeacherInfoData currentTeacherAttendance;
    private List<StudentAttendanceEntryData> teacherRosterAttendances;

    private void Start()
    {
        mainScreen.SetActive(true);
        foreach (GameObject screen in otherScreens)
        {
            screen.SetActive(false);
        }

        searchMenu.SetActive(true);
        editMenu.SetActive(false);

        teacherSearchMenu.SetActive(true);
        teacherEditMenu.SetActive(false);
    }

    //Teacher Creation Methods
    public void CreateTeacher()
    {
        TeacherInfoData newTeacher = new TeacherInfoData(teacherIdInput.text, teacherNameInput.text, new List<ListWrapper<string>>(), Database.instance.GetUsername());
        Database.instance.SaveDataToFirebase(newTeacher);
    }

    //Student Creation Methods
    public void CreateStudent()
    {
        string[] teacherIds = new string[7];
        for (int i = 0; i < studentClassesInput.Length; i++)
        {
            if (string.IsNullOrEmpty(studentClassesInput[i].text))
            {
                Debug.LogError("Fill out all classes");
                return;
            }
            teacherIds[i] = studentClassesInput[i].text;
        }

        string studentName = studentNameInput.text;

        StudentInfoData newStudent = new StudentInfoData(studentIdInput.text, studentName, teacherIds, Database.instance.GetUsername(), new List<string>());
        Database.instance.SaveDataToFirebase(newStudent);

        StartCoroutine(AddStudentToRoster(studentIdInput.text, teacherIds, 0));
    }

    IEnumerator AddStudentToRoster(string studentId, string[] teacherIds, int index)
    {
        yield return new WaitForSeconds(0.1f);
        Database.instance.ReadData(teacherIds[index], new Database.ReadDataCallbackParams<TeacherInfoData>(AddStudentToRosterCallback), new object[] { studentId, teacherIds, index });
    }

    private void AddStudentToRosterCallback(TeacherInfoData output, object[] additionalParams)
    {

        string studentId = (string)additionalParams[0];
        string[] teacherIds = (string[])additionalParams[1];
        int index = (int)additionalParams[2];

        TeacherInfoData updatedInfo;

        if (output == null)
        {
            Debug.LogError("No teacher found with id: " + teacherIds[index]);
            return;
        }
        else
        {
            updatedInfo = new TeacherInfoData(output);
        }

        updatedInfo.roster[index].Add(studentId);

        if (updatedInfo.roster[index][0] == "Default")
        {
            updatedInfo.roster[index].internalList.RemoveAt(0);
        }


        Database.instance.SaveDataToFirebase(updatedInfo);

        if (index < 6)
        {
            StartCoroutine(AddStudentToRoster(studentId, teacherIds, index + 1));
        }
    }

    //Present Methods
    public void ManuallyMarkPresent()
    {
        string date = presentDateField.text;

        if (string.IsNullOrEmpty(date))
        {
            date = DateTime.Today.ToString("yyyy-MM-dd");
        }

        MarkPresent(presentStudentField.text, presentTeacherField.text, date, presentTardyToggle.isOn);
    }

    public void MarkPresent(string studentId, string teacherId, string date, bool tardy)
    {
        Database.instance.ReadData(studentId + "*" + date, new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(MarkPresentCallback), new object[] { studentId, teacherId, date, tardy });
    }

    private static void MarkPresentCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        string studentId = (string)additionalParams[0];
        string teacherId = (string)additionalParams[1];
        string date = (string)additionalParams[2];
        bool tardy = (bool)additionalParams[3];

        StudentAttendanceEntryData updatedEntry;

        if (output == null)
        {
            updatedEntry = new StudentAttendanceEntryData(studentId, date, new List<string>(), new List<string>());
        }
        else
        {
            updatedEntry = output;
        }

        //presentList
        if (!updatedEntry.presentList.Contains(teacherId)) updatedEntry.presentList.Add(teacherId);

        //tardyList
        if (tardy)
        {
            if (!updatedEntry.tardyList.Contains(teacherId)) updatedEntry.tardyList.Add(teacherId);
        }
        else
        {
            if (updatedEntry.tardyList.Contains(teacherId)) updatedEntry.tardyList.Remove(teacherId);
        }


        Database.instance.SaveDataToFirebase(updatedEntry);
    }

    //Day Student Schedule Methods
    public void SelectStudentDaySchedule()
    {
        Database.instance.ReadData(dayScheduleStudentIdField.text, new Database.ReadDataCallback<StudentInfoData>(SelectStudentDayScheduleCallbackInfo));
        Database.instance.ReadData(dayScheduleStudentIdField.text + "*" + dayScheduleDateField.text, new Database.ReadDataCallback<StudentAttendanceEntryData>(SelectStudentDayScheduleCallbackAttendance));
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
            currentDayStudentAttendance = new StudentAttendanceEntryData(dayScheduleStudentIdField.text, dayScheduleDateField.text, new List<string>(), new List<string>());
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

        searchMenu.SetActive(false);
        editMenu.SetActive(true);

        editTitle.text = $"{currentDayStudent.studentName}'s ({currentDayStudent.studentId}) Attendance for {currentDayStudentAttendance.date}";

        for (int i = 0; i < 7; i++)
        {
            classAttendanceParents[i].label.text = currentDayStudent.classList[i];
            AddTeacherNameToIDField(currentDayStudent.classList[i], i);
            classAttendanceParents[i].presentToggle.isOn = currentDayStudentAttendance.presentList.Contains(currentDayStudent.classList[i]);
            classAttendanceParents[i].tardyToggle.isOn = currentDayStudentAttendance.tardyList.Contains(currentDayStudent.classList[i]);
        }
    }

    private void AddTeacherNameToIDField(string teacherId, int index)
    {
        Database.instance.ReadData(teacherId, new Database.ReadDataCallbackParams<TeacherInfoData>(AddTeacherNameToIDFieldCallback), new object[] { index });
    }

    private void AddTeacherNameToIDFieldCallback(TeacherInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        int index = (int)additionalParams[0];

        classAttendanceParents[index].GetComponentInChildren<TMP_Text>().text = output.teacherId + " - " + output.teacherName;
    }

    public void UpdateDayAttendance()
    {
        currentDayStudentAttendance.presentList.Clear();
        currentDayStudentAttendance.tardyList.Clear();

        for (int i = 0; i < 7; i++)
        {
            if (classAttendanceParents[i].presentToggle.isOn)
            {
                currentDayStudentAttendance.presentList.Add(currentDayStudent.classList[i]);
            }

            if (classAttendanceParents[i].tardyToggle.isOn)
            {
                currentDayStudentAttendance.tardyList.Add(currentDayStudent.classList[i]);
            }
        }
        Database.instance.SaveDataToFirebase(currentDayStudentAttendance);
    }

    //Teacher Attendance Methods
    public void SelectTeacherAttendance()
    {
        currentTeacherAttendance = null;
        teacherRosterAttendances = new List<StudentAttendanceEntryData>();
        Database.instance.ReadData(teacherAttendanceIdField.text, new Database.ReadDataCallback<TeacherInfoData>(SelectTeacherAttendanceCallback));
    }

    private void SelectTeacherAttendanceCallback(TeacherInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("No teacher found");
            return;
        }

        currentTeacherAttendance = output;

        int period = Int32.Parse(teacherAttendancePeriodField.text);
        for (int i = 0; i < currentTeacherAttendance.roster[period - 1].internalList.Count; i++)
        {
            Database.instance.ReadData(currentTeacherAttendance.roster[period - 1].internalList[i] + "*" + teacherAttendanceDateField.text, new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(SelectTeacherGetStudentCallback), new object[] { currentTeacherAttendance.roster[period - 1].internalList[i] });
        }
    }

    private void SelectTeacherGetStudentCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        string studentId = (string)additionalParams[0];

        StudentAttendanceEntryData student;

        if (output == null)
        {
            student = new StudentAttendanceEntryData(studentId, teacherAttendanceDateField.text, new List<string>(), new List<string>());
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
        if (teacherRosterAttendances.Count < currentTeacherAttendance.roster[Int32.Parse(teacherAttendancePeriodField.text) - 1].internalList.Count)
        {
            return;
        }

        teacherRosterAttendances.Sort();

        teacherSearchMenu.SetActive(false);
        teacherEditMenu.SetActive(true);

        teacherAttendanceTitle.text = $"{currentTeacherAttendance.teacherName}'s ({currentTeacherAttendance.teacherId}) Roster Attendance for {teacherRosterAttendances[0].date}, Period {teacherAttendancePeriodField.text}";

        foreach (Transform t in studentsParentContainer.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(studentsParentContainer.transform)) continue;

            Destroy(t.gameObject);
        }

        for (int i = 0; i < teacherRosterAttendances.Count; i++)
        {
            GameObject presentBox = Instantiate(studentAttendanceCheckboxPrefab, studentsParentContainer.transform);

            StudentAttendanceCheckbox checkBox = presentBox.GetComponent<StudentAttendanceCheckbox>();

            TMP_Text field = checkBox.label;

            field.text = teacherRosterAttendances[i].studentId;

            AddStudentNameToIDField(teacherRosterAttendances[i].studentId, field);

            checkBox.presentToggle.isOn = teacherRosterAttendances[i].presentList.Contains(teacherAttendanceIdField.text);
            checkBox.tardyToggle.isOn = teacherRosterAttendances[i].tardyList.Contains(teacherAttendanceIdField.text);
        }
         
    }

    private void AddStudentNameToIDField(string studentId, TMP_Text field)
    {
        Database.instance.ReadData(studentId, new Database.ReadDataCallbackParams<StudentInfoData>(AddStudentNameToIDFieldCallback), new object[] { field });
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
        List<StudentAttendanceCheckbox> filteredPresentBoxes = new List<StudentAttendanceCheckbox>();
        for (int i = 0; i < studentsParentContainer.transform.childCount; i++)
        {
            filteredPresentBoxes.Add(studentsParentContainer.transform.GetChild(i).GetComponent<StudentAttendanceCheckbox>());
        }

        filteredPresentBoxes.Sort(delegate (StudentAttendanceCheckbox p1, StudentAttendanceCheckbox p2)
        {
            return p1.label.text.CompareTo(p2.label.text);
        });

        string teacherId = teacherAttendanceIdField.text;

        for (int i = 0; i < teacherRosterAttendances.Count; i++)
        {
            //presentToggle
            if (filteredPresentBoxes[i].presentToggle.isOn)
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
            if (filteredPresentBoxes[i].tardyToggle.isOn)
            {
                if (!teacherRosterAttendances[i].tardyList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].tardyList.Add(teacherId);
                }
            }
            else
            {
                if (teacherRosterAttendances[i].tardyList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].tardyList.Remove(teacherId);
                }
            }
            Database.instance.SaveDataToFirebase(teacherRosterAttendances[i]);
        }
    }
}