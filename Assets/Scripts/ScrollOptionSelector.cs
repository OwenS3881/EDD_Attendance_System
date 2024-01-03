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
}
