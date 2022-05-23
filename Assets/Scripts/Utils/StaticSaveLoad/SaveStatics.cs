using System;
using System.Linq;
using UnityEngine;

namespace Assets.Utils.StaticSaveLoad
{
    class SavePropertyAttribute : Attribute {}
    
    static class SaveStatics
    {
        public static void Save(Type type)
        {
            var props = type.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(SavePropertyAttribute), false).Any());

            var typename = type.Name;
            foreach (var prop in props)
            {
                var saveName = $"{typename}.{prop.Name}";
                if (prop.PropertyType == typeof(float))
                    PlayerPrefs.SetFloat(saveName, (float)prop.GetValue(null));
                if (prop.PropertyType == typeof(double))
                    PlayerPrefs.SetFloat(saveName, (float)(double)prop.GetValue(null));
                if (prop.PropertyType == typeof(string))
                    PlayerPrefs.SetString(saveName, (string)prop.GetValue(null));
                if (prop.PropertyType == typeof(int))
                    PlayerPrefs.SetInt(saveName, (int)prop.GetValue(null));
            }
            PlayerPrefs.Save();
        }
        
        public static void Load(Type type)
        {
            var props = type.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(SavePropertyAttribute), false).Any());

            var typename = type.Name;
            foreach (var prop in props)
            {
                var saveName = $"{typename}.{prop.Name}";
                if (prop.PropertyType == typeof(float))
                    prop.SetValue(null, PlayerPrefs.GetFloat(saveName));
                if (prop.PropertyType == typeof(double))
                    prop.SetValue(null, (double)PlayerPrefs.GetFloat(saveName));
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(null, PlayerPrefs.GetString(saveName));
                if (prop.PropertyType == typeof(int))
                    prop.SetValue(null, PlayerPrefs.GetInt(saveName));
            }
        }
    }
}