using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollOptionSelector : MonoBehaviour
{
    [SerializeField] private GameObject contentParent;
    [SerializeField] private float distanceBetweenOptions;
    public int selectedIndex;

    private void Start()
    {
        UpdateIndex();
    }

    public void UpdateIndex()
    {
        contentParent.transform.GetChild(selectedIndex).GetComponent<Image>().enabled = false;

        selectedIndex = (int)(contentParent.transform.localPosition.y / distanceBetweenOptions);

        selectedIndex = Mathf.Clamp(selectedIndex, 0, contentParent.transform.childCount - 1);

        contentParent.transform.GetChild(selectedIndex).GetComponent<Image>().enabled = true;
    }

    public string GetSelectedOption()
    {
        return contentParent.transform.GetChild(selectedIndex).GetComponentInChildren<TMP_Text>().text;
    }

    public void SetSelectedOption(string option)
    {
        //find object
        Transform selectedObject = null;

        for (int i = 0; i < contentParent.transform.childCount; i++)
        {
            if (contentParent.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text.Equals(option))
            {
                selectedObject = contentParent.transform.GetChild(i);
                break;
            }
        }

        if (selectedObject == null)
        {
            Debug.LogError("Couldn't find option: " + option);
            return;
        }

        //get position
        float yPos = selectedObject.localPosition.y;
        yPos += contentParent.GetComponent<GridLayoutGroup>().padding.top;
        yPos += contentParent.GetComponent<GridLayoutGroup>().cellSize.y / 2;
        yPos *= -1;

        //set position
        contentParent.transform.localPosition = new Vector3(0, yPos, 0);

        //update index
        UpdateIndex();
    }
}
