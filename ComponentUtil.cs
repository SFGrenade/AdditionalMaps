
using System.Reflection;
using UnityEngine;
using System;

namespace AdditionalMaps
{
    public static class ComponentUtil
    {
        public static void CopyOnto<T>(this T comp, GameObject o) where T : Component
        {
            Type type = typeof(T);
            T newComp = o.AddComponent<T>();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(newComp, pinfo.GetValue(comp, null), null);
                    }
                    catch
                    {
                    }
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(newComp, finfo.GetValue(comp));
            }
        }
    }
}
