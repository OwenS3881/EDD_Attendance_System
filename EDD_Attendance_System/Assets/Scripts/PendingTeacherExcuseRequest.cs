using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PendingTeacherExcuseRequest : MonoBehaviour
{
    [SerializeField] private TMP_Text studentIdField;
    [SerializeField] private TMP_Text dateField;

    private AttendanceExcuseRequest requestData;

    public void AssignRequest(AttendanceExcuseRequest request)
    {
        requestData = request;

        studentIdField.text = request.studentId.ToString();
        dateField.text = DateButton.ConvertToNiceDate(request.date);
    }

    public void OnSelect()
    {
        TeacherExcuseRequestManager.instance.RequestSelected(requestData);
    }
}
