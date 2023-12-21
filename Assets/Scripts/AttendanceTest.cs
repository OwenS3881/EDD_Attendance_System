using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AttendanceTest : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField classInput;
    [SerializeField] private TMP_InputField dateInput;
    [SerializeField] private Toggle presentToggle;

    [SerializeField] private TMP_InputField readInput;

    [SerializeField] private StudentRecord currentRecord;

    public void SaveAttendance()
    {
        Database.instance.ReadData(nameInput.text, new Database.ReadDataCallback<StudentRecord>(SaveAttendanceCallback));
    }

    private void SaveAttendanceCallback(StudentRecord output)
    {
        AttendanceRecord newRecord = new AttendanceRecord(classInput.text, dateInput.text, presentToggle.isOn);

        StudentRecord updatedRecord;

        if (output == null)
        {
            updatedRecord = new StudentRecord(nameInput.text, new List<AttendanceRecord>());
        }
        else
        {
            updatedRecord = new StudentRecord(output);
        }

        updatedRecord.AddRecord(newRecord);


        Database.instance.SaveDataToFirebase(updatedRecord);
    }

    public void ReadAttendance()
    {
        Database.instance.ReadData(readInput.text, new Database.ReadDataCallback<StudentRecord>(CallbackReadAttendance));
    }

    private void CallbackReadAttendance(StudentRecord output)
    {
        currentRecord = output;
    }
}
