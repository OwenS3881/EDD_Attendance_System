using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StudentFreePeriodManager : MonoBehaviour
{
    private StudentInfoData studentInfo;

    private void Start()
    {
        GetData();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MobileHome");
    }

    private void GetData()
    {
        MobileGraphics.instance.Loading(true);
        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<StudentInfoData>(GetStudentDataCallback));
    }

    private void GetStudentDataCallback(StudentInfoData output)
    {
        if (output == null)
        {
            MobileGraphics.instance.Loading(false);
            MobileGraphics.instance.DisplayMessage("An error has occurred");
            Debug.LogWarning("Couldn't find StudentInfo");
            return;
        }

        studentInfo = output;

        MobileGraphics.instance.Loading(false);
    }
}
