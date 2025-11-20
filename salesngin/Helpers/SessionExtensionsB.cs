using Newtonsoft.Json;

namespace salesngin.Helpers
{
    public static class SessionExtensionsB
    {
        public static void SetJsonB(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetJsonB<T>(this ISession session, string key)
        {
            var sessionData = session.GetString(key);
            return sessionData == null ? default : JsonConvert.DeserializeObject<T>(sessionData);
        }
    }
}
