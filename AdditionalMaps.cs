using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using Modding;
using ModCommon;
using ModCommon.Util;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Security.Cryptography;
using AdditionalMaps.Consts;
using AdditionalMaps.Utils;
using AdditionalMaps.MonoBehaviours;
using SFCore;
using TMPro;
using UScene = UnityEngine.SceneManagement.Scene;

namespace AdditionalMaps
{
    public class AdditionalMaps : Mod<AmSaveSettings, AmGlobalSettings>
    {
        internal static AdditionalMaps Instance;

        public LanguageStrings langStrings { get; private set; }
        public TextureStrings spriteDict { get; private set; }
        public AchievementHelper achHelper { get; private set; }
        public CharmHelper charmHelper { get; private set; } // DEBUG
        public ItemHelper itemHelper { get; private set; }

        private GameManager gm;
        private PlayerData pd;

        public static Sprite GetSprite(string name) => Instance.spriteDict.Get(name);
        public static Material defaultSpriteMaterial { get; private set; }

        // Thx to 56
        public override string GetVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            string ver = asm.GetName().Version.ToString();

            SHA1 sha1 = SHA1.Create();
            FileStream stream = File.OpenRead(asm.Location);

            byte[] hashBytes = sha1.ComputeHash(stream);

            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            stream.Close();
            sha1.Clear();

            string ret = $"{ver}-{hash.Substring(0, 6)}";

            return ret;
        }

        public override void Initialize()
        {
            Log("Initializing");
            Instance = this;
            gm = GameManager.instance;
            pd = PlayerData.instance;

            initGlobalSettings();
            langStrings = new LanguageStrings();
            spriteDict = new TextureStrings();
            initCallbacks();

            defaultSpriteMaterial = new Material(Shader.Find("Sprites/Default"));
            defaultSpriteMaterial.SetColor(Shader.PropertyToID("_Color"), new Color(1.0f, 1.0f, 1.0f, 1.0f));
            defaultSpriteMaterial.SetInt(Shader.PropertyToID("PixelSnap"), 0);
            defaultSpriteMaterial.SetFloat(Shader.PropertyToID("_EnableExternalAlpha"), 0.0f);
            defaultSpriteMaterial.SetInt(Shader.PropertyToID("_StencilComp"), 8);
            defaultSpriteMaterial.SetInt(Shader.PropertyToID("_Stencil"), 0);
            defaultSpriteMaterial.SetInt(Shader.PropertyToID("_StencilOp"), 0);
            defaultSpriteMaterial.SetInt(Shader.PropertyToID("_StencilWriteMask"), 255);
            defaultSpriteMaterial.SetInt(Shader.PropertyToID("_StencilReadMask"), 255);

            //GameMapBetter.Init(gameMapBetterCallback); // DEBUG
            GameMapHooks.Init(gameMapCallback); // DEBUG
            GameManager.instance.StartCoroutine(ChangeMap2()); // DEBUG

            Log("Initialized");
        }

        private IEnumerator MapCompleteRegion()
        {
            yield return new WaitWhile(() => !GameObject.FindObjectOfType<GameMap>());

            string scene_name = "scene_name"; // Whichever scene the player just entered
            
            var gameMap = GameObject.FindObjectOfType<GameMap>();
            GameObject ret = null;
            ret = gameMap.areaAncientBasin.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaCity.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaCliffs.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaCrossroads.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaCrystalPeak.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaDeepnest.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaDirtmouth.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaFogCanyon.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaFungalWastes.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaGreenpath.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaKingdomsEdge.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaQueensGardens.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaRestingGrounds.FindGameObjectInChildren(scene_name) ?? ret;
            ret = gameMap.areaWaterways.FindGameObjectInChildren(scene_name) ?? ret;
            if (ret)
            {
                for (int i = 0; i < ret.transform.childCount; i++)
                {
                    var go = ret.transform.GetChild(i);
                    if (!PlayerData.instance.GetVariable<List<string>>(nameof(PlayerData.instance.scenesMapped)).Contains(go.name))
                    {
                        PlayerData.instance.GetVariable<List<string>>(nameof(PlayerData.instance.scenesMapped)).Add(go.name);
                    }
                }
            }
        }

