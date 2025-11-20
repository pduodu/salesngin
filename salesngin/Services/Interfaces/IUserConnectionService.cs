namespace salesngin.Services.Interfaces;

public interface IUserConnectionService
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string userId);
    string GetConnection(string userId);
}

