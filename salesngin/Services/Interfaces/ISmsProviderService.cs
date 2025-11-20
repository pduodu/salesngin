namespace salesngin.Services.Interfaces;

public interface ISmsProviderService
{
    Task<string> SendSmsAsync(string[] phoneNumbers, string message);
}
