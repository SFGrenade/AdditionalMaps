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

class ActualLogAction : FsmStateAction
{
    public FsmString text;

    public override void Reset()
    {
        this.text = string.Empty;

        base.Reset();
    }

    public override void OnEnter()
    {
        if (!string.IsNullOrEmpty(this.text.Value))
        {
            Log($"FSM Log: \"{this.text.Value}\"");
            //printDebugFsm(this.Fsm);

        }
        base.Finish();
    }

    private void printDebugFsm(Fsm fsm)
    {
        foreach (var state in fsm.States)
        {
            Log("State: " + state.Name);
            foreach (var trans in state.Transitions)
            {
                Log("\t" + trans.EventName + " -> " + trans.ToState);
            }
        }
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