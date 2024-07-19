using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SFCore.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using MLogger = Modding.Logger;
using ULogger = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace AdditionalMaps.MonoBehaviours;

public struct SCustomArea
{
    public GameObject AreaGameObject;
    public Vector3 CameraPosition;
    public string PlayerDataBoolGotAreaMap;
    public List<string> MapZoneStrings;
    public float? panMinX;
    public float? panMaxX;
    public float? panMinY;
    public float? panMaxY;
}

public class ActionArg
{
    public GameObject Go;
    public HutongGames.PlayMaker.FsmString FsmString;
}

public static class GameMapHooks
{
    private static readonly Dictionary<string, SCustomArea> CustomAreas = new();

    private static readonly List<Func<GameMap, Dictionary<string, SCustomArea>>> Callbacks = new();

    public static void Init(Func<GameMap, Dictionary<string, SCustomArea>> callback = null)
    {
        try
        {
            On.GameManager.SetGameMap -= OnGameManagerSetGameMap;
        }
        catch
        {
        }
        finally
        {
            On.GameManager.SetGameMap += OnGameManagerSetGameMap;
        }

        if (callback != null) Callbacks.Add(callback);
    }

    private static void MakeHooks()
    {
        try
        {
            IL.GameMap.WorldMap -= IlWorldMap;
        }
        catch
        {
        }
        finally
        {
            IL.GameMap.WorldMap += IlWorldMap;
        }
        try
        {
            On.GameMap.QuickMapAncientBasin -= NewQuickMapAncientBasin;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapAncientBasin += NewQuickMapAncientBasin;
        }

        try
        {
            On.GameMap.QuickMapCity -= NewQuickMapCity;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapCity += NewQuickMapCity;
        }

        try
        {
            On.GameMap.QuickMapCliffs -= NewQuickMapCliffs;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapCliffs += NewQuickMapCliffs;
        }

        try
        {
            On.GameMap.QuickMapCrossroads -= NewQuickMapCrossroads;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapCrossroads += NewQuickMapCrossroads;
        }

        try
        {
            On.GameMap.QuickMapCrystalPeak -= NewQuickMapCrystalPeak;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapCrystalPeak += NewQuickMapCrystalPeak;
        }

        try
        {
            On.GameMap.QuickMapDeepnest -= NewQuickMapDeepnest;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapDeepnest += NewQuickMapDeepnest;
        }

        try
        {
            On.GameMap.QuickMapFogCanyon -= NewQuickMapFogCanyon;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapFogCanyon += NewQuickMapFogCanyon;
        }

        try
        {
            On.GameMap.QuickMapFungalWastes -= NewQuickMapFungalWastes;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapFungalWastes += NewQuickMapFungalWastes;
        }

        try
        {
            On.GameMap.QuickMapGreenpath -= NewQuickMapGreenpath;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapGreenpath += NewQuickMapGreenpath;
        }

        try
        {
            On.GameMap.QuickMapKingdomsEdge -= NewQuickMapKingdomsEdge;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapKingdomsEdge += NewQuickMapKingdomsEdge;
        }

        try
        {
            On.GameMap.QuickMapQueensGardens -= NewQuickMapQueensGardens;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapQueensGardens += NewQuickMapQueensGardens;
        }

        try
        {
            On.GameMap.QuickMapRestingGrounds -= NewQuickMapRestingGrounds;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapRestingGrounds += NewQuickMapRestingGrounds;
        }

        try
        {
            On.GameMap.QuickMapDirtmouth -= NewQuickMapDirtmouth;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapDirtmouth += NewQuickMapDirtmouth;
        }

        try
        {
            On.GameMap.QuickMapWaterways -= NewQuickMapWaterways;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.QuickMapWaterways += NewQuickMapWaterways;
        }

        try
        {
            On.GameMap.CloseQuickMap -= NewCloseQuickMap;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.CloseQuickMap += NewCloseQuickMap;
        }

        try
        {
            IL.GameMap.PositionCompass -= IlPositionCompass;
        }
        catch
        {
        }
        finally
        {
            IL.GameMap.PositionCompass += IlPositionCompass;
        }
    }

