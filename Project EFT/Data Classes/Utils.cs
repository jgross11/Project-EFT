using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Project_EFT.Data_Classes
{
    public static class Utils
    {
        public static void SetComplexObject<T>(this ISession session, string keyName, T value) 
        {
            session.SetString(keyName, JsonConvert.SerializeObject(value));
        }

        public static T GetComplexObject<T>(this ISession session, string keyName) 
        {
            string value = session.GetString(keyName);
            
            // insert hacker man + galaxy brain meme
            if (typeof(T) == typeof(string)) 
            {
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
            }

            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }

        public static bool ContainsKey(this ISession session, string keyName)
        {
            return session.GetString(keyName) != null;
        }

        public static T TryGetAndRemoveKey<T>(this ISession session, string keyName) 
        {
            if (session.ContainsKey(keyName))
            {
                T result = session.GetComplexObject<T>(keyName);
                session.Remove(keyName);
                return result;
            }
            else {
                return default;
            }
        }
    }
}
