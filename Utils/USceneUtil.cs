using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using GlobalEnums;
using On;
using Logger = Modding.Logger;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using ModCommon.Util;
using HutongGames.PlayMaker.Actions;
using UScene = UnityEngine.SceneManagement.Scene;

namespace AdditionalMaps.Utils
{
    public static class USceneUtil
    {
        public static GameObject FindRoot(this Scene scene, string name)
        {
            if (scene.IsValid())
            {
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.name == name)
                    {
                        return go;
                    }
                }
            }
            return null;
        }
        public static GameObject Find(this Scene scene, string name)
        {
            if (scene.IsValid())
            {
                GameObject retGo;
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.name == name)
                    {
                        return go;
                    }
                    retGo = go.Find(name);
                    if (retGo != null)
                    {
                        return retGo;
                    }
                }
            }
            return null;
        }
        public static GameObject Find(this GameObject root, string name)
        {
            Transform c = null;
            for (int i = 0; i < root.transform.childCount; i++)
            {
                c = root.transform.GetChild(i);
                if ((c != null) && (c.gameObject.name == name))
                {
                    return c.gameObject;
                }
                else if (c != null)
                {
                    return c.gameObject.Find(name);
                }
            }
            return null;
        }

        public static void Log(this Scene scene)
        {
            Logger.Log($"[SceneLog] - Scene \"{scene.name}\"");
            foreach (var go in scene.GetRootGameObjects())
            {
                go.transform.Log();
            }
        }
        public static void Log(this Transform go, string n = "\t")
        {
            Transform c;
            Logger.Log($"[SceneLog] - {n}\"{go.name}\"");
            foreach (var comp in go.GetComponents<Component>())
            {
                Logger.Log($"[SceneLog] - {n} => \"{comp.GetType()}\": {comp.ToString()}");
            }
            for (int i = 0; i < go.childCount; i++)
            {
                go.GetChild(i).Log($"{n}\t");
            }
        }

        public static UScene GetScene(string name)
        {
            UScene scene = default;
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                if (scene.name == name)
                {
                    return scene;
                }
            }
            return default;
        }
    }
}
