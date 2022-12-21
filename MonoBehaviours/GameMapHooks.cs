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
            On.GameMap.PositionCompass -= NewPositionCompass;
        }
        catch
        {
        }
        finally
        {
            On.GameMap.PositionCompass += NewPositionCompass;
        }

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
        MLogger.Log(outMsg);
        ULogger.Log(outMsg);
    }

    #region New Functions

    private static void NewWorldMap(On.GameMap.orig_WorldMap orig, GameMap self)
    {
        orig(self);

        self.panMinX = float.MinValue; //-1.44f;
        self.panMaxX = float.MaxValue; //4.55f;
        self.panMinY = float.MinValue; //-8.642f;
        self.panMaxY = float.MaxValue; //-5.58f;

        var pd = PlayerData.instance;
        var currentMapZone = GameManager.instance.GetCurrentMapZone();
        foreach (var pair in from pair in CustomAreas
                 where pd.GetBool(pair.Value.PlayerDataBoolGotAreaMap) ||
                       pair.Value.MapZoneStrings.Contains(currentMapZone) &&
                       pd.GetBool("equippedCharm_2")
                 where pd.GetBool(pair.Value.PlayerDataBoolGotAreaMap)
                 where pair.Value.AreaGameObject != null
                 select pair)
            pair.Value.AreaGameObject.SetActive(true);
    }

    private static void IlWorldMap(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.Before, x => x.MatchLdarg(0), x => x.MatchLdfld<GameMap>("flamePins"));

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

            /*
             * data.AreaGameObject.SetActive(this.pd.GetBool(data.PlayerDataBoolGotAreaMap));
             */
            cursor.EmitReference(data.AreaGameObject);
            entireInsideIfLabel.Target = cursor.Prev;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(GameMap), "pd"));
            cursor.Emit(OpCodes.Ldstr, data.PlayerDataBoolGotAreaMap);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(PlayerData), "GetBool"));
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(GameObject), "SetActive"));

            previousEntireOutsideIfLabel = entireOutsideIfLabel;
        }
        if (previousEntireOutsideIfLabel != null)
        {
            previousEntireOutsideIfLabel.Target = cursor.Prev.Next;
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

    private static void NewPositionCompass(On.GameMap.orig_PositionCompass orig, GameMap self, bool posShade)
    {
        GameObject gameObject = null;
        var currentMapZone = ReflectionHelper.GetField<GameMap, GameManager>(self, "gm").GetCurrentMapZone();

        string sceneName;
        if (!self.inRoom)
        {
            sceneName = ReflectionHelper.GetField<GameMap, GameManager>(self, "gm").sceneName;
        }
        else
        {
            currentMapZone = self.doorMapZone;
            sceneName = self.doorScene;
        }

        switch (currentMapZone)
        {
            case "ABYSS":
                gameObject = self.areaAncientBasin;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "CITY":
            case "KINGS_STATION":
            case "SOUL_SOCIETY":
            case "LURIENS_TOWER":
                gameObject = self.areaCity;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "CLIFFS":
                gameObject = self.areaCliffs;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "CROSSROADS":
            case "SHAMAN_TEMPLE":
                gameObject = self.areaCrossroads;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "MINES":
                gameObject = self.areaCrystalPeak;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "DEEPNEST":
            case "BEASTS_DEN":
                gameObject = self.areaDeepnest;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "FOG_CANYON":
            case "MONOMON_ARCHIVE":
                gameObject = self.areaFogCanyon;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "WASTES":
            case "QUEENS_STATION":
                gameObject = self.areaFungalWastes;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "GREEN_PATH":
                gameObject = self.areaGreenpath;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "OUTSKIRTS":
            case "HIVE":
            case "COLOSSEUM":
                gameObject = self.areaKingdomsEdge;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "ROYAL_GARDENS":
                gameObject = self.areaQueensGardens;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "RESTING_GROUNDS":
                gameObject = self.areaRestingGrounds;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "TOWN":
            case "KINGS_PASS":
                gameObject = self.areaDirtmouth;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
            case "WATERWAYS":
            case "GODSEEKER_WASTE":
                gameObject = self.areaWaterways;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                break;
        }

        foreach (var pair in CustomAreas.Where(pair => pair.Value.MapZoneStrings.Contains(currentMapZone))
                     .Where(pair => pair.Value.AreaGameObject != null))
        {
            gameObject = pair.Value.AreaGameObject;
            self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
        }

        if (self.currentScene != null)
        {
            if (gameObject is not null)
                self.currentScenePos =
                    new Vector3(
                        self.currentScene.transform.localPosition.x + gameObject.transform.localPosition.x,
                        self.currentScene.transform.localPosition.y + gameObject.transform.localPosition.y,
                        0f);

            switch (posShade)
            {
                case false when !ReflectionHelper.GetField<GameMap, bool>(self, "posGate"):
                {
                    if (ReflectionHelper.GetField<GameMap, PlayerData>(self, "pd").GetBool("equippedCharm_2"))
                    {
                        ReflectionHelper.SetField(self, "displayingCompass", true);
                        self.compassIcon.SetActive(true);
                    }
                    else
                    {
                        ReflectionHelper.SetField(self, "displayingCompass", false);
                        self.compassIcon.SetActive(false);
                    }

                    break;
                }
                case true:
                {
                    if (!self.inRoom)
                    {
                        self.shadeMarker.transform.localPosition =
                            new Vector3(self.currentScenePos.x, self.currentScenePos.y, 0f);
                    }
                    else
                    {
                        var x = self.currentScenePos.x -
                                self.currentScene.GetComponent<SpriteRenderer>().sprite.rect.size.x / 100f / 2f +
                                (self.doorX + self.doorOriginOffsetX) / self.doorSceneWidth *
                                (self.currentScene.GetComponent<SpriteRenderer>().sprite.rect.size.x / 100f *
                                 self.transform.localScale.x) / self.transform.localScale.x;
                        var y = self.currentScenePos.y -
                                self.currentScene.GetComponent<SpriteRenderer>().sprite.rect.size.y / 100f / 2f +
                                (self.doorY + self.doorOriginOffsetY) / self.doorSceneHeight *
                                (self.currentScene.GetComponent<SpriteRenderer>().sprite.rect.size.y / 100f *
                                 self.transform.localScale.y) / self.transform.localScale.y;
                        self.shadeMarker.transform.localPosition = new Vector3(x, y, 0f);
                    }

                    ReflectionHelper.GetField<GameMap, PlayerData>(self, "pd").SetVector3("shadeMapPos",
                        new Vector3(self.currentScenePos.x, self.currentScenePos.y, 0f));
                    break;
                }
            }

            if (!ReflectionHelper.GetField<GameMap, bool>(self, "posGate")) return;
            self.dreamGateMarker.transform.localPosition =
                new Vector3(self.currentScenePos.x, self.currentScenePos.y, 0f);
            ReflectionHelper.GetField<GameMap, PlayerData>(self, "pd").SetVector3("dreamgateMapPos",
                new Vector3(self.currentScenePos.x, self.currentScenePos.y, 0f));
        }
        else
        {
            Log("Couldn't find current scene object!");
            if (!posShade) return;
            ReflectionHelper.GetField<GameMap, PlayerData>(self, "pd")
                .SetVector3("shadeMapPos", new Vector3(-10000f, -10000f, 0f));
            self.shadeMarker.transform.localPosition =
                ReflectionHelper.GetField<GameMap, PlayerData>(self, "pd").GetVector3("shadeMapPos");
        }
    }

    #endregion
}