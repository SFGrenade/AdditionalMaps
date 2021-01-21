using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using UnityEngine;
using GlobalEnums;
using Logger = Modding.Logger;
using ModCommon.Util;
using System.IO;
using Modding;
using ModCommon;
using UnityEngine.UI;
using UnityEngine.Audio;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Security.Cryptography;
using AdditionalMaps.Consts;
using AdditionalMaps.Utils;
using SFCore;
using TMPro;
using On;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using ReflectionHelper = Modding.ReflectionHelper;

class ActualActionAction : FsmStateAction
{
    public Action<GameObject, string> action;
    public GameObject go;
    public FsmString fsmString;

    public override void Reset()
    {
        action = null;

        base.Reset();
    }

    public override void OnEnter()
    {
        if (action != null)
        {
            //Log($"Invoked with values \"{go}\" & \"{fsmString.Value}\"");
            action.Invoke(go, fsmString.Value);
        }
        base.Finish();
    }

    new private void Log(string message)
    {
        Logger.Log($"[{GetType().FullName.Replace(".", "]:[")}] - {message}");
    }
    private void Log(object message)
    {
        Logger.Log($"[{GetType().FullName.Replace(".", "]:[")}] - {message.ToString()}");
    }
}