using System.Reflection;
using UnityEngine;

namespace AdditionalMaps
{
    public static class ComponentUtil
    {
        public static void CopyOnto<T>(this T comp, GameObject o) where T : Component
        {
            var type = typeof(T);
            var newComp = o.AddComponent<T>();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var pInfos = type.GetProperties(flags);
            foreach (var pInfo in pInfos)
            {
                if (!pInfo.CanWrite) continue;
                try
                {
                    pInfo.SetValue(newComp, pInfo.GetValue(comp, null), null);
                }
                catch
                {
                    // ignored
                }
            }
            var fInfos = type.GetFields(flags);
            foreach (var fInfo in fInfos)
            {
                fInfo.SetValue(newComp, fInfo.GetValue(comp));
            }
        }
    }
}