    private static void OnGameManagerSetGameMap(On.GameManager.orig_SetGameMap orig, GameManager self,
        GameObject goGameMap)
    {
        ReplaceComponent(goGameMap);
        orig(self, goGameMap);
    }

    public static void ReplaceComponent(GameObject gameMapGameObject)
    {
        Log("!ReplaceComponent");

        if (gameMapGameObject == null)
        {
            Log("~ReplaceComponent");
            return;
        }

        Log("Got " + Callbacks.Count + " callbacks!");
        foreach (var tmp in Callbacks)
        {
            Object.DontDestroyOnLoad(gameMapGameObject);
            var callbackAreas = tmp.Invoke(gameMapGameObject.GetComponent<GameMap>());
            Log("Got " + callbackAreas.Count + " custom Areas!");
            foreach (var (areaKey, areaStruct) in callbackAreas)
            {
                Log("Name: " + areaKey);
                CustomAreas[areaKey] = areaStruct;
                Object.DontDestroyOnLoad(areaStruct.AreaGameObject);
                areaStruct.AreaGameObject.transform.SetParent(gameMapGameObject.transform);
            }
        }

        ReplaceFsmQuickMap(gameMapGameObject);

        MakeHooks();

        Log("~ReplaceComponent");
    }

    public static void ReplaceFsmQuickMap(GameObject gameMapGameObject)
    {
        Log("!ReplaceFsmQuickMap");

        #region Make transform changes of custom areas

        foreach (var pair in CustomAreas) pair.Value.AreaGameObject.transform.SetParent(gameMapGameObject.transform);

        #endregion

        var quickMapGameObject = GameObject.Find("Quick Map");
        var quickMapFsm = quickMapGameObject.LocateMyFSM("Quick Map");

        #region Add fsm string variable

        var quickMapFsmVars = quickMapFsm.FsmVariables;

        #endregion

        #region Make States & Transitions

        //quickMapFsm.InsertAction("Check Area", new ActualLogAction { text = quickMapFsmVars.FindFsmString("Map Zone") }, 0);

        foreach (var pair in CustomAreas)
        {
            Log($"Adding state {pair.Key}");

            // Make State
            const string prefabState = "Cliffs";
            quickMapFsm.CopyState(prefabState, pair.Key);
            quickMapFsm.GetAction<PlayerDataBoolTest>(pair.Key, 0).boolName = pair.Value.PlayerDataBoolGotAreaMap;
            quickMapFsm.GetAction<GetLanguageString>(pair.Key, 1).convName = pair.Key;
            quickMapFsm.GetAction<SetPosition>(pair.Key, 5).x = pair.Value.CameraPosition.x;
            quickMapFsm.GetAction<SetPosition>(pair.Key, 5).y = pair.Value.CameraPosition.y;
            quickMapFsm.GetAction<SetPosition>(pair.Key, 5).z = pair.Value.CameraPosition.z;

            #region Make Action

            //CallMethodProper fsmCMPAction = new CallMethodProper();
            //fsmCMPAction.gameObject = tmpGameObjectOwner;
            //fsmCMPAction.behaviour = "GameMap";
            //fsmCMPAction.methodName = "QuickMapNew";
            //fsmCMPAction.parameters = new FsmVar[] { new FsmVar() { NamedVar = quickMapFsmVars.FindFsmString("Map Zone") } };
            //fsmCMPAction.storeResult = new FsmVar() { NamedVar = quickMapFsmVars.FindFsmBool("Check Area Return") };

            var fsmAaAction = new FunctionAction<ActionArg>
            {
                arg = new ActionArg
                {
                    Go = gameMapGameObject, FsmString = quickMapFsmVars.FindFsmString("Map Zone")
                },
                action = QuickMapNew
            };

            //quickMapFsm.InsertAction(pair.Key, fsmAaAction, prefabState == "Cliffs" ? 5 : 6);
            quickMapFsm.InsertAction(pair.Key, fsmAaAction, 5);
            //quickMapFsm.RemoveAction(pair.Key, prefabState == "Cliffs" ? 4 : 5);
            quickMapFsm.RemoveAction(pair.Key, 4);

            #endregion

            // Make Transitions
            foreach (var mapZoneName in pair.Value.MapZoneStrings)
            {
                quickMapFsm.AddTransition("Check Area", mapZoneName, pair.Key);
            }

            quickMapFsm.AddTransition(pair.Key, "CLOSE QUICK MAP", "Check State");
            quickMapFsm.ChangeTransition(pair.Key, "CLOSE QUICK MAP", "Check State");
        }

        #endregion

        Log("~ReplaceFsmQuickMap");
    }

