using System.Collections.Concurrent;

namespace HZC.MyOrm.Reflections
{
    public class MyEntityMapperContainer
    {
        private static readonly ConcurrentDictionary<string, string> Dict 
            = new ConcurrentDictionary<string, string>();

        public static string Get(string key)
        {
            if (Dict.TryGetValue(key, out var field))
            {
                return field;
            }

            return string.Empty;
        }

        public static void Add(string key, string value)
        {
            Dict.TryAdd(key, value);
        }
    }
}
