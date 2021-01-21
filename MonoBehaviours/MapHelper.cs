using ModCommon;
using ModCommon.Util;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Logger = Modding.Logger;

namespace SFCore
{
    class MapHelper
    {
        /* 
         * ItemHelper
         * v 0.0.1.0
         */

        /* 
         * This HelperClass is currently set up to add only normal items, so not things like Rancid Eggs, Simple Keys and such.
         * 
         * Example inclusion in your mod's Initialize() function:
         * 
         */

        private void Log(string message)
        {
            Logger.Log($"[{GetType().FullName.Replace(".", "]:[")}] - {message}");
        }
        private void Log(object message)
        {
            Logger.Log($"[{GetType().FullName.Replace(".", "]:[")}] - {message.ToString()}");
        }
    }
}
