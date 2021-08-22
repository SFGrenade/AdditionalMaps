using System;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using SFCore.Utils;
using UnityEngine;
using Logger = Modding.Logger;
using Object = UnityEngine.Object;

namespace AdditionalMaps.MonoBehaviours
{
    public struct SCustomArea
    {
        public GameObject areaGameObject;
        public Vector3 cameraPosition;
        public string playerDataBoolGotAreaMap;
        public List<string> mapZoneStrings;
    }

    public class ActionArg
    {
        public GameObject go;
        public HutongGames.PlayMaker.FsmString fsmString;
    }

    public class GameMapHooks
    {
        protected static Dictionary<string, SCustomArea> customAreas = new();

        protected static List<Func<GameMap, Dictionary<string, SCustomArea>>> callbacks =
            new();

        public static void Init(Func<GameMap, Dictionary<string, SCustomArea>> callback = null)
        {
            try
            {
                On.GameManager.SetGameMap -= OnGameManagerSetGameMap;
            }
            catch (Exception)
            {
                // ignored
            }

            On.GameManager.SetGameMap += OnGameManagerSetGameMap;

            MakeHooks();

            if (callback != null) callbacks.Add(callback);
        }

        protected static void MakeHooks()
        {
            On.GameMap.WorldMap += NewWorldMap;
            On.GameMap.QuickMapAncientBasin += NewQuickMapAncientBasin;
            On.GameMap.QuickMapCity += NewQuickMapCity;
            On.GameMap.QuickMapCliffs += NewQuickMapCliffs;
            On.GameMap.QuickMapCrossroads += NewQuickMapCrossroads;
            On.GameMap.QuickMapCrystalPeak += NewQuickMapCrystalPeak;
            On.GameMap.QuickMapDeepnest += NewQuickMapDeepnest;
            On.GameMap.QuickMapFogCanyon += NewQuickMapFogCanyon;
            On.GameMap.QuickMapFungalWastes += NewQuickMapFungalWastes;
            On.GameMap.QuickMapGreenpath += NewQuickMapGreenpath;
            On.GameMap.QuickMapKingdomsEdge += NewQuickMapKingdomsEdge;
            On.GameMap.QuickMapQueensGardens += NewQuickMapQueensGardens;
            On.GameMap.QuickMapRestingGrounds += NewQuickMapRestingGrounds;
            On.GameMap.QuickMapDirtmouth += NewQuickMapDirtmouth;
            On.GameMap.QuickMapWaterways += NewQuickMapWaterways;
            On.GameMap.CloseQuickMap += NewCloseQuickMap;
            On.GameMap.PositionCompass += NewPositionCompass;
        }

        protected static void OnGameManagerSetGameMap(On.GameManager.orig_SetGameMap orig, GameManager self,
            GameObject goGameMap)
        {
            ReplaceComponent(goGameMap);
            orig(self, goGameMap);
        }

        protected static void ReplaceComponent(GameObject gameMapGameObject)
        {
            Log("!ReplaceComponent");

            if (gameMapGameObject == null)
            {
                Log("~ReplaceComponent");
                return;
            }

            Log("Got " + callbacks.Count + " callbacks!");
            foreach (var tmp in callbacks)
            {
                Object.DontDestroyOnLoad(gameMapGameObject);
                var callbackAreas = tmp.Invoke(gameMapGameObject.GetComponent<GameMap>());
                Log("Got " + callbackAreas.Count + " custom Areas!");
                foreach (var (areaKey, areaStruct) in callbackAreas)
                {
                    Log("Name: " + areaKey);
                    customAreas.Add(areaKey, areaStruct);
                    Object.DontDestroyOnLoad(areaStruct.areaGameObject);
                    areaStruct.areaGameObject.transform.SetParent(gameMapGameObject.transform);
                }
            }

            callbacks.Clear();

            ReplaceFsmQuickMap(gameMapGameObject);

            Log("~ReplaceComponent");
        }

