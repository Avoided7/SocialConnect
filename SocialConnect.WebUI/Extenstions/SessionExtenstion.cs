using System.Text.Json;

namespace SocialConnect.WebUI.Extenstions;

public static class SessionExtenstion
{
    public static void SetJson(this ISession sesion, string key, object value)
    {
        sesion.SetString(key, JsonSerializer.Serialize(value));
    }
    public static T? GetJson<T>(this ISession session, string key)
    {
        var result = session.GetString(key);
        return result == null ? default(T) : JsonSerializer.Deserialize<T>(result);
    }
}