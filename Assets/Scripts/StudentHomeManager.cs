using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudentHomeManager : MonoBehaviour
{
    public void ScanQRCode()
    {
        SceneManager.LoadScene("QRScanner");
    }

    public void ViewAttendance()
    {
        SceneManager.LoadScene("MobileViewAttendance");
    }

    public void ViewAccountInfo()
    {
        SceneManager.LoadScene("MobileAccountInfo");
    }

    public void Logout()
    {
        Database.instance.LogoutUser();
        SceneManager.LoadScene("MobileLogin");
    }
}
