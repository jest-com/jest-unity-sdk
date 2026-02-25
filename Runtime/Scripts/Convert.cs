using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides utility methods for converting between different data types and their string representations.
    /// </summary>
    public static class Convert
    {
        /// <summary>
        /// Converts an object to its string representation based on its type.
        /// </summary>
        /// <typeparam name="T">The type of object to convert.</typeparam>
        /// <param name="obj">The object to convert to a string.</param>
        /// <returns>A string representation of the object.</returns>
        public static string ToString<T>(T obj)
        {
            var type = typeof(T);
            if (type.IsPrimitive || obj is Vector2 || obj is Vector3)
            {
                return obj.ToString();
            }
            else if (obj is Dictionary<string, object>)
            {
                return JsonConvert.SerializeObject(obj);
            }
            else
            {
                return JsonUtility.ToJson(obj);
            }
        }

        /// <summary>
        /// Converts a string value to a specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the string to.</typeparam>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The converted value of type T.</returns>
        public static T FromString<T>(string value)
        {
            System.Type type = typeof(T);
            return type switch
            {
                System.Type t when t == typeof(int) => (T)(object)int.Parse(value),
                System.Type t when t == typeof(float) => (T)(object)float.Parse(value),
                System.Type t when t == typeof(bool) => (T)(object)bool.Parse(value),
                System.Type t when t == typeof(System.Enum) => (T)System.Enum.Parse(type, value),
                System.Type t when t == typeof(Vector3) => (T)(object)ParseVector3(value),
                System.Type t when t == typeof(Vector2) => (T)(object)ParseVector2(value),
                System.Type t when t == typeof(string) => (T)(object)value,
                System.Type t when t == typeof(Dictionary<string, object>) => (T)(object)DeserializeDictionary(value),
                _ => JsonUtility.FromJson<T>(value)
            };
        }

        private static Vector3 ParseVector3(string value)
        {
            var components = SplitIntoComponents(value, 3);
            return new Vector3(components[0], components[1], components[2]);
        }

        private static Vector2 ParseVector2(string value)
        {
            var components = SplitIntoComponents(value, 2);
            return new Vector2(components[0], components[1]);
        }

        private static float[] SplitIntoComponents(string value, int componentCount)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new System.ArgumentException($"Cannot parse null or empty string as Vector{componentCount}");
            }

            // Remove the parentheses and split the string
            string[] components = value.Replace("(", "").Replace(")", "").Split(',');

            if (components.Length != componentCount)
            {
                throw new System.ArgumentException(
                    $"Expected {componentCount} components but found {components.Length} in '{value}'. " +
                    $"Format must be (x,y{(componentCount == 3 ? ",z" : "")})");
            }

            var result = new float[componentCount];
            for (int i = 0; i < componentCount; i++)
            {
                string trimmed = components[i].Trim();
                if (!float.TryParse(trimmed, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out result[i]))
                {
                    throw new System.ArgumentException(
                        $"Cannot parse component {i} ('{components[i]}') as float in '{value}'");
                }
            }

            return result;
        }

        private static Dictionary<string, object> DeserializeDictionary(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, object>();
            }

            var result = JsonConvert.DeserializeObject(json, typeof(Dictionary<string, object>)) as
             Dictionary<string, object>;

            if (result == null)
            {
                UnityEngine.Debug.LogWarning($"[JestSDK] Failed to deserialize dictionary from JSON: {json}");
                return new Dictionary<string, object>();
            }

            foreach (var key in result.Keys.ToList())
            {
                var value = result[key];
                if (value is JObject jObj)
                {
                    result[key] = DeserializeDictionary(jObj.ToString());
                }
            }
            return result;
        }

    }
}
