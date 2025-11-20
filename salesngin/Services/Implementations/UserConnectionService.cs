namespace salesngin.Services.Implementations;

public class UserConnectionService : IUserConnectionService
{
    private readonly Dictionary<string, string> userConnections = new Dictionary<string, string>();

    public void AddConnection(string userId, string connectionId)
    {
        userConnections[userId] = connectionId;
    }

    public void RemoveConnection(string userId)
    {
        if (userConnections.ContainsKey(userId))
        {
            userConnections.Remove(userId);
        }
    }

    public string GetConnection(string userId)
    {
        return userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
    }

}

