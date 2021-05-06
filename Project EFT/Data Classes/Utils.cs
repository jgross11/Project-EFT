using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Project_EFT.Data_Classes
{
    /// <summary>Contains a multitude of helper / extension methods to be used for various purposes throughout the system.</summary>
    public static class Utils
    {
        /// <summary>Extension method that allows the storing of Objects in a <see cref="ISession"/> as JSON strings.</summary>
        /// <typeparam name="T">The type of the object that will be stored in the session - probably redundant.</typeparam>
        /// <param name="session">N/A, as this is an extension method.</param>
        /// <param name="keyName">The key in the session that references the value.</param>
        /// <param name="value">The value to store in the session.</param>
        public static void SetComplexObject<T>(this ISession session, string keyName, T value) 
        {
            session.SetString(keyName, JsonConvert.SerializeObject(value, Program.DerivedJSONSettings));
        }

        /// <summary>Extension method that allows the retrieval of Objects from their JSON string representation (probably stored via <see cref="SetComplexObject{T}(ISession, string, T)"/>.</summary>
        /// <typeparam name="T">The type of the object that will be retrieved from the session.</typeparam>
        /// <param name="session">N/A, as this is an extension method.</param>
        /// <param name="keyName">The key in the session that references the desired value.</param>
        /// <returns>The properly typed value stored in the session for the given key, if it exists. The default value of <typeparamref name="T"/> otherwise.</returns>
        public static T GetComplexObject<T>(this ISession session, string keyName)
        {
            string value = session.GetString(keyName);

            // insert hacker man + galaxy brain meme
            if (typeof(T) == typeof(string))
            {
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
            }

            return value == null ? default : JsonConvert.DeserializeObject<T>(value, Program.DerivedJSONSettings);
        }

        /// <summary>Determines if the caller session contains the given key.</summary>
        /// <param name="session">N/A, as this is an extension method.</param>
        /// <param name="keyName">The key to search for in the session.</param>
        /// <returns>True if the session contains a non-null value for this key. False otherwise.</returns>
        public static bool ContainsKey(this ISession session, string keyName)
        {
            return session.GetString(keyName) != null;
        }

        /// <summary>Attempts to retrieve and remove a value in the session by it's key, if it exists.</summary>
        /// <typeparam name="T">The type of the desired value.</typeparam>
        /// <param name="session">N/A, as this is an extension method.</param>
        /// <param name="keyName">The key whose value will be retrieved.</param>
        /// <returns>The properly typed value stored in the session for the given key, if it exists. The default value of <typeparamref name="T"/> otherwise.</returns> 
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
