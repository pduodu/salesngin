
namespace salesngin.Extensions;
public static class SessionExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static void SetJson<T>(this ISession session, string key, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        session.SetString(key, json);
    }

    public static T GetJson<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        return json is null ? default! : JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
    }
}