    public static void QuickMapNew(ActionArg arg)
    {
        QuickMapNew(arg.Go.GetComponent<GameMap>(), arg.FsmString.Value);
    }

    public static void QuickMapNew(GameObject gameMapGo, string area)
    {
        QuickMapNew(gameMapGo.GetComponent<GameMap>(), area);
    }

    public static void QuickMapNew(GameMap self, string area)
    {
        Log($"!QuickMapNew: \"{area}\"");

        if (area == "")
        {
            area = GameManager.instance.GetCurrentMapZone();
            Log($"Area: \"{area}\"");
        }

        foreach (var (_, areaStruct) in CustomAreas.Where(pair => pair.Value.AreaGameObject != null))
            areaStruct.AreaGameObject.SetActive(areaStruct.MapZoneStrings.Contains(area));

        self.PositionCompass(false);

        Log("~QuickMapNew");
    }

    private static void Log(string message)
    {
        var outMsg = $"[AdditionalMaps][MonoBehaviours][GameMapHooks] - {message}";
        MLogger.LogDebug(outMsg);
        ULogger.Log(outMsg);
    }

    #region New Functions

    private static void IlWorldMap(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.Before, x => x.MatchLdarg(0), x => x.MatchLdfld<GameMap>("flamePins"));

        List<ILLabel> labelsToRedirect = new List<ILLabel>();
        foreach (Instruction ilInstr in il.Instrs)
        {
            if (ilInstr.Operand is ILLabel ilLabel)
            {
                if (ilInstr.Offset > 0 && ilInstr.Offset <= cursor.Prev.Offset && ilLabel.Target.Offset > 0 &&
                    ilLabel.Target.Offset >= cursor.Next.Offset)
                {
                    if (!labelsToRedirect.Contains(ilLabel))
                    {
                        Log($"Label from {ilInstr.Offset} to {ilLabel.Target.Offset} while being before {cursor.Next.Offset} has to be adjusted");
                        labelsToRedirect.Add(ilLabel);
                    }
                }
            }
        }

