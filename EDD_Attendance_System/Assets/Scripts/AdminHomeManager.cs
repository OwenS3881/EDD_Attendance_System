using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class AdminHomeManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_Text idField;
    [SerializeField] private TMP_Text emailField;

    public SchoolInfoData currentData;

    public static AdminHomeManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple " + GetType() + "s in the scene");
        }
    }

    private void Start()
    {
        idField.text = Database.instance.GetUsername();
        emailField.text = Database.instance.GetUserEmail() != null ? Database.instance.GetUserEmail() : "";
        GetSchoolData();
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

    public void GetSchoolData()
    {
        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<SchoolInfoData>(GetSchoolDataCallback));
        DesktopGraphics.instance.Loading(true);
    }

    private void GetSchoolDataCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find school");
            return;
        }

        currentData = output;

        nameField.text = currentData.schoolName;

        DesktopGraphics.instance.Loading(false);
    }

    //Only meant to be called if free periods are removed from database
    //Should not be called during user use, only during dev use
    private void InitializeFreePeriods()
    {

        foreach (string i in Database.freePeriodIds)
        {
            TeacherInfoData freePeriod = new TeacherInfoData(i, $"FREE-{i}", new List<ListWrapper<string>>(),"0");
            Database.instance.SaveDataToFirebase(freePeriod);
        }
    }

    public void UpdateName()
    {
        currentData.schoolName = nameField.text;
        Database.instance.SaveDataToFirebase(currentData);
        DesktopGraphics.instance.DisplayMessage("Success");
    }

    public void Logout()
    {
        Database.instance.LogoutUser();
        SceneManager.LoadScene("AdminLogin");
    }

    public static List<int> GetPeriods(string date, SchoolInfoData schoolInfo)
    {
        //check for overrides
        foreach (ScheduledPeriods sp in schoolInfo.scheduleOverrides)
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
        foreach (ScheduledPeriods sp in schoolInfo.blockSchedule)
        {
            if (sp.date.ToLower().Equals(dayOfWeek))
            {
                return sp.periods;
            }
        }

        return new List<int>();
    }
}
