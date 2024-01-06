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

        Database.instance.ReadData(createIdInput.text, new Database.ReadDataCallback<StudentInfoData>(VerifyCreateId));
        MobileGraphics.instance.Loading(true);
    }

    private void VerifyCreateId(StudentInfoData output)
    {
        if (output == null)
        {
            OutputMessage("Student ID does not exist");
            MobileGraphics.instance.Loading(false);
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
        OutputMessage("Account regsitered successfully!");
        MobileGraphics.instance.Loading(false);
    }

    void OnError(PlayFabError error)
    {
        MobileGraphics.instance.Loading(false);
        OutputMessage("Error: " + error.GenerateErrorReport());
    }
}