        ILLabel previousEntireOutsideIfLabel = null;
        foreach ((string key, SCustomArea data) in CustomAreas)
        {
            Log($"Adding area '{key}' to method!");
            ILLabel entireOutsideIfLabel = il.DefineLabel();

            ILLabel entireInsideIfLabel = il.DefineLabel();
            // if (this.pd.GetBool(data.PlayerDataBoolGotAreaMap) || ...
            cursor.Emit(OpCodes.Ldarg_0);
            if (previousEntireOutsideIfLabel != null)
            {
                previousEntireOutsideIfLabel.Target = cursor.Prev;
            }
            foreach (ILLabel ilLabel in labelsToRedirect)
            {
                ilLabel.Target = cursor.Prev;
            }
            labelsToRedirect.Clear();
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "pd"));
            cursor.Emit(OpCodes.Ldstr, data.PlayerDataBoolGotAreaMap);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(PlayerData), "GetBool"));
            cursor.Emit(OpCodes.Brtrue_S, entireInsideIfLabel);

            ILLabel entireOtherInsideIfLabel = il.DefineLabel();
            for (int i = 0; i < data.MapZoneStrings.Count - 1; i++)
            {
                // ... (||) currentMapZone == areaKey (||) ...
                cursor.Emit(OpCodes.Ldloc_0);
                cursor.Emit(OpCodes.Ldstr, data.MapZoneStrings[i]);
                cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(string), "op_Equality", false));
                cursor.Emit(OpCodes.Brtrue_S, entireOtherInsideIfLabel);
            }
            if (data.MapZoneStrings.Count > 0)
            {
                // ... (||) currentMapZone == areaKey ...
                cursor.Emit(OpCodes.Ldloc_0);
                cursor.Emit(OpCodes.Ldstr, data.MapZoneStrings[data.MapZoneStrings.Count - 1]);
                cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(string), "op_Equality", false));
                cursor.Emit(OpCodes.Brfalse_S, entireOutsideIfLabel);
            }
            // ... ) && (this.pd.GetBool("equippedCharm_2") || this.pd.GetBool("CompassAlwaysOn.Enabled"))))
            cursor.Emit(OpCodes.Ldarg_0);
            entireOtherInsideIfLabel.Target = cursor.Prev;
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "pd"));
            cursor.Emit(OpCodes.Ldstr, nameof(PlayerData.equippedCharm_2));
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(PlayerData), "GetBool"));
            cursor.Emit(OpCodes.Brtrue_S, entireInsideIfLabel);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "pd"));
            cursor.Emit(OpCodes.Ldstr, "CompassAlwaysOn.Enabled");
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(PlayerData), "GetBool"));
            cursor.Emit(OpCodes.Brfalse_S, entireOutsideIfLabel);

            // data.AreaGameObject.SetActive(this.pd.GetBool(data.PlayerDataBoolGotAreaMap));
            cursor.EmitReference(data.AreaGameObject);
            entireInsideIfLabel.Target = cursor.Prev.Previous;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "pd"));
            cursor.Emit(OpCodes.Ldstr, data.PlayerDataBoolGotAreaMap);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(PlayerData), "GetBool"));
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(GameObject), "SetActive"));

            if (data.panMinX is not null)
            {
                // this.panMinX = Mathf.Min(this.panMinX, data.panMinX);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "panMinX"));
                cursor.Emit(OpCodes.Ldc_R4, data.panMinX);
                cursor.Emit(OpCodes.Callvirt, typeof(Mathf).GetMethods(BindingFlags.Static | BindingFlags.Public).First(x => x.Name == "Min" && x.GetParameters()[0].ParameterType == typeof(float)));
                cursor.Emit(OpCodes.Stfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "panMinX"));
            }
            if (data.panMaxX is not null)
            {
                // this.panMinY = Mathf.Min(this.panMinY, data.panMaxX);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "panMaxX"));
                cursor.Emit(OpCodes.Ldc_R4, data.panMaxX);
                cursor.Emit(OpCodes.Callvirt, typeof(Mathf).GetMethods(BindingFlags.Static | BindingFlags.Public).First(x => x.Name == "Max" && x.GetParameters()[0].ParameterType == typeof(float)));
                cursor.Emit(OpCodes.Stfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "panMaxX"));
            }
            if (data.panMinY is not null)
            {
                // this.panMaxX = Mathf.Min(this.panMaxX, data.panMinY);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "panMinY"));
                cursor.Emit(OpCodes.Ldc_R4, data.panMinY);
                cursor.Emit(OpCodes.Callvirt, typeof(Mathf).GetMethods(BindingFlags.Static | BindingFlags.Public).First(x => x.Name == "Min" && x.GetParameters()[0].ParameterType == typeof(float)));
                cursor.Emit(OpCodes.Stfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "panMinY"));
            }
            if (data.panMaxY is not null)
            {
                // this.panMaxY = Mathf.Min(this.panMaxY, data.panMaxY);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "panMaxY"));
                cursor.Emit(OpCodes.Ldc_R4, data.panMaxY);
                cursor.Emit(OpCodes.Callvirt, typeof(Mathf).GetMethods(BindingFlags.Static | BindingFlags.Public).First(x => x.Name == "Max" && x.GetParameters()[0].ParameterType == typeof(float)));
                cursor.Emit(OpCodes.Stfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "panMaxY"));
            }
            cursor.Emit(OpCodes.Nop);

            previousEntireOutsideIfLabel = entireOutsideIfLabel;
        }
        if (previousEntireOutsideIfLabel != null)
        {
            previousEntireOutsideIfLabel.Target = cursor.Prev;
        }
    }

    public static void NewQuickMapAncientBasin(On.GameMap.orig_QuickMapAncientBasin orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.ABYSS.ToString());
    }

    public static void NewQuickMapCity(On.GameMap.orig_QuickMapCity orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.CITY.ToString());
    }

    public static void NewQuickMapCliffs(On.GameMap.orig_QuickMapCliffs orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.CLIFFS.ToString());
    }

    public static void NewQuickMapCrossroads(On.GameMap.orig_QuickMapCrossroads orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.CROSSROADS.ToString());
    }

    public static void NewQuickMapCrystalPeak(On.GameMap.orig_QuickMapCrystalPeak orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.MINES.ToString());
    }

    public static void NewQuickMapDeepnest(On.GameMap.orig_QuickMapDeepnest orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.DEEPNEST.ToString());
    }

    public static void NewQuickMapFogCanyon(On.GameMap.orig_QuickMapFogCanyon orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.FOG_CANYON.ToString());
    }

    public static void NewQuickMapFungalWastes(On.GameMap.orig_QuickMapFungalWastes orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.WASTES.ToString());
    }

    public static void NewQuickMapGreenpath(On.GameMap.orig_QuickMapGreenpath orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.GREEN_PATH.ToString());
    }

    public static void NewQuickMapKingdomsEdge(On.GameMap.orig_QuickMapKingdomsEdge orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.OUTSKIRTS.ToString());
    }

    public static void NewQuickMapQueensGardens(On.GameMap.orig_QuickMapQueensGardens orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.ROYAL_GARDENS.ToString());
    }

    public static void NewQuickMapRestingGrounds(On.GameMap.orig_QuickMapRestingGrounds orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.RESTING_GROUNDS.ToString());
    }

    public static void NewQuickMapDirtmouth(On.GameMap.orig_QuickMapDirtmouth orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.TOWN.ToString());
    }

    public static void NewQuickMapWaterways(On.GameMap.orig_QuickMapWaterways orig, GameMap self)
    {
        orig(self);
        QuickMapNew(self, MapZone.WATERWAYS.ToString());
    }

    private static void NewCloseQuickMap(On.GameMap.orig_CloseQuickMap orig, GameMap self)
    {
        orig(self);

        foreach (var pair in CustomAreas.Where(pair => pair.Value.AreaGameObject != null))
            pair.Value.AreaGameObject.SetActive(false);
    }

    private static void IlPositionCompass(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        cursor.Goto(0);
        /*
         * removes:
         * if (currentMapZone == "DREAM_WORLD" || currentMapZone == "WHITE_PALACE" || currentMapZone == "GODS_GLORY")
         * {
         *     this.compassIcon.SetActive(false);
         *     this.displayingCompass = false;
         *     return;
         * }
         */
        if (cursor.TryGotoNext(MoveType.Before, x => x.MatchLdloc(1), x => x.MatchLdstr("DREAM_WORLD")))
        {
            cursor.RemoveRange(20);
        }
        /*
         * move before:
         * if (this.currentScene != null) { ...
         */
        cursor.GotoNext(MoveType.Before, x => x.MatchLdarg(0), x => x.MatchLdfld<GameMap>("currentScene"), x => x.MatchLdnull());

        // redirect labels/gotos that go from before our spot to after our spot
        List<ILLabel> labelsToRedirect = new List<ILLabel>();
        foreach (Instruction ilInstr in il.Instrs)
        {
            if (ilInstr.Operand is ILLabel ilLabel)
            {
                if (ilInstr.Offset > 0 && ilInstr.Offset <= cursor.Prev.Offset && ilLabel.Target.Offset > 0 &&
                    ilLabel.Target.Offset >= cursor.Next.Offset)
                {
                    if (!labelsToRedirect.Contains(ilLabel))
                    {
                        Log($"Label from {ilInstr.Offset} to {ilLabel.Target.Offset} while being before {cursor.Next.Offset} has to be adjusted");
                        labelsToRedirect.Add(ilLabel);
                    }
                }
            }
        }

        ILLabel previousEntireOutsideIfLabel = null;
        foreach ((string key, SCustomArea data) in CustomAreas)
        {
            Log($"Adding area '{key}' to method!");
            ILLabel entireOutsideIfLabel = il.DefineLabel();
            bool doneSettingLabelTarget = false;

            ILLabel entireInsideIfLabel = il.DefineLabel();
            for (int i = 0; i < data.MapZoneStrings.Count - 1; i++)
            {
                // ... (||) currentMapZone == areaKey (||) ...
                cursor.Emit(OpCodes.Ldloc_1);
                if (previousEntireOutsideIfLabel != null && !doneSettingLabelTarget)
                {
                    previousEntireOutsideIfLabel.Target = cursor.Prev;
                    doneSettingLabelTarget = true;
                }
                foreach (ILLabel ilLabel in labelsToRedirect)
                {
                    ilLabel.Target = cursor.Prev;
                }
                labelsToRedirect.Clear();
                cursor.Emit(OpCodes.Ldstr, data.MapZoneStrings[i]);
                cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(string), "op_Equality", false));
                cursor.Emit(OpCodes.Brtrue_S, entireInsideIfLabel);
            }
            if (data.MapZoneStrings.Count > 0)
            {
                // ... (||) currentMapZone == areaKey ...
                cursor.Emit(OpCodes.Ldloc_1);
                if (previousEntireOutsideIfLabel != null && !doneSettingLabelTarget)
                {
                    previousEntireOutsideIfLabel.Target = cursor.Prev;
                }
                foreach (ILLabel ilLabel in labelsToRedirect)
                {
                    ilLabel.Target = cursor.Prev;
                }
                labelsToRedirect.Clear();
                cursor.Emit(OpCodes.Ldstr, data.MapZoneStrings[data.MapZoneStrings.Count - 1]);
                cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(string), "op_Equality", false));
                cursor.Emit(OpCodes.Brfalse_S, entireOutsideIfLabel);
            }
            // gameObject = data.AreaGameObject;
            cursor.EmitReference(data.AreaGameObject);
            entireInsideIfLabel.Target = cursor.Prev.Previous;
            cursor.Emit(OpCodes.Stloc_0);

            // for (int num8 = 0; ...
            ILLabel forLoopHeadLabel = il.DefineLabel();
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Stloc_S, (byte) 29);
            cursor.Emit(OpCodes.Br_S, forLoopHeadLabel);

            // GameObject gameObject15 = gameObject.transform.GetChild(num8).gameObject;
            ILLabel forLoopContentLabel = il.DefineLabel();
            cursor.Emit(OpCodes.Ldloc_0);
            forLoopContentLabel.Target = cursor.Prev;
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetPropertyInfo(typeof(GameObject), "transform").GetGetMethod());
            cursor.Emit(OpCodes.Ldloc_S, (byte) 29);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(Transform), "GetChild"));
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetPropertyInfo(typeof(Component), "gameObject").GetGetMethod());
            cursor.Emit(OpCodes.Stloc_S, (byte) 30);

            // if (gameObject15.name == sceneName)
            ILLabel forLoopIfNameLabel = il.DefineLabel();
            cursor.Emit(OpCodes.Ldloc_S, (byte) 30);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetPropertyInfo(typeof(UnityEngine.Object), "name").GetGetMethod());
            cursor.Emit(OpCodes.Ldloc_2);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(string), "op_Equality", false));
            cursor.Emit(OpCodes.Brfalse_S, forLoopIfNameLabel);

            // this.currentScene = gameObject15;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_S, (byte) 30);
            cursor.Emit(OpCodes.Stfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "currentScene"));

            // break;
            cursor.Emit(OpCodes.Br_S, entireOutsideIfLabel);

            // ... num8++)
            cursor.Emit(OpCodes.Ldloc_S, (byte) 29);
            forLoopIfNameLabel.Target = cursor.Prev;
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Add);
            cursor.Emit(OpCodes.Stloc_S, (byte) 29);

            // ... num8 < gameObject.transform.childCount; ...
            cursor.Emit(OpCodes.Ldloc_S, (byte) 29);
            forLoopHeadLabel.Target = cursor.Prev;
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetPropertyInfo(typeof(GameObject), "transform").GetGetMethod());
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetPropertyInfo(typeof(Transform), "childCount").GetGetMethod());
            cursor.Emit(OpCodes.Blt_S, forLoopContentLabel);

            cursor.Emit(OpCodes.Nop);

            previousEntireOutsideIfLabel = entireOutsideIfLabel;
        }
        if (previousEntireOutsideIfLabel != null)
        {
            previousEntireOutsideIfLabel.Target = cursor.Prev;
        }
    }

    #endregion
}