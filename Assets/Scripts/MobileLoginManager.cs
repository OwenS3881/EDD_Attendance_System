using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MobileLoginManager : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject[] otherScreens;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField idInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField forgotPasswordEmailInput;

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
        MobileGraphics.instance.DisplayMessage(message);
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

        var request = new LoginWithPlayFabRequest
        {
            Username = idInput.text,
            Password = passwordInput.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetUserAccountInfo = true }
        };
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnError);
        MobileGraphics.instance.Loading(true);
    }

    void OnLoginSuccess(LoginResult result)
    {
        MobileGraphics.instance.Loading(false);
        Database.instance.CurrentUser = result;
        SceneManager.LoadScene("MobileHome");
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
        MobileGraphics.instance.Loading(true);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        MobileGraphics.instance.Loading(false);
        OutputMessage("Password reset email sent, check your email for further instructions, then try logging in");
    }

    void OnError(PlayFabError error)
    {
        MobileGraphics.instance.Loading(false);
        OutputMessage("Error: " + error.GenerateErrorReport());
    }
}
