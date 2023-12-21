[System.Serializable]
public class AttendanceRecord
{
    public string className;
    public string date;
    public bool present;

    public AttendanceRecord(string className, string date, bool present)
    {
        this.className = className;
        this.date = date;
        this.present = present;
    }
}
