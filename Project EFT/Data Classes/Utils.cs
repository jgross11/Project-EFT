using System;
using System.Collections.Generic;
using System.Linq;
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
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }

        public static bool ContainsKey(this ISession session, string keyName)
        {
            return session.GetString(keyName) != null;
        }
    }
}
