namespace salesngin.Services.Implementations;

public class Notification
{
    public void WriteMessage(string message)
    {
        Console.WriteLine($"MyDependency.WriteMessage called. Message: {message}");
    }

}

