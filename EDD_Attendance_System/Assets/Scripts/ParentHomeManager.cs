using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class ParentHomeManager : MonoBehaviour
{
    public static ParentHomeManager instance { get; private set; }

    [SerializeField] private TMP_Text nameField;
    [SerializeField] private TMP_Text idField;
    [SerializeField] private TMP_Text emailField;

    private StudentInfoData studentInfo;
    public StudentInfoData StudentInfo
    {
        get { return studentInfo; }
    }

    private SchoolInfoData schoolData;
    public SchoolInfoData SchoolData
    {
        get { return schoolData; }
    }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"Multiple {GetType()}s in the scene");
        }
    }

    private void Start()
    {
        idField.text = Database.instance.GetUsername().Substring(1);
        emailField.text = Database.instance.GetUserEmail() != null ? Database.instance.GetUserEmail() : "";
        GetStudentInfo();
    }

    private void GetStudentInfo()
    {
        DesktopGraphics.instance.Loading(true);
        Database.instance.ReadData(Database.instance.GetUsername().Substring(1), new Database.ReadDataCallback<StudentInfoData>(GetStudentInfoCallback));
    }

    private void GetStudentInfoCallback(StudentInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find student");
            return;
        }

        studentInfo = output;

        nameField.text = output.studentName;

        Database.instance.ReadData(studentInfo.schoolId.ToString(), new Database.ReadDataCallback<SchoolInfoData>(GetSchoolDataCallback));
    }

    private void GetSchoolDataCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find school");
            return;
        }

        schoolData = output;

        DesktopGraphics.instance.Loading(false);
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
        DesktopGraphics.instance.Loading(true);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        DesktopGraphics.instance.Loading(false);
        DesktopGraphics.instance.DisplayMessage("Password reset email sent, check your email for further instructions, then try logging in");
    }

    void OnError(PlayFabError error)
    {
        DesktopGraphics.instance.Loading(false);
        DesktopGraphics.instance.DisplayMessage("Error: " + error.GenerateErrorReport());
    }

    public void Logout()
    {
        Database.instance.LogoutUser();
        SceneManager.LoadScene("ParentLogin");
    }
}
