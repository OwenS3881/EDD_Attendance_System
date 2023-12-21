using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StudentRecord : BasicData
{
    public List<AttendanceRecord> records;

    public StudentRecord(string fileName, List<AttendanceRecord> records)
    {
        this.fileName = fileName;
        this.records = records;
    }

    public StudentRecord(string fileName, AttendanceRecord record)
    {
        this.fileName = fileName;
        this.records = new List<AttendanceRecord>();
        AddRecord(record);
    }

    public StudentRecord(StudentRecord clone)
    {
        this.fileName = clone.fileName;
        this.records = new List<AttendanceRecord>(clone.records);
    }

    public void AddRecord(AttendanceRecord newRecord)
    {
        records.Add(newRecord);
    }
}
