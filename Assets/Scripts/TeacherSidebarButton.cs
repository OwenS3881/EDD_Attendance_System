using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeacherSidebarButton : MonoBehaviour
{
    [SerializeField] private TeacherSidebar parentSidebar;
    [SerializeField] private Image bg;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_FontAsset selectedFont;
    [SerializeField] private TMP_FontAsset unselectedFont;
    [SerializeField] private Color selectedLabelColor;
    [SerializeField] private Color unselectedLabelColor;
    [SerializeField] private Color selectedBGColor;
    [SerializeField] private Color unselectedBGColor;
    [SerializeField] private GameObject linkedScreen;

    private bool selected;
    public bool Selected
    {
        get
        {
            return selected;
        }

        set
        {
            selected = value;

            if (selected)
            {
                bg.color = selectedBGColor;
                label.font = selectedFont;
                label.color = selectedLabelColor;
                if (linkedScreen != null) linkedScreen.SetActive(true);
            }
            else
            {
                bg.color = unselectedBGColor;
                label.font = unselectedFont;
                label.color = unselectedLabelColor;
                if (linkedScreen != null) linkedScreen.SetActive(false);
            }
        }
    }

    public void Select()
    {
        parentSidebar.UnselectAll();
        Selected = true;
    }
}
