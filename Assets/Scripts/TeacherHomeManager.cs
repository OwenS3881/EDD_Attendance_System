using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class TeacherHomeManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nameField;
    [SerializeField] private TMP_Text idField;
    [SerializeField] private TMP_Text emailField;

    private void Start()
    {
        idField.text = Database.instance.GetUsername();
        emailField.text = Database.instance.GetUserEmail() != null ? Database.instance.GetUserEmail() : "";
        GetName();
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

    private void GetName()
    {
        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<TeacherInfoData>(GetNameCallback));
    }

    private void GetNameCallback(TeacherInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        nameField.text = output.teacherName;
    }

    public void Logout()
    {
        Database.instance.LogoutUser();
        SceneManager.LoadScene("TeacherLogin");
    }
}