        private Dictionary<string, s_CustomArea> gameMapCallback(GameMap gameMapBetter)
        {
            Log("!gameMapCallback");

            var AreaWhitePalace = GameObject.Instantiate(gameMapBetter.areaCliffs, gameMapBetter.transform);
            AreaWhitePalace.SetActive(true);

            var subAreaPrefab = GameObject.Instantiate(gameMapBetter.areaCliffs.transform.GetChild(6).GetChild(0).gameObject);
            var roomMat = UnityEngine.Object.Instantiate(gameMapBetter.areaCliffs.transform.GetChild(1).GetComponent<SpriteRenderer>().material);
            defaultSpriteMaterial = roomMat;

            for (int i = 0; i < AreaWhitePalace.transform.childCount; i++)
            {
                GameObject.Destroy(AreaWhitePalace.transform.GetChild(i).gameObject);
            }

            var tmpDict = new Dictionary<string, s_CustomArea>();
            AreaWhitePalace.name = "WHITE_PALACE";
            AreaWhitePalace.layer = 5;
            AreaWhitePalace.transform.localScale = Vector3.one;
            AreaWhitePalace.transform.localPosition = new Vector3(-2.0f, 16f, gameMapBetter.areaCliffs.transform.localPosition.z);

            List<GameObject> wpScenes = new List<GameObject>() {
                new GameObject("White_Palace_01", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_02", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_03_hub", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_04", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_05", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_06", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_07", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_08", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_09", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_12", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_13", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_14", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_15", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_16", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_17", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_18", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_19", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("White_Palace_20", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                //new GameObject(TextureStrings.WPMapKey, typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom))
            };
            foreach (var sceneGo in wpScenes)
            {
                sceneGo.transform.SetParent(AreaWhitePalace.transform);
                sceneGo.layer = 5;
                sceneGo.transform.localScale = Vector3.one;
                sceneGo.SetActive(true);
                var sr = sceneGo.GetComponent<SpriteRenderer>();
                sr.material = roomMat;
                sr.sprite = spriteDict.Get(sceneGo.name);
                sr.sortingLayerID = 629535577;
                sr.sortingOrder = 0;
                var rmr = sceneGo.GetComponent<RoughMapRoom>();
                rmr.fullSprite = sr.sprite;
                //var mcr = sceneGo.GetComponent<MappedCustomRoom>();
                //mcr.fullSpriteDisplayed = true;
            }
            var tmpChildZ = gameMapBetter.areaCliffs.transform.GetChild(6).localPosition.z;
            float sceneDivider = 500.0f + (100.0f / 3.0f);
            wpScenes[0].transform.localPosition = new Vector3(-375 / sceneDivider, -2510 / sceneDivider, tmpChildZ);
            wpScenes[1].transform.localPosition = new Vector3(856 / sceneDivider, -2687 / sceneDivider, tmpChildZ);
            wpScenes[2].transform.localPosition = new Vector3(-30 / sceneDivider, -1496 / sceneDivider, tmpChildZ);
            wpScenes[3].transform.localPosition = new Vector3(-1110 / sceneDivider, -1647 / sceneDivider, tmpChildZ);
            wpScenes[4].transform.localPosition = new Vector3(1112 / sceneDivider, -1093 / sceneDivider, tmpChildZ);
            wpScenes[5].transform.localPosition = new Vector3(52 / sceneDivider, -717 / sceneDivider, tmpChildZ);
            wpScenes[6].transform.localPosition = new Vector3(36 / sceneDivider, 67 / sceneDivider, tmpChildZ);
            wpScenes[7].transform.localPosition = new Vector3(1971 / sceneDivider, 850 / sceneDivider, tmpChildZ);
            wpScenes[8].transform.localPosition = new Vector3(-391 / sceneDivider, 1804 / sceneDivider, tmpChildZ);
            wpScenes[9].transform.localPosition = new Vector3(-117 / sceneDivider, 456 / sceneDivider, tmpChildZ);
            wpScenes[10].transform.localPosition = new Vector3(773 / sceneDivider, 572 / sceneDivider, tmpChildZ);
            wpScenes[11].transform.localPosition = new Vector3(-1094 / sceneDivider, -1302 / sceneDivider, tmpChildZ);
            wpScenes[12].transform.localPosition = new Vector3(592 / sceneDivider, -1201 / sceneDivider, tmpChildZ);
            wpScenes[13].transform.localPosition = new Vector3(1407 / sceneDivider, -1147 / sceneDivider, tmpChildZ);
            wpScenes[14].transform.localPosition = new Vector3(-2187 / sceneDivider, -283 / sceneDivider, tmpChildZ);
            wpScenes[15].transform.localPosition = new Vector3(-1507 / sceneDivider, -640 / sceneDivider, tmpChildZ);
            wpScenes[16].transform.localPosition = new Vector3(-1717 / sceneDivider, -93 / sceneDivider, tmpChildZ);
            wpScenes[17].transform.localPosition = new Vector3(-1292 / sceneDivider, 40 / sceneDivider, tmpChildZ);
            //wpScenes[18].transform.localPosition = new Vector3(0f, 0f, tmpChildZ);

            wpScenes[17].transform.localScale = new Vector3(0.93f, 1.04f, 1.0f);
            //wpScenes[18].GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);

            List<GameObject> wpRoomSprites = new List<GameObject>() {
                new GameObject("RWP01", typeof(SpriteRenderer)),
                new GameObject("RWP02", typeof(SpriteRenderer)),
                new GameObject("RWP03", typeof(SpriteRenderer)),
                new GameObject("RWP04", typeof(SpriteRenderer)),
                new GameObject("RWP05", typeof(SpriteRenderer)),
                new GameObject("RWP06", typeof(SpriteRenderer)),
                new GameObject("RWP07", typeof(SpriteRenderer)),
                new GameObject("RWP08", typeof(SpriteRenderer)),
                new GameObject("RWP09", typeof(SpriteRenderer)),
                new GameObject("RWP12", typeof(SpriteRenderer)),
                new GameObject("RWP13", typeof(SpriteRenderer)),
                new GameObject("RWP14", typeof(SpriteRenderer)),
                new GameObject("RWP15", typeof(SpriteRenderer)),
                new GameObject("RWP16", typeof(SpriteRenderer)),
                new GameObject("RWP17", typeof(SpriteRenderer)),
                new GameObject("RWP18", typeof(SpriteRenderer)),
                new GameObject("RWP19", typeof(SpriteRenderer)),
                new GameObject("RWP20", typeof(SpriteRenderer))
            };
            float roomDivider = 5.333333f;
            wpRoomSprites[0].transform.SetParent(wpScenes[0].transform);
            wpRoomSprites[0].transform.localPosition = new Vector3(0.23f / roomDivider, 0.03333334f / roomDivider);
            wpRoomSprites[1].transform.SetParent(wpScenes[1].transform);
            wpRoomSprites[1].transform.localPosition = new Vector3(0.64f / roomDivider, -0.144f / roomDivider);
            wpRoomSprites[2].transform.SetParent(wpScenes[2].transform);
            wpRoomSprites[2].transform.localPosition = new Vector3(0.035f / roomDivider, 0f / roomDivider);
            wpRoomSprites[3].transform.SetParent(wpScenes[3].transform);
            wpRoomSprites[3].transform.localPosition = new Vector3(0.166f / roomDivider, -0.037f / roomDivider);
            wpRoomSprites[4].transform.SetParent(wpScenes[4].transform);
            wpRoomSprites[4].transform.localPosition = new Vector3(1.76f / roomDivider, 0.21f / roomDivider);
            wpRoomSprites[5].transform.SetParent(wpScenes[5].transform);
            wpRoomSprites[5].transform.localPosition = new Vector3(1.293333f / roomDivider, -0.03f / roomDivider);
            wpRoomSprites[6].transform.SetParent(wpScenes[6].transform);
            wpRoomSprites[6].transform.localPosition = new Vector3(0.8666667f / roomDivider, 0.05f / roomDivider);
            wpRoomSprites[7].transform.SetParent(wpScenes[7].transform);
            wpRoomSprites[7].transform.localPosition = new Vector3(0.3166667f / roomDivider, 0.03333334f / roomDivider);
            wpRoomSprites[8].transform.SetParent(wpScenes[8].transform);
            wpRoomSprites[8].transform.localPosition = new Vector3(0.175f / roomDivider, -1.347f / roomDivider);
            wpRoomSprites[9].transform.SetParent(wpScenes[9].transform);
            wpRoomSprites[9].transform.localPosition = new Vector3(0.024f / roomDivider, 4.4f / roomDivider);
            wpRoomSprites[10].transform.SetParent(wpScenes[10].transform);
            wpRoomSprites[10].transform.localPosition = new Vector3(0.08333334f / roomDivider, 3.96f / roomDivider);
            wpRoomSprites[11].transform.SetParent(wpScenes[11].transform);
            wpRoomSprites[11].transform.localPosition = new Vector3(0.168f / roomDivider, 2.326667f / roomDivider);
            wpRoomSprites[12].transform.SetParent(wpScenes[12].transform);
            wpRoomSprites[12].transform.localPosition = new Vector3(0.32f / roomDivider, 0.5433334f / roomDivider);
            wpRoomSprites[13].transform.SetParent(wpScenes[13].transform);
            wpRoomSprites[13].transform.localPosition = new Vector3(4.514f / roomDivider, -0.1033333f / roomDivider);
            wpRoomSprites[14].transform.SetParent(wpScenes[14].transform);
            wpRoomSprites[14].transform.localPosition = new Vector3(0.05777778f / roomDivider, -0.05f / roomDivider);
            wpRoomSprites[15].transform.SetParent(wpScenes[15].transform);
            wpRoomSprites[15].transform.localPosition = new Vector3(4.403333f / roomDivider, -0.1866667f / roomDivider);
            wpRoomSprites[16].transform.SetParent(wpScenes[16].transform);
            wpRoomSprites[16].transform.localPosition = new Vector3(-0.696f / roomDivider, 0.957f / roomDivider);
            wpRoomSprites[17].transform.SetParent(wpScenes[17].transform);
            wpRoomSprites[17].transform.localPosition = new Vector3(0.04333333f / roomDivider, 4.23f / roomDivider);
            foreach (var sprite in wpRoomSprites)
            {
                sprite.layer = 5;
                sprite.transform.localScale = Vector3.one;
                sprite.SetActive(true);
                var sr = sprite.GetComponent<SpriteRenderer>();
                sr.material = roomMat;
                sr.sprite = spriteDict.Get(sprite.name);
                sr.sortingLayerID = 629535577;
                sr.sortingOrder = 0;
            }
            wpRoomSprites[17].transform.localScale = new Vector3(1.0f / 0.93f, 1.0f / 1.04f, 1.0f);

            var pathOfPainArea = GameObject.Instantiate(subAreaPrefab, wpScenes[15].transform);
            pathOfPainArea.SetActive(true);
            pathOfPainArea.transform.localPosition = new Vector3(5.875f, -0.8f, pathOfPainArea.transform.localPosition.z);
            pathOfPainArea.GetComponent<SetTextMeshProGameText>().convName = LanguageStrings.PathOfPain_Key;

            var workshopArea = GameObject.Instantiate(subAreaPrefab, wpScenes[7].transform);
            workshopArea.SetActive(true);
            workshopArea.transform.localPosition = new Vector3(5f, -1.25f, workshopArea.transform.localPosition.z);
            workshopArea.GetComponent<SetTextMeshProGameText>().convName = LanguageStrings.Workshop_Key;

            var creditsArea = GameObject.Instantiate(subAreaPrefab, wpScenes[6].transform);
            creditsArea.SetActive(true);
            creditsArea.transform.localPosition = new Vector3(7f, -1.5f, creditsArea.transform.localPosition.z);
            creditsArea.GetComponent<SetTextMeshProGameText>().convName = LanguageStrings.Credits_Key;
            var rectT = creditsArea.GetComponent<RectTransform>();
            rectT.sizeDelta = new Vector2(rectT.sizeDelta.x + 1, rectT.sizeDelta.y);

            AreaWhitePalace.SetActive(true);
            tmpDict.Add(
                "WHITE_PALACE",
                new s_CustomArea()
                {
                    areaGameObject = AreaWhitePalace,
                    cameraPosition = new Vector3(4.07f, -24.5f, 18f),
                    mapZoneStrings = new List<string>() { "WHITE_PALACE" },
                    playerDataBoolGotAreaMap = "mapAbyss"
                }
            );


            GameObject.Destroy(subAreaPrefab);
            //printDebug(AreaWhitePalace);
            Log("~gameMapCallback");
            return tmpDict;
        }

        private void initGlobalSettings()
        {
            // Found in a project, might help saving, don't know, but who cares
            // Global Settings
        }

        private void initSaveSettings(SaveGameData data)
        {
            //// Found in a project, might help saving, don't know, but who cares
            //// Save Settings
            //// Start Mod Quest
            //Settings.SFGrenadeTestOfTeamwork_StartQuest = Settings.SFGrenadeTestOfTeamwork_StartQuest;
            //if (!Settings.SFGrenadeTestOfTeamwork_StartQuest)
            //    Settings.SFGrenadeTestOfTeamwork_StartQuest = (PlayerData.instance.royalCharmState == 4);
            //// Mechanics
            //Settings.SFGrenadeTestOfTeamwork_HornetCompanion = Settings.SFGrenadeTestOfTeamwork_HornetCompanion;
            //// Bosses
            //Settings.SFGrenadeTestOfTeamwork_DefeatedWeaverPrincess = Settings.SFGrenadeTestOfTeamwork_DefeatedWeaverPrincess;
            //// Areas
            //Settings.SFGrenadeTestOfTeamwork_TotOpened = Settings.SFGrenadeTestOfTeamwork_TotOpened;
            //Settings.SFGrenadeTestOfTeamwork_VisitedTestOfTeamwork = Settings.SFGrenadeTestOfTeamwork_VisitedTestOfTeamwork;
            //Settings.SFGrenadeTestOfTeamwork_TotOpenedShortcut = Settings.SFGrenadeTestOfTeamwork_TotOpenedShortcut;
            //Settings.SFGrenadeTestOfTeamwork_TotOpenedTotem = Settings.SFGrenadeTestOfTeamwork_TotOpenedTotem;

            //// Charms
            //Settings.gotCharms = Settings.gotCharms;
            //Settings.newCharms = Settings.newCharms;
            //Settings.equippedCharms = Settings.equippedCharms;
            //Settings.charmCosts = Settings.charmCosts;
        }

        private void initCallbacks()
        {
            //// Hooks
            //ModHooks.Instance.GetPlayerBoolHook += OnGetPlayerBoolHook;
            //ModHooks.Instance.SetPlayerBoolHook += OnSetPlayerBoolHook;
            //ModHooks.Instance.GetPlayerIntHook += OnGetPlayerIntHook;
            //ModHooks.Instance.SetPlayerIntHook += OnSetPlayerIntHook;
            //ModHooks.Instance.AfterSavegameLoadHook += initSaveSettings;
            //ModHooks.Instance.ApplicationQuitHook += SaveTotGlobalSettings;
            ModHooks.Instance.LanguageGetHook += OnLanguageGetHook;
            //UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;

            //// DEBUG Game Map stuff
            ////On.GameMap.CloseQuickMap += GameMap_CloseQuickMap;
            ////On.GameMap.PositionCompass += GameMap_PositionCompass;
            ////On.GameMap.QuickMapAncientBasin += GameMap_QuickMapAncientBasin;
            ////On.GameMap.QuickMapCity += GameMap_QuickMapCity;
            ////On.GameMap.QuickMapCliffs += GameMap_QuickMapCliffs;
            ////On.GameMap.QuickMapCrossroads += GameMap_QuickMapCrossroads;
            ////On.GameMap.QuickMapCrystalPeak += GameMap_QuickMapCrystalPeak;
            ////On.GameMap.QuickMapDeepnest += GameMap_QuickMapDeepnest;
            ////On.GameMap.QuickMapDirtmouth += GameMap_QuickMapDirtmouth;
            ////On.GameMap.QuickMapFogCanyon += GameMap_QuickMapFogCanyon;
            ////On.GameMap.QuickMapFungalWastes += GameMap_QuickMapFungalWastes;
            ////On.GameMap.QuickMapGreenpath += GameMap_QuickMapGreenpath;
            ////On.GameMap.QuickMapKingdomsEdge += GameMap_QuickMapKingdomsEdge;
            ////On.GameMap.QuickMapQueensGardens += GameMap_QuickMapQueensGardens;
            ////On.GameMap.QuickMapRestingGrounds += GameMap_QuickMapRestingGrounds;
            ////On.GameMap.QuickMapWaterways += GameMap_QuickMapWaterways;
            ////On.GameMap.WorldMap += GameMap_WorldMap;
        }

        private void SaveTotGlobalSettings()
        {
            SaveGlobalSettings();
        }

        #region Get/Set Hooks
        private string OnLanguageGetHook(string key, string sheet)
        {
            //Log($"Sheet: {sheet}; Key: {key}");
            if (langStrings.ContainsKey(key, sheet))
            {
                return langStrings.Get(key, sheet);
            }
            return Language.Language.GetInternal(key, sheet);
        }

        private bool OnGetPlayerBoolHook(string target)
        {
            if (Settings.BoolValues.ContainsKey(target))
            {
                return Settings.BoolValues[target];
            }
            return PlayerData.instance.GetBoolInternal(target);
        }
        private void OnSetPlayerBoolHook(string target, bool val)
        {
            if (Settings.BoolValues.ContainsKey(target))
            {
                Settings.BoolValues[target] = val;
                return;
            }
            PlayerData.instance.SetBoolInternal(target, val);
        }

        private int OnGetPlayerIntHook(string target)
        {
            if (Settings.IntValues.ContainsKey(target))
            {
                return Settings.IntValues[target];
            }
            return PlayerData.instance.GetIntInternal(target);
        }
        private void OnSetPlayerIntHook(string target, int val)
        {
            if (Settings.IntValues.ContainsKey(target))
            {
                Settings.IntValues[target] = val;
            }
            else
            {
                PlayerData.instance.SetIntInternal(target, val);
            }
            //Log("Int  set: " + target + "=" + val.ToString());
        }
        #endregion

        private void printDebugFsm(PlayMakerFSM fsm)
        {
            foreach (var state in fsm.FsmStates)
            {
                Log("State: " + state.Name);
                foreach (var trans in state.Transitions)
                {
                    Log("\t" + trans.EventName + " -> " + trans.ToState);
                }
            }
        }

        private IEnumerator debugPrintWait(string name)
        {
            yield return new WaitWhile(() => !(GameObject.Find(name)));

            printDebug(GameObject.Find(name));
        }

        #region Custom Area Test
        string customAreaName = "WHITE_PALACE_AREA";

        private IEnumerator ChangeMap2()
        {
            Log("ChangeMap2...");
            yield return new WaitWhile(() => !(GameObject.Find("Wide Map")));
            Log("!ChangeMap2");

            var wideMap = GameObject.Find("Wide Map");

            #region Add sprite and text for custom area
            var customPart = GameObject.Instantiate(wideMap.FindGameObjectInChildren("Ancient Basin"), wideMap.transform, true);
            customPart.name = customAreaName;
            customPart.transform.localPosition = new Vector3(3.52f, -2.1f, -2.3f);
            customPart.GetComponent<SpriteRenderer>().sprite = GetSprite(TextureStrings.CustomAreaKey);
            customPart.GetComponentInChildren<SetTextMeshProGameText>().convName = customAreaName.ToUpper();
            customPart.transform.Find("Area Name").localPosition += new Vector3(-0.5f, 0, 0);
            #endregion

            #region Edit World Map - UI Control FSM
            // Our Custom Global Event
            var customGlobalEvent = new FsmEvent(customAreaName.ToUpper());

            var worldMap = wideMap.transform.parent.gameObject;
            var worldMapFsm = worldMap.LocateMyFSM("UI Control");

            #region Add Custom FSM Variable
            var wmUCFsmVars = worldMapFsm.FsmVariables;
            FsmGameObject[] tmpGameObjectVariables = new FsmGameObject[wmUCFsmVars.GameObjectVariables.Length + 1];
            wmUCFsmVars.GameObjectVariables.CopyTo(tmpGameObjectVariables, 0);
            tmpGameObjectVariables[tmpGameObjectVariables.Length - 1] = tmpGameObjectVariables[tmpGameObjectVariables.Length - 2];
            tmpGameObjectVariables[tmpGameObjectVariables.Length - 1].Name = customAreaName;
            wmUCFsmVars.GameObjectVariables = tmpGameObjectVariables;
            #endregion
            #region Add FindChild Action to store Custom Area Sprite
            FindChild tmpActionFindChild = new FindChild();
            tmpActionFindChild.gameObject = worldMapFsm.GetAction<FindChild>("Init", 10).gameObject;
            tmpActionFindChild.childName = customAreaName;
            tmpActionFindChild.storeResult = wmUCFsmVars.GetFsmGameObject(customAreaName);
            worldMapFsm.InsertAction("Init", tmpActionFindChild, 11);
            #endregion

            #region Add Custom Global Transition
            var tmpFsmGlobalTransitions = new FsmTransition[worldMapFsm.FsmGlobalTransitions.Length + 1];
            worldMapFsm.FsmGlobalTransitions.CopyTo(tmpFsmGlobalTransitions, 0);
            tmpFsmGlobalTransitions[tmpFsmGlobalTransitions.Length - 1] = new FsmTransition(tmpFsmGlobalTransitions[tmpFsmGlobalTransitions.Length - 2]);
            tmpFsmGlobalTransitions[tmpFsmGlobalTransitions.Length - 1].FsmEvent = customGlobalEvent;
            tmpFsmGlobalTransitions[tmpFsmGlobalTransitions.Length - 1].ToState = customAreaName;

            Modding.ReflectionHelper.GetAttr<PlayMakerFSM, Fsm>(worldMapFsm, "fsm").GlobalTransitions = tmpFsmGlobalTransitions;
            #endregion

            Log("... Added Custom Global Transition...");

            // Reference to GameManager for FSM actions
            var tmpGameObject = worldMapFsm.GetAction<PlayerDataBoolTest>("D Up", 2).gameObject;

            Log("... Create Custom States...");

            #region Create Custom States
            var customStateMain = worldMapFsm.CopyState("Mines", customAreaName);
            worldMapFsm.GetAction<SendEventByName>(customAreaName, 2).eventTarget.gameObject.GameObject = customPart;
            worldMapFsm.GetAction<GetLanguageString>(customAreaName, 3).convName = $"MAP_NAME_{customAreaName.ToUpper()}";
            worldMapFsm.GetAction<SetStringValue>(customAreaName, 5).stringValue = customAreaName.ToUpper();
            worldMapFsm.GetAction<SetVector3Value>(customAreaName, 6).vector3Value = new Vector3(2.07f, -20f, -22f);

            var customStateLeft = worldMapFsm.CopyState("T Left", $"{customAreaName} Left");

            var customStateRight = worldMapFsm.CopyState("T Right", $"{customAreaName} Right");

            var customStateDown = worldMapFsm.CopyState("Mi Left", $"{customAreaName} Down");

            var customStateZoom = worldMapFsm.CopyState("To Zoom 10", $"{customAreaName} Zoom");

            var townUp = worldMapFsm.CopyState("CR Up", "T Up");
            worldMapFsm.InsertAction("T Up", new PlayerDataBoolTest() { gameObject = tmpGameObject, boolName = "mapAbyss", isTrue = customGlobalEvent }, 0);

            var town = worldMapFsm.GetState("Town");
            List<FsmTransition> fsmTransitions = town.Transitions.ToList<FsmTransition>();
            fsmTransitions.Add(new FsmTransition() { FsmEvent = FsmEvent.FindEvent("UI UP"), ToState = "T Up" });
            town.Transitions = fsmTransitions.ToArray();
            #endregion

            worldMapFsm.ChangeTransition(customAreaName, "UI LEFT", $"{customAreaName} Left");
            worldMapFsm.ChangeTransition(customAreaName, "UI RIGHT", $"{customAreaName} Right");
            worldMapFsm.ChangeTransition(customAreaName, "UI DOWN", $"{customAreaName} Down");
            worldMapFsm.ChangeTransition(customAreaName, "UI CONFIRM", $"{customAreaName} Zoom");
            worldMapFsm.ChangeTransition($"{customAreaName} LEFT", "FINISHED", customAreaName);
            worldMapFsm.ChangeTransition($"{customAreaName} RIGHT", "FINISHED", customAreaName);
            worldMapFsm.ChangeTransition($"{customAreaName} Down", "FINISHED", customAreaName);

            worldMapFsm.AddTransition("Town", "UI UP", "T Up");
            //worldMapFsm.AddEventTransition("Town", "UI UP", "T Up");
            worldMapFsm.ChangeTransition("Town", "UI UP", "T Up");
            worldMapFsm.ChangeTransition("T Up", "FINISHED", "Town");



            var worldMapFsmVars = worldMapFsm.FsmVariables;
            Log("Name: " + worldMapFsm.GetAction<SendEventByName>("Reset", 8).sendEvent.Name);
            worldMapFsm.InsertAction("Reset", new ActualLogAction() { text = worldMapFsmVars.FindFsmString("Current Selection") }, 8);

            #endregion

            Log("~ChangeMap2");
        }
        #endregion

        private void printDebug(GameObject go, string tabindex = "", int parentCount = 0)
        {
            Transform parent = go.transform.parent;
            for (int i = 0; i < parentCount; i++)
            {
                if (parent != null)
                {
                    Log(tabindex + "DEBUG parent: " + parent.gameObject.name);
                    parent = parent.parent;
                }
            }
            Log(tabindex + "DEBUG Name: " + go.name);
            foreach (var comp in go.GetComponents<Component>())
            {
                Log(tabindex + "DEBUG Component: " + comp.GetType());
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                printDebug(go.transform.GetChild(i).gameObject, tabindex + "\t");
            }
        }

        private static void SetInactive(GameObject go)
        {
            if (go != null)
            {
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.SetActive(false);
            }
        }
        private static void SetInactive(UnityEngine.Object go)
        {
            if (go != null)
            {
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
        }
    }
}
