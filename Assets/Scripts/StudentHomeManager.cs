using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudentHomeManager : MonoBehaviour
{
    private void Start()
    {
        //This is run to verify whether or not the user is logged in
        //If this returns null, the login scene is automatically loaded
        Database.instance.GetUsername();
    }

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

    public void ExcuseRequest()
    {
        SceneManager.LoadScene("MobileExcuseRequest");
    }

    public void FreePeriod()
    {
        SceneManager.LoadScene("MobileFreePeriod");
    }

    public void Logout()
    {
        Database.instance.LogoutUser();
        SceneManager.LoadScene("MobileLogin");
    }
}
