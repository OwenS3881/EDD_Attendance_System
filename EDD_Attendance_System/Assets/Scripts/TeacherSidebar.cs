using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeacherSidebar : MonoBehaviour
{
    [SerializeField] private TeacherSidebarButton[] buttons;

    private void Start()
    {
        buttons[0].Select();
    }

    public void UnselectAll()
    {
        foreach (TeacherSidebarButton button in buttons)
        {
            button.Selected = false;
        }
    }
}
