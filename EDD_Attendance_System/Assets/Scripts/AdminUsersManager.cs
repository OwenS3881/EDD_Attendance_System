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

    [Header("Student View Fields")]
    [SerializeField] private GameObject studentViewParent;
    [SerializeField] private GameObject studentViewPrefab;
    [SerializeField] private GridLayoutGroup studentViewGrid;
    [SerializeField] private float studentViewParentDefaultHeight;
    private List<StudentInfoData> studentViewData = new List<StudentInfoData>();

    [Header("Teacher View Fields")]
    [SerializeField] private GameObject teacherViewParent;
    [SerializeField] private GameObject teacherViewPrefab;
    [SerializeField] private GridLayoutGroup teacherViewGrid;
    [SerializeField] private float teacherViewParentDefaultHeight;
    private List<TeacherInfoData> teacherViewData = new List<TeacherInfoData>();

    [Header("Delete Student Fields")]
    [SerializeField] private TMP_InputField deleteStudentInput;
    [SerializeField] private GameObject deleteStudentSureButton1;
    [SerializeField] private GameObject deleteStudentSureButton2;
    [SerializeField] private GameObject cancelDeleteButton;

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
        if (output == null)
        {
            Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<SchoolInfoData>(TeacherUpdateSchoolDataCallback));
        }
        else
        {
            DesktopGraphics.instance.Loading(false);
            DesktopGraphics.instance.DisplayMessage("Teacher ID already exists");
        }
    }

    private void TeacherUpdateSchoolDataCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("School not found");
            DesktopGraphics.instance.DisplayMessage("An error has occurred");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        AdminHomeManager.instance.currentData = output;

        AdminHomeManager.instance.currentData.teacherList.Add(Int32.Parse(teacherIdInput.text));
        
        Database.instance.SaveDataToFirebase(AdminHomeManager.instance.currentData);

        TeacherInfoData newTeacher = new TeacherInfoData(Int32.Parse(teacherIdInput.text), teacherNameInput.text, new List<ListWrapper<string>>(), Int32.Parse(Database.instance.GetUsername()));
        Database.instance.SaveDataToFirebase(newTeacher);

        DesktopGraphics.instance.Loading(false);
        DesktopGraphics.instance.DisplayMessage("Success");
        OnEnable();
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

        Database.instance.ReadData(studentIdInput.text, new Database.ReadDataCallback<StudentInfoData>(VerifyStudentCreateId));
    }

    private void VerifyStudentCreateId(StudentInfoData output)
    {
        if (output == null)
        {
            Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<SchoolInfoData>(StudentUpdateSchoolDataCallback));
        }
        else
        {
            DesktopGraphics.instance.Loading(false);
            DesktopGraphics.instance.DisplayMessage("Student ID already exists");
        }
    }

    private void StudentUpdateSchoolDataCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("School not found");
            DesktopGraphics.instance.DisplayMessage("An error has occurred");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        AdminHomeManager.instance.currentData = output;

        int[] teacherIds = new int[7];
        for (int i = 0; i < studentClassesInput.Length; i++)
        {
            if (string.IsNullOrEmpty(studentClassesInput[i].text))
            {
                DesktopGraphics.instance.DisplayMessage($"Please enter a teacher for period {i + 1}");
                DesktopGraphics.instance.Loading(false);
                return;
            }
            if (!AdminHomeManager.instance.currentData.teacherList.Contains(Int32.Parse(studentClassesInput[i].text)) && !Database.freePeriodIds.Contains(Int32.Parse(studentClassesInput[i].text)))
            {
                DesktopGraphics.instance.DisplayMessage($"Teacher for period {i + 1} does not exist");
                DesktopGraphics.instance.Loading(false);
                return;
            }
            teacherIds[i] = Int32.Parse(studentClassesInput[i].text);
        }

        int studentId = Int32.Parse(studentIdInput.text);
        string studentName = studentNameInput.text;

        StudentInfoData newStudent = new StudentInfoData(studentId, studentName, teacherIds, Int32.Parse(Database.instance.GetUsername()), new List<string>());
        Database.instance.SaveDataToFirebase(newStudent);

        AdminHomeManager.instance.currentData.studentList.Add(studentId);
        Database.instance.SaveDataToFirebase(AdminHomeManager.instance.currentData);

        StartCoroutine(AddStudentToRoster(studentId, teacherIds, 0));
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

        while ((index + 1) < Database.freePeriodIds.Count && Database.freePeriodIds.Contains(teacherIds[index + 1]))
        {
            index++;
        }

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

    //View Student Methods
    public void LoadStudentView()
    {
        DesktopGraphics.instance.Loading(true);

        ClearStudentViewContainer();

        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<SchoolInfoData>(LoadStudentViewCallback));
    }

    private void LoadStudentViewCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("School not found");
            DesktopGraphics.instance.DisplayMessage("An error has occurred");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        AdminHomeManager.instance.currentData = output;

        foreach (int student in AdminHomeManager.instance.currentData.studentList)
        {
            Database.instance.ReadData(student.ToString(), new Database.ReadDataCallback<StudentInfoData>(GetStudentCallback));
        }

    }

    private void GetStudentCallback(StudentInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("No student found");
            //dont't return, keep going
        }

        studentViewData.Add(output);

        DisplayStudentData();
    }

    private void DisplayStudentData()
    {
        if (studentViewData.Count < AdminHomeManager.instance.currentData.studentList.Count)
        {
            return;
        }

        studentViewData.Sort();

        foreach (StudentInfoData student in studentViewData)
        {
            StudentViewBox newBox = Instantiate(studentViewPrefab, studentViewParent.transform).GetComponent<StudentViewBox>();

            newBox.StudentName = student.studentName;
            newBox.Id = student.studentId.ToString();
        }

        UpdateStudentViewParentHeight();

        DesktopGraphics.instance.Loading(false);
    }

    private void ClearStudentViewContainer()
    {
        foreach (Transform t in studentViewParent.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(studentViewParent.transform)) continue;

            Destroy(t.gameObject);
        }

        studentViewData.Clear();

        UpdateStudentViewParentHeight();
    }

    private void UpdateStudentViewParentHeight()
    {
        studentViewParent.GetComponent<RectTransform>().sizeDelta = new Vector2(studentViewParent.GetComponent<RectTransform>().sizeDelta.x, studentViewParentDefaultHeight + (studentViewData.Count * (studentViewGrid.cellSize.y + studentViewGrid.spacing.y)));
    }

    //View Teacher Methods
    public void LoadTeacherView()
    {
        DesktopGraphics.instance.Loading(true);

        ClearTeacherViewContainer();

        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<SchoolInfoData>(LoadTeacherViewCallback));
    }

    private void LoadTeacherViewCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("School not found");
            DesktopGraphics.instance.DisplayMessage("An error has occurred");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        AdminHomeManager.instance.currentData = output;

        foreach (int teacher in AdminHomeManager.instance.currentData.teacherList)
        {
            Database.instance.ReadData(teacher.ToString(), new Database.ReadDataCallback<TeacherInfoData>(GetTeacherCallback));
        }

    }

    private void GetTeacherCallback(TeacherInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("No teacher found");
            //dont't return, keep going
        }

        teacherViewData.Add(output);

        DisplayTeacherData();
    }

    private void DisplayTeacherData()
    {
        if (teacherViewData.Count < AdminHomeManager.instance.currentData.teacherList.Count)
        {
            return;
        }

        teacherViewData.Sort();

        foreach (TeacherInfoData teacher in teacherViewData)
        {
            TeacherViewBox newBox = Instantiate(teacherViewPrefab, teacherViewParent.transform).GetComponent<TeacherViewBox>();

            newBox.TeacherName = teacher.teacherName;
            newBox.Id = teacher.teacherId.ToString();
        }

        UpdateTeacherViewParentHeight();

        DesktopGraphics.instance.Loading(false);
    }

    private void ClearTeacherViewContainer()
    {
        foreach (Transform t in teacherViewParent.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(teacherViewParent.transform)) continue;

            Destroy(t.gameObject);
        }

        teacherViewData.Clear();

        UpdateTeacherViewParentHeight();
    }

    private void UpdateTeacherViewParentHeight()
    {
        teacherViewParent.GetComponent<RectTransform>().sizeDelta = new Vector2(teacherViewParent.GetComponent<RectTransform>().sizeDelta.x, teacherViewParentDefaultHeight + (teacherViewData.Count * (teacherViewGrid.cellSize.y + teacherViewGrid.spacing.y)));
    }

    //Delete Student Methods
    public void CancelDelete()
    {
        deleteStudentSureButton1.SetActive(false);
        deleteStudentSureButton2.SetActive(false);
        cancelDeleteButton.SetActive(false);
    }

    public void InitiateDelete()
    {
        Debug.Log("Deleting...");

        /*
         * Areas where student needs to be deleted:
         *      School's studentList
         *      Student's attendanceObjects
         *      Student's Teachers' rosters
         *      Student object itself
        */

        if (string.IsNullOrEmpty(deleteStudentInput.text))
        {
            DesktopGraphics.instance.DisplayMessage("Enter a Student ID");
            CancelDelete();
            return;
        }

        DesktopGraphics.instance.Loading(true);

        Database.instance.ReadData(deleteStudentInput.text, new Database.ReadDataCallback<StudentInfoData>(DeleteStudentCallback));
    }

    private void DeleteStudentCallback(StudentInfoData output)
    {
        if (output == null)
        {
            DesktopGraphics.instance.DisplayMessage("Student ID does not exist");
            DesktopGraphics.instance.Loading(false);
            CancelDelete();
            return;
        }

        //Clearing attendance objects
        foreach (string attendanceObject in output.attendanceObjects)
        {
            Database.instance.DeleteData(attendanceObject);
        }

        //Clear from rosters
        for (int i = 0; i < output.classList.Length; i++)
        {
            Database.instance.ReadData(output.classList[i].ToString(), new Database.ReadDataCallbackParams<TeacherInfoData>(DeleteStudentFromRosterCallback), new object[] { i });
        }
    }

    private void DeleteStudentFromRosterCallback(TeacherInfoData output, object[] additionalParams)
    {
        int index = (int)additionalParams[0];

        output.roster[index].internalList.Remove(deleteStudentInput.text);

        if (output.roster[index].internalList.Count <= 0)
        {
            output.roster[index].Add("Default");
        }

        Database.instance.SaveDataToFirebase(output);
    }
}
