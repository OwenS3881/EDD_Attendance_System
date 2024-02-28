using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavScreenManager : MonoBehaviour
{
    public void LoadTeachers()
    {
        SceneManager.LoadScene("TeacherLogin");
    }

    public void LoadAdmin()
    {
        SceneManager.LoadScene("AdminLogin");
    }

    public void LoadParents()
    {
        SceneManager.LoadScene("ParentLogin");
    }
}