        protected static void ReplaceFsmQuickMap(GameObject gameMapGameObject)
        {
            Log("!ReplaceFsmQuickMap");

            #region Make transform changes of custom areas

            foreach (var pair in customAreas) pair.Value.areaGameObject.transform.SetParent(gameMapGameObject.transform);

            #endregion

            var quickMapGameObject = GameObject.Find("Quick Map");
            var quickMapFsm = quickMapGameObject.LocateMyFSM("Quick Map");

            #region Add fsm string variable

            var quickMapFsmVars = quickMapFsm.FsmVariables;

            #endregion

            #region Make States & Transitions

            //quickMapFsm.InsertAction("Check Area", new ActualLogAction { text = quickMapFsmVars.FindFsmString("Map Zone") }, 0);

            foreach (var pair in customAreas)
            {
                Log($"Adding state {pair.Key}");

                // Make State
                const string prefabState = "Cliffs";
                quickMapFsm.CopyState(prefabState, pair.Key);
                quickMapFsm.GetAction<PlayerDataBoolTest>(pair.Key, 0).boolName = pair.Value.playerDataBoolGotAreaMap;
                quickMapFsm.GetAction<GetLanguageString>(pair.Key, 1).convName = pair.Key;
                quickMapFsm.GetAction<SetPosition>(pair.Key, 5).x = pair.Value.cameraPosition.x;
                quickMapFsm.GetAction<SetPosition>(pair.Key, 5).y = pair.Value.cameraPosition.y;
                quickMapFsm.GetAction<SetPosition>(pair.Key, 5).z = pair.Value.cameraPosition.z;

                #region Make Action

                //CallMethodProper fsmCMPAction = new CallMethodProper();
                //fsmCMPAction.gameObject = tmpGameObjectOwner;
                //fsmCMPAction.behaviour = "GameMap";
                //fsmCMPAction.methodName = "QuickMapNew";
                //fsmCMPAction.parameters = new FsmVar[] { new FsmVar() { NamedVar = quickMapFsmVars.FindFsmString("Map Zone") } };
                //fsmCMPAction.storeResult = new FsmVar() { NamedVar = quickMapFsmVars.FindFsmBool("Check Area Return") };

                var fsmAaAction = new ActionAction<ActionArg>
                {
                    arg = new ActionArg()
                    {
                        go = gameMapGameObject, fsmString = quickMapFsmVars.FindFsmString("Map Zone")
                    },
                    action = QuickMapNew
                };
                
                //quickMapFsm.InsertAction(pair.Key, fsmAaAction, prefabState == "Cliffs" ? 5 : 6);
                quickMapFsm.InsertAction(pair.Key, fsmAaAction, 5);
                //quickMapFsm.RemoveAction(pair.Key, prefabState == "Cliffs" ? 4 : 5);
                quickMapFsm.RemoveAction(pair.Key, 4);

                #endregion

                // Make Transitions
                foreach (var mapZoneName in pair.Value.mapZoneStrings)
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
            QuickMapNew(arg.go.GetComponent<GameMap>(), arg.fsmString.Value);
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

            foreach (var (_, areaStruct) in customAreas.Where(pair => pair.Value.areaGameObject != null))
                areaStruct.areaGameObject.SetActive(areaStruct.mapZoneStrings.Contains(area));

            self.PositionCompass(false);

            Log("~QuickMapNew");
        }

        private static void Log(string message)
        {
            Logger.Log($"[GameMapHooks] - {message}");
        }

        private static void Log(object message)
        {
            Logger.Log($"[GameMapHooks] - {message}");
        }

        #region New Functions

        protected static void NewWorldMap(On.GameMap.orig_WorldMap orig, GameMap self)
        {
            orig(self);

            self.panMinX = float.MinValue; //-1.44f;
            self.panMaxX = float.MaxValue; //4.55f;
            self.panMinY = float.MinValue; //-8.642f;
            self.panMaxY = float.MaxValue; //-5.58f;

            var pd = PlayerData.instance;
            var currentMapZone = GameManager.instance.GetCurrentMapZone();
            foreach (var pair in from pair in customAreas where pd.GetBool(pair.Value.playerDataBoolGotAreaMap) || pair.Value.mapZoneStrings.Contains(currentMapZone) &&
                pd.GetBool("equippedCharm_2") where pd.GetBool(pair.Value.playerDataBoolGotAreaMap) where pair.Value.areaGameObject != null select pair)
                pair.Value.areaGameObject.SetActive(true);
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

        protected static void NewCloseQuickMap(On.GameMap.orig_CloseQuickMap orig, GameMap self)
        {
            orig(self);

            foreach (var pair in customAreas)
                if (pair.Value.areaGameObject != null)
                    pair.Value.areaGameObject.SetActive(false);
        }

        protected static void NewPositionCompass(On.GameMap.orig_PositionCompass orig, GameMap self, bool posShade)
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

            if (currentMapZone == "ABYSS")
            {
                gameObject = self.areaAncientBasin;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "CITY" || currentMapZone == "KINGS_STATION" || currentMapZone == "SOUL_SOCIETY" || currentMapZone == "LURIENS_TOWER")
            {
                gameObject = self.areaCity;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "CLIFFS")
            {
                gameObject = self.areaCliffs;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "CROSSROADS" || currentMapZone == "SHAMAN_TEMPLE")
            {
                gameObject = self.areaCrossroads;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "MINES")
            {
                gameObject = self.areaCrystalPeak;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "DEEPNEST" || currentMapZone == "BEASTS_DEN")
            {
                gameObject = self.areaDeepnest;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "FOG_CANYON" || currentMapZone == "MONOMON_ARCHIVE")
            {
                gameObject = self.areaFogCanyon;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "WASTES" || currentMapZone == "QUEENS_STATION")
            {
                gameObject = self.areaFungalWastes;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "GREEN_PATH")
            {
                gameObject = self.areaGreenpath;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "OUTSKIRTS" || currentMapZone == "HIVE" || currentMapZone == "COLOSSEUM")
            {
                gameObject = self.areaKingdomsEdge;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "ROYAL_GARDENS")
            {
                gameObject = self.areaQueensGardens;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "RESTING_GROUNDS")
            {
                gameObject = self.areaRestingGrounds;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "TOWN" || currentMapZone == "KINGS_PASS")
            {
                gameObject = self.areaDirtmouth;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }
            else if (currentMapZone == "WATERWAYS" || currentMapZone == "GODSEEKER_WASTE")
            {
                gameObject = self.areaWaterways;
                self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
            }

            foreach (var pair in customAreas)
                if (pair.Value.mapZoneStrings.Contains(currentMapZone))
                    if (pair.Value.areaGameObject != null)
                    {
                        gameObject = pair.Value.areaGameObject;
                        self.currentScene = gameObject.FindGameObjectInChildren(sceneName);
                    }

            if (self.currentScene != null)
            {
                if (gameObject is not null)
                    self.currentScenePos =
                        new Vector3(self.currentScene.transform.localPosition.x + gameObject.transform.localPosition.x,
                            self.currentScene.transform.localPosition.y + gameObject.transform.localPosition.y, 0f);

                if (!posShade && !ReflectionHelper.GetField<GameMap, bool>(self, "posGate"))
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
                }

                if (posShade)
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
                }

                if (ReflectionHelper.GetField<GameMap, bool>(self, "posGate"))
                {
                    self.dreamGateMarker.transform.localPosition =
                        new Vector3(self.currentScenePos.x, self.currentScenePos.y, 0f);
                    ReflectionHelper.GetField<GameMap, PlayerData>(self, "pd").SetVector3("dreamgateMapPos",
                        new Vector3(self.currentScenePos.x, self.currentScenePos.y, 0f));
                }
            }
            else
            {
                Debug.Log("Couldn't find current scene object!");
                if (posShade)
                {
                    ReflectionHelper.GetField<GameMap, PlayerData>(self, "pd")
                        .SetVector3("shadeMapPos", new Vector3(-10000f, -10000f, 0f));
                    self.shadeMarker.transform.localPosition = ReflectionHelper.GetField<GameMap, PlayerData>(self, "pd")
                        .GetVector3("shadeMapPos");
                }
            }
        }

        #endregion
    }
}
