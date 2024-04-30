using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class StudentAccountInfoManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nameField;
    [SerializeField] private TMP_Text idField;
    [SerializeField] private TMP_Text emailField;
    [SerializeField] private GameObject classListContentParent;
    [SerializeField] private GameObject classListEntryPrefab;
    private StudentInfoData currentDayStudent;

    private void Start()
    {
        idField.text = Database.instance.GetUsername();
        emailField.text = Database.instance.GetUserEmail() != null ? Database.instance.GetUserEmail() : "";
        GetSchedule();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MobileHome");
    }

    //Change Password
    public void ResetPassword()
    {

        var request = new SendAccountRecoveryEmailRequest
        {
            Email = Database.instance.GetUserEmail(),
            TitleId = "8C68F"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
        MobileGraphics.instance.Loading(true);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        MobileGraphics.instance.Loading(false);
        MobileGraphics.instance.DisplayMessage("Password reset email sent, check your email for further instructions, then try logging in");
    }

    void OnError(PlayFabError error)
    {
        MobileGraphics.instance.Loading(false);
        MobileGraphics.instance.DisplayMessage("Error: " + error.GenerateErrorReport());
    }

    //Get Schedule
    public void GetSchedule()
    {
        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<StudentInfoData>(GetScheduleCallback));
        MobileGraphics.instance.Loading(true);
    }

    private void GetScheduleCallback(StudentInfoData output)
    {

        if (output == null)
        {
            Debug.LogWarning("Couldn't find StudentInfo");
            return;
        }

        currentDayStudent = output;
        FoundStudent();
    }

    private void FoundStudent()
    {
        MobileGraphics.instance.Loading(false);

        nameField.text = currentDayStudent.studentName;

        foreach (Transform child in classListContentParent.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 7; i++)
        {
            GameObject classListEntry = Instantiate(classListEntryPrefab, classListContentParent.transform);

            classListEntry.GetComponentInChildren<TMP_Text>().text = currentDayStudent.classList[i];
            AddTeacherNameToIDField(currentDayStudent.classList[i], classListEntry.GetComponentInChildren<TMP_Text>(), i + 1);
        }
    }

    private void AddTeacherNameToIDField(string teacherId, TMP_Text field, int period)
    {
        Database.instance.ReadData(teacherId, new Database.ReadDataCallbackParams<TeacherInfoData>(AddTeacherNameToIDFieldCallback), new object[] { field, period });
    }

    private void AddTeacherNameToIDFieldCallback(TeacherInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        TMP_Text field = (TMP_Text)additionalParams[0];
        int period = (int)additionalParams[1];

        field.text = "Period " + period + " - " + output.teacherId + " - " + output.teacherName;
    }
}
