using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class AdminLoginManager : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject[] otherScreens;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField idInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField forgotPasswordEmailInput;
    [SerializeField] private TMP_InputField createIdInput;
    [SerializeField] private TMP_InputField createPasswordInput;
    [SerializeField] private TMP_InputField createPasswordConfirmInput;
    [SerializeField] private TMP_InputField createEmailInput;

    private void Start()
    {
        mainScreen.SetActive(true);
        foreach (GameObject screen in otherScreens)
        {
            screen.SetActive(false);
        }
    }

    public void OutputMessage(string message)
    {
        DesktopGraphics.instance.DisplayMessage(message);
    }

    public void Login()
    {
        if (string.IsNullOrEmpty(idInput.text))
        {
            OutputMessage("Please enter an ID");
            return;
        }

        if (string.IsNullOrEmpty(passwordInput.text))
        {
            OutputMessage("Please enter a password");
            return;
        }

        Database.instance.ReadData(idInput.text, new Database.ReadDataCallback<SchoolInfoData>(VerifyLoginId));
        DesktopGraphics.instance.Loading(true);
    }

    private void VerifyLoginId(SchoolInfoData output)
    {
        if (output == null || string.IsNullOrEmpty(output.schoolName))
        {
            OutputMessage("School ID does not exist");
            DesktopGraphics.instance.Loading(false);
            return;
        }


        var request = new LoginWithPlayFabRequest
        {
            Username = idInput.text,
            Password = passwordInput.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetUserAccountInfo = true }
        };
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        DesktopGraphics.instance.Loading(false);
        Database.instance.CurrentUser = result;
        SceneManager.LoadScene("AdminHome");
    }

    public void ResetPassword()
    {
        if (string.IsNullOrEmpty(forgotPasswordEmailInput.text))
        {
            OutputMessage("Please enter an email address");
            return;
        }

        var request = new SendAccountRecoveryEmailRequest
        {
            Email = forgotPasswordEmailInput.text,
            TitleId = "8C68F"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
        DesktopGraphics.instance.Loading(true);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        DesktopGraphics.instance.Loading(false);
        OutputMessage("Password reset email sent, check your email for further instructions, then try logging in");
    }

    public void RegisterAccount()
    {
        if (string.IsNullOrEmpty(createIdInput.text))
        {
            OutputMessage("Please enter an ID");
            return;
        }

        if (string.IsNullOrEmpty(createPasswordInput.text))
        {
            OutputMessage("Please enter a password");
            return;
        }

        if (string.IsNullOrEmpty(createPasswordConfirmInput.text))
        {
            OutputMessage("Please confirm your password");
            return;
        }

        if (!createPasswordInput.text.Equals(createPasswordConfirmInput.text))
        {
            OutputMessage("Passwords do not match");
            return;
        }

        if (string.IsNullOrEmpty(createEmailInput.text))
        {
            OutputMessage("Please enter an email");
            return;
        }

        Database.instance.ReadData(createIdInput.text, new Database.ReadDataCallback<SchoolInfoData>(VerifyCreateId));
        DesktopGraphics.instance.Loading(true);
    }

    private void VerifyCreateId(SchoolInfoData output)
    {
        if (output != null)
        {
            OutputMessage("ID already exists");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest
        {
            Username = createIdInput.text,
            Password = createPasswordInput.text,
            Email = createEmailInput.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        SchoolInfoData newSchool = new SchoolInfoData(createIdInput.text, "New School", new List<string>(), new List<string>(), new List<ScheduledPeriods>(), new List<ScheduledPeriods>(), new List<AttendanceExcuseRequest>());
        Database.instance.SaveDataToFirebase(newSchool);
        OutputMessage("Account regsitered successfully!");
        DesktopGraphics.instance.Loading(false);
    }

    void OnError(PlayFabError error)
    {
        DesktopGraphics.instance.Loading(false);
        OutputMessage("Error: " + error.GenerateErrorReport());
    }

    public void ReturnToNav()
    {
        SceneManager.LoadScene("NavScreen");
    }
}
