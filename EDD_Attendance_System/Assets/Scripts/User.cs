public class User : BasicData
{
    public string username;
    public string email;

    public User(string fileName, string username, string email)
    {
        this.fileName = fileName;
        this.username = username;
        this.email = email;
    }
}
