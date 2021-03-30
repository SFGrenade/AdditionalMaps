using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Security.Cryptography;
using AdditionalMaps.Consts;
using AdditionalMaps.MonoBehaviours;
using SFCore.Generics;
using SFCore.Utils;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;
using UObject = UnityEngine.Object;
using GlobalEnums;

namespace AdditionalMaps
{
    public class AdditionalMaps : FullSettingsMod<AmSaveSettings, AmGlobalSettings>
    {
        internal static AdditionalMaps Instance;

        public LanguageStrings langStrings { get; private set; }
        public TextureStrings spriteDict { get; private set; }

        private GameManager gm;
        private PlayerData pd;

        public static Sprite GetSprite(string name) => Instance.spriteDict.Get(name);
        public static Material defaultSpriteMaterial { get; private set; }

        private GameObject shinyPrefab;
        private PlayMakerFSM setCompassPointPrefab;
        private PlayMakerFSM setCompassPointRoomPrefab;

        public override string GetVersion() => SFCore.Utils.Util.GetVersion(Assembly.GetExecutingAssembly());

        public override List<ValueTuple<string, string>> GetPreloadNames()
        {
            return new List<ValueTuple<string, string>>
            {
                new ValueTuple<string, string>("Crossroads_33", "scatter_map 1"), // 64
                new ValueTuple<string, string>("Crossroads_33", "scatter_map 2"), // 64
                new ValueTuple<string, string>("Crossroads_33", "scatter_map 3"), // 64
                new ValueTuple<string, string>("Grimm_Divine", "Charm Holder"),
                new ValueTuple<string, string>("Town", "_Props/Stag_station/open/door_station"),
                new ValueTuple<string, string>("Room_mapper", "_SceneManager")
            };
        }

        public AdditionalMaps() : base("Additional Maps")
        {
            On.PlayMakerFSM.Start += OnPlayMakerFSMStart;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;
            gm = GameManager.instance;
            pd = PlayerData.instance;

            shinyPrefab = preloadedObjects["Grimm_Divine"]["Charm Holder"];
            {
                UObject.Destroy(shinyPrefab.transform.GetChild(2));
                UObject.Destroy(shinyPrefab.transform.GetChild(1));
                UObject.Destroy(shinyPrefab.transform.GetChild(0).gameObject.GetComponent<PersistentBoolItem>());
            }
            SetInactive(shinyPrefab);
            setCompassPointPrefab = preloadedObjects["Town"]["_Props/Stag_station/open/door_station"].LocateMyFSM("Set Compass Point");
            setCompassPointRoomPrefab = preloadedObjects["Room_mapper"]["_SceneManager"].LocateMyFSM("map_isroom");

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

            GameMapHooks.Init(gameMapCallback);

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

            #region Prefabs

            var areaNamePrefab = GameObject.Instantiate(gameMapBetter.areaCliffs.transform.GetChild(0).gameObject);
            areaNamePrefab.SetActive(false);
            var subAreaPrefab = GameObject.Instantiate(gameMapBetter.areaCliffs.transform.GetChild(6).GetChild(0).gameObject);
            subAreaPrefab.SetActive(false);
            var roomMat = UnityEngine.Object.Instantiate(gameMapBetter.areaCliffs.transform.GetChild(1).GetComponent<SpriteRenderer>().material);
            defaultSpriteMaterial = roomMat;
            var benchPrefab = GameObject.Instantiate(gameMapBetter.areaCliffs.transform.GetChild(3).GetChild(2).gameObject);
            benchPrefab.SetActive(false);

            var tmpDict = new Dictionary<string, s_CustomArea>();

            #endregion

            #region White Palace Map

            var AreaWhitePalace = GameObject.Instantiate(gameMapBetter.areaCliffs, gameMapBetter.transform);
            AreaWhitePalace.SetActive(true);

            for (int i = 0; i < AreaWhitePalace.transform.childCount; i++)
            {
                GameObject.Destroy(AreaWhitePalace.transform.GetChild(i).gameObject);
            }

            AreaWhitePalace.name = "WHITE_PALACE";
            AreaWhitePalace.layer = 5;
            AreaWhitePalace.transform.localScale = Vector3.one;
            AreaWhitePalace.transform.localPosition = new Vector3(-2.0f, 15f, gameMapBetter.areaCliffs.transform.localPosition.z);

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

            wpScenes[17].transform.localScale = new Vector3(0.93f, 1.04f, 1.0f);

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

            #region Benches

            var tmp = GameObject.Instantiate(benchPrefab, wpRoomSprites[0].transform);
            tmp.transform.localPosition = new Vector3(-0.4f, -0.5f, -0.013f);
            tmp.SetActive(true);
            var tmp2 = GameObject.Instantiate(benchPrefab, wpRoomSprites[2].transform);
            tmp2.transform.localPosition = new Vector3(0.05f, -0.15f, -0.013f);
            tmp2.SetActive(true);
            var tmp3 = GameObject.Instantiate(benchPrefab, wpRoomSprites[5].transform);
            tmp3.transform.localPosition = new Vector3(-0.1f, 0.45f, -0.013f);
            tmp3.SetActive(true);

            #endregion

            #region Area Name

            var wpAreaNameArea = GameObject.Instantiate(areaNamePrefab, AreaWhitePalace.transform);
            wpAreaNameArea.transform.localPosition = new Vector3(6.433125f, 1.6825f, wpAreaNameArea.transform.localPosition.z);
            wpAreaNameArea.GetComponent<SetTextMeshProGameText>().convName = "WHITE_PALACE";
            wpAreaNameArea.SetActive(true);

            #endregion

            AreaWhitePalace.SetActive(true);
            tmpDict.Add(
                "WHITE_PALACE",
                new s_CustomArea()
                {
                    areaGameObject = AreaWhitePalace,
                    cameraPosition = new Vector3(3.07f, -24.5f, 18f),
                    mapZoneStrings = new List<string>() { "WHITE_PALACE" },
                    //playerDataBoolGotAreaMap = "AdditionalMapsGotWpMap"
                    playerDataBoolGotAreaMap = "AdditionalMapsGotWpMap"
                }
            );

            #endregion

            #region Godhome Map

            var AreaGodhome = GameObject.Instantiate(gameMapBetter.areaCliffs, gameMapBetter.transform);
            AreaGodhome.SetActive(true);

            for (int i = 0; i < AreaGodhome.transform.childCount; i++)
            {
                GameObject.Destroy(AreaGodhome.transform.GetChild(i).gameObject);
            }

            AreaGodhome.name = "GODS_GLORY";
            AreaGodhome.layer = 5;
            AreaGodhome.transform.localScale = Vector3.one;
            AreaGodhome.transform.localPosition = new Vector3(5.5f, 14.5f, gameMapBetter.areaCliffs.transform.localPosition.z);

            List<GameObject> ghScenes = new List<GameObject>() {
                new GameObject("GG_Atrium", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
                new GameObject("GG_Atrium_Roof", typeof(SpriteRenderer), typeof(RoughMapRoom), typeof(MappedCustomRoom)),
            };
            foreach (var sceneGo in ghScenes)
            {
                sceneGo.transform.SetParent(AreaGodhome.transform);
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
            }
            ghScenes[0].transform.localPosition = new Vector3(0.3687f, -2.678f, tmpChildZ);
            ghScenes[1].transform.localPosition = new Vector3(-0.708f, 0.65f, tmpChildZ);

            List<GameObject> ghRoomSprites = new List<GameObject>() {
                new GameObject("RGH0", typeof(SpriteRenderer)),
                new GameObject("RGH1", typeof(SpriteRenderer)),
            };
            ghRoomSprites[0].transform.SetParent(ghScenes[0].transform);
            ghRoomSprites[0].transform.localPosition = new Vector3(-0.408678f, 0.138f);
            ghRoomSprites[1].transform.SetParent(ghScenes[1].transform);
            ghRoomSprites[1].transform.localPosition = new Vector3(0.6480024f, -0.187f);
            foreach (var sprite in ghRoomSprites)
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

            var creditsArea2 = GameObject.Instantiate(subAreaPrefab, ghScenes[0].transform);
            creditsArea2.SetActive(true);
            creditsArea2.transform.localPosition = new Vector3(8f, 1.5f, creditsArea2.transform.localPosition.z);
            creditsArea2.GetComponent<SetTextMeshProGameText>().convName = LanguageStrings.Credits_Key;
            var rectT2 = creditsArea2.GetComponent<RectTransform>();
            rectT2.sizeDelta = new Vector2(rectT2.sizeDelta.x + 1, rectT2.sizeDelta.y);

            #region Benches

            var tmpBench = GameObject.Instantiate(benchPrefab, ghRoomSprites[0].transform);
            tmpBench.transform.localPosition = new Vector3(0.925f, 0.5f, -0.013f);
            tmpBench.SetActive(true);
            var tmpBench2 = GameObject.Instantiate(benchPrefab, ghRoomSprites[1].transform);
            tmpBench2.transform.localPosition = new Vector3(0.8f, -0.1f, -0.013f);
            tmpBench2.SetActive(true);

            #endregion

            #region Area Name

            var ghAreaNameArea = GameObject.Instantiate(areaNamePrefab, AreaGodhome.transform);
            ghAreaNameArea.transform.localPosition = new Vector3(5.208f, 0.65f, ghAreaNameArea.transform.localPosition.z);
            ghAreaNameArea.GetComponent<SetTextMeshProGameText>().convName = "GODS_GLORY";
            ghAreaNameArea.SetActive(true);

            #endregion

            AreaGodhome.SetActive(true);
            tmpDict.Add(
                "GODS_GLORY",
                new s_CustomArea()
                {
                    areaGameObject = AreaGodhome,
                    cameraPosition = new Vector3(-8.5f, -22f, 18f),
                    mapZoneStrings = new List<string>() { "GODS_GLORY" },
                    playerDataBoolGotAreaMap = "AdditionalMapsGotGhMap"
                }
            );

            #endregion

            GameObject.Destroy(subAreaPrefab);
            Log("~gameMapCallback");
            return tmpDict;
        }

        private void initGlobalSettings()
        {
            // Found in a project, might help saving, don't know, but who cares
        }

        private void initSaveSettings(SaveGameData data)
        {
            // Found in a project, might help saving, don't know, but who cares
        }

        private void initCallbacks()
        {
            // Hooks
            ModHooks.Instance.LanguageGetHook += OnLanguageGetHook;
            ModHooks.Instance.GetPlayerBoolHook += OnGetPlayerBoolHook;
            ModHooks.Instance.SetPlayerBoolHook += OnSetPlayerBoolHook;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneManagerActiveSceneChanged;
        }

        private List<string> sceneList = new List<string>()
        {
            "White_Palace_08",
            "GG_Atrium",
            "GG_Workshop",
            "GG_Blue_Room"
        };
        private void OnSceneManagerActiveSceneChanged(Scene from, Scene to)
        {
            if (!sceneList.Contains(to.name)) return;

            // ToDo make this optional
            if (to.name.Equals("White_Palace_08"))
            {
                var sm = to.Find("_SceneManager").GetComponent<SceneManager>();
                sm.mapZone = MapZone.WHITE_PALACE;

                MakeSpriteGo("sm1", GetSprite(TextureStrings.SM1Key), new Vector3(104.28f, 21.07f, 0.1f), Vector3.zero);
                MakeSpriteGo("sm2", GetSprite(TextureStrings.SM2Key), new Vector3(104.33f, 19.2f, 0.1f), new Vector3(0, 0, -1.374f));
                MakeSpriteGo("sm2", GetSprite(TextureStrings.SM2Key), new Vector3(105.53f, 21.15f, 0.17f), new Vector3(0, 0, 14.291f));
                MakeSpriteGo("sm3", GetSprite(TextureStrings.SM3Key), new Vector3(106.33f, 19.4f, 0.1f), Vector3.zero);
                MakeSpriteGo("sm3", GetSprite(TextureStrings.SM3Key), new Vector3(106.83f, 21.15f, 0.1f), new Vector3(0, 0, -5.748f));

                #region Shiny FSM

                GameObject shinyParent = GameObject.Instantiate(shinyPrefab);
                shinyParent.name = "Map";
                shinyParent.SetActive(false);
                shinyParent.transform.GetChild(0).gameObject.SetActive(true);
                shinyParent.transform.position = new Vector3(105.53f, 19.51f, 0.05f);
                //shinyParent.transform.position = new Vector3(55, 47, 0.05f);

                PlayMakerFSM shinyFsm = shinyParent.transform.GetChild(0).gameObject.LocateMyFSM("Shiny Control");
                FsmVariables shinyFsmVars = shinyFsm.FsmVariables;
                shinyFsmVars.FindFsmInt("Charm ID").Value = 0;
                shinyFsmVars.FindFsmInt("Type").Value = 0;
                shinyFsmVars.FindFsmBool("Activated").Value = false;
                shinyFsmVars.FindFsmBool("Charm").Value = false;
                shinyFsmVars.FindFsmBool("Dash Cloak").Value = false;
                shinyFsmVars.FindFsmBool("Exit Dream").Value = false;
                shinyFsmVars.FindFsmBool("Fling L").Value = false;
                shinyFsmVars.FindFsmBool("Fling On Start").Value = false;
                shinyFsmVars.FindFsmBool("Journal").Value = false;
                shinyFsmVars.FindFsmBool("King's Brand").Value = false;
                shinyFsmVars.FindFsmBool("Mantis Claw").Value = false;
                shinyFsmVars.FindFsmBool("Pure Seed").Value = false;
                shinyFsmVars.FindFsmBool("Quake").Value = false;
                shinyFsmVars.FindFsmBool("Show Charm Tute").Value = false;
                shinyFsmVars.FindFsmBool("Slug Fling").Value = false;
                shinyFsmVars.FindFsmBool("Super Dash").Value = false;
                shinyFsmVars.FindFsmString("Item Name").Value = LanguageStrings.Wp_Map_Key;
                shinyFsmVars.FindFsmString("PD Bool Name").Value = "AdditionalMapsGotWpMap";

                IntSwitch isAction = shinyFsm.GetAction<IntSwitch>("Trinket Type", 0);
                var tmpCompareTo = new List<FsmInt>(isAction.compareTo);
                tmpCompareTo.Add(tmpCompareTo.Count + 1);
                isAction.compareTo = tmpCompareTo.ToArray();
                shinyFsmVars.FindFsmInt("Trinket Num").Value = tmpCompareTo.Count;
                var tmpSendEvent = new List<FsmEvent>(isAction.sendEvent);
                tmpSendEvent.Add(FsmEvent.FindEvent("PURE SEED"));
                isAction.sendEvent = tmpSendEvent.ToArray();

                shinyFsm.CopyState("Love Key", "Necklace");

                shinyFsm.GetAction<SetPlayerDataBool>("Necklace", 0).boolName = "AdditionalMapsGotWpMap";
                shinyFsm.GetAction<SetSpriteRendererSprite>("Necklace", 1).sprite = GetSprite(TextureStrings.MapKey);
                shinyFsm.GetAction<GetLanguageString>("Necklace", 2).convName = LanguageStrings.Wp_Map_Key;

                shinyFsm.AddTransition("Trinket Type", "PURE SEED", "Necklace");

                shinyParent.SetActive(true);

                #endregion
            }
            else if (to.name.Equals("GG_Atrium"))
            {
                var blueDoor = to.Find("door1_blueRoom");
                setCompassPointPrefab.CopyOnto(blueDoor);
                var fsm = blueDoor.LocateMyFSM("Set Compass Point");
                fsm.Preprocess();
                var workshopDoor = to.Find("Door_Workshop");
                setCompassPointPrefab.CopyOnto(workshopDoor);
                var fsm2 = workshopDoor.LocateMyFSM("Set Compass Point");
                fsm2.Preprocess();

                #region Shiny FSM

                GameObject shinyParent = GameObject.Instantiate(shinyPrefab);
                shinyParent.name = "Map";
                shinyParent.SetActive(false);
                shinyParent.transform.GetChild(0).gameObject.SetActive(true);
                shinyParent.transform.position = new Vector3(118.03f, 60.51f, 0.05f);
                //shinyParent.transform.position = new Vector3(55, 47, 0.05f);

                PlayMakerFSM shinyFsm = shinyParent.transform.GetChild(0).gameObject.LocateMyFSM("Shiny Control");
                FsmVariables shinyFsmVars = shinyFsm.FsmVariables;
                shinyFsmVars.FindFsmInt("Charm ID").Value = 0;
                shinyFsmVars.FindFsmInt("Type").Value = 0;
                shinyFsmVars.FindFsmBool("Activated").Value = false;
                shinyFsmVars.FindFsmBool("Charm").Value = false;
                shinyFsmVars.FindFsmBool("Dash Cloak").Value = false;
                shinyFsmVars.FindFsmBool("Exit Dream").Value = false;
                shinyFsmVars.FindFsmBool("Fling L").Value = false;
                shinyFsmVars.FindFsmBool("Fling On Start").Value = false;
                shinyFsmVars.FindFsmBool("Journal").Value = false;
                shinyFsmVars.FindFsmBool("King's Brand").Value = false;
                shinyFsmVars.FindFsmBool("Mantis Claw").Value = false;
                shinyFsmVars.FindFsmBool("Pure Seed").Value = false;
                shinyFsmVars.FindFsmBool("Quake").Value = false;
                shinyFsmVars.FindFsmBool("Show Charm Tute").Value = false;
                shinyFsmVars.FindFsmBool("Slug Fling").Value = false;
                shinyFsmVars.FindFsmBool("Super Dash").Value = false;
                shinyFsmVars.FindFsmString("Item Name").Value = LanguageStrings.Gh_Map_Key;
                shinyFsmVars.FindFsmString("PD Bool Name").Value = "AdditionalMapsGotGhMap";

                IntSwitch isAction = shinyFsm.GetAction<IntSwitch>("Trinket Type", 0);
                var tmpCompareTo = new List<FsmInt>(isAction.compareTo);
                tmpCompareTo.Add(tmpCompareTo.Count + 1);
                isAction.compareTo = tmpCompareTo.ToArray();
                shinyFsmVars.FindFsmInt("Trinket Num").Value = tmpCompareTo.Count;
                var tmpSendEvent = new List<FsmEvent>(isAction.sendEvent);
                tmpSendEvent.Add(FsmEvent.FindEvent("PURE SEED"));
                isAction.sendEvent = tmpSendEvent.ToArray();

                shinyFsm.CopyState("Love Key", "Necklace");

                shinyFsm.GetAction<SetPlayerDataBool>("Necklace", 0).boolName = "AdditionalMapsGotGhMap";
                shinyFsm.GetAction<SetSpriteRendererSprite>("Necklace", 1).sprite = GetSprite(TextureStrings.MapKey);
                shinyFsm.GetAction<GetLanguageString>("Necklace", 2).convName = LanguageStrings.Gh_Map_Key;

                shinyFsm.AddTransition("Trinket Type", "PURE SEED", "Necklace");

                shinyParent.SetActive(true);

                #endregion
            }
            else if (to.name.Equals("GG_Workshop"))
            {
                var smGo = to.Find("_SceneManager");
                setCompassPointRoomPrefab.CopyOnto(smGo);
                var fsm = smGo.LocateMyFSM("map_isroom");
                fsm.Preprocess();
                //var fsmVars = fsm.FsmVariables;
                //fsmVars.GetFsmString("Map Zone").Value = "GODS_GLORY";
                //fsmVars.GetFsmString("Scene Name").Value = "GG_Atrium";
            }
            else if (to.name.Equals("GG_Blue_Room"))
            {
                var smGo = to.Find("_SceneManager");
                setCompassPointRoomPrefab.CopyOnto(smGo);
                var fsm = smGo.LocateMyFSM("map_isroom");
                fsm.Preprocess();
                //var fsmVars = fsm.FsmVariables;
                //fsmVars.GetFsmString("Map Zone").Value = "GODS_GLORY";
                //fsmVars.GetFsmString("Scene Name").Value = "GG_Atrium";
            }
        }

        private void MakeSpriteGo(string name, Sprite sprite, Vector3 pos, Vector3 angles)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.eulerAngles = angles;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.material = new Material(Shader.Find("Sprites/Default"));
            sr.sprite = sprite;
        }

        private void OnPlayMakerFSMStart(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.FsmName.Equals("UI Control") && self.gameObject.name.Equals("World Map"))
            {
                ChangeWpMap(self.gameObject, self.transform.GetChild(4).gameObject);
                ChangeGhMap(self.gameObject, self.transform.GetChild(4).gameObject);
            }
        }

        private void SaveTotGlobalSettings()
        {
            SaveGlobalSettings();
        }

        #region Get/Set Hooks
        private string OnLanguageGetHook(string key, string sheet)
        {
            //Log($"Sheet: {sheet}; Key: {key}");
            if (key.ToUpper().Contains("GOD"))
            {
                Log($"Sheet: {sheet}; Key: {key}");
            }
            if (langStrings.ContainsKey(key, sheet))
            {
                return langStrings.Get(key, sheet);
            }
            return Language.Language.GetInternal(key, sheet);
        }

        private bool HasGetSettingsValue<T>(string target)
        {
            var tmpField = _saveSettingsType.GetField(target);
            return tmpField != null && tmpField.FieldType == typeof(T);
        }
        private T GetSettingsValue<T>(string target)
        {
            var tmpField = _saveSettingsType.GetField(target);
            return (T) tmpField.GetValue(_saveSettings);
        }
        private void SetSettingsValue<T>(string target, T val)
        {
            var tmpField = _saveSettingsType.GetField(target);
            tmpField.SetValue(_saveSettings, val);
        }

        private bool OnGetPlayerBoolHook(string target)
        {
            if (HasGetSettingsValue<bool>(target))
            {
                return GetSettingsValue<bool>(target);
            }
            return PlayerData.instance.GetBoolInternal(target);
        }
        private void OnSetPlayerBoolHook(string target, bool val)
        {
            if (HasGetSettingsValue<bool>(target))
            {
                SetSettingsValue<bool>(target, val);
                // trigger map updated thing
                GameManager.instance.UpdateGameMap();
                GameObject.FindObjectOfType<GameMap>().SetupMap(false);

                Resources.FindObjectsOfTypeAll<Transform>().First(t => t.gameObject.name.Equals("Map Update Msg"))
                    .gameObject.Spawn(Vector3.zero);
                return;
            }
            PlayerData.instance.SetBoolInternal(target, val);
        }

        private int OnGetPlayerIntHook(string target)
        {
            if (HasGetSettingsValue<int>(target))
            {
                return GetSettingsValue<int>(target);
            }
            return PlayerData.instance.GetIntInternal(target);
        }
        private void OnSetPlayerIntHook(string target, int val)
        {
            if (HasGetSettingsValue<int>(target))
            {
                SetSettingsValue<int>(target, val);
                return;
            }
            PlayerData.instance.SetIntInternal(target, val);
        }
        #endregion

        #region Custom Area Test

        private void ChangeWpMap(GameObject worldMap, GameObject wideMap)
        {
            DebugLog($"!ChangeWpMap: \"{wideMap}\"");

            string customAreaName = "White_Palace";
            string boolName = "AdditionalMapsGotWpMap";
            Vector3 CameraZoomPosition = new Vector3(2.07f, -20f, -22f);
            Vector3 MapAreaPosition = new Vector3(1.02f, -1.75f, -2.3f);

            bool tmpActive = wideMap.activeSelf;
            wideMap.SetActive(false);

            #region temporary Variables

            string caState = $"{customAreaName} State";
            string caLeftState = $"{customAreaName} State Left";
            string caRightState = $"{customAreaName} State Right";
            string caUpState = $"{customAreaName} State Up";
            string caDownState = $"{customAreaName} State Down";
            string caZoomState = $"{customAreaName} State Zoom";

            string extraUpState = "T Up";

            #endregion

            #region Add sprite and text for custom area
            var customPart = GameObject.Instantiate(wideMap.transform.GetChild(0).gameObject, wideMap.transform, true);
            customPart.SetActive(false);
            customPart.name = customAreaName;
            customPart.transform.localPosition = MapAreaPosition;
            customPart.GetComponent<SpriteRenderer>().sprite = GetSprite(TextureStrings.CustomAreaKey);
            customPart.GetComponentInChildren<SetTextMeshProGameText>().convName = customAreaName.ToUpper();
            customPart.transform.Find("Area Name").localPosition += new Vector3(-1.0f, 0, 0);
            customPart.LocateMyFSM("deactivate").FsmVariables.GetFsmString("playerData bool").Value = boolName;
            #endregion

            #region Edit World Map - UI Control FSM
            var worldMapFsm = worldMap.LocateMyFSM("UI Control");

            if (worldMapFsm.GetState("Mines").Fsm == null)
            {
                worldMapFsm.Preprocess();
            }

            var wmUCFsmVars = worldMapFsm.FsmVariables;

            #region Create Custom States
            worldMapFsm.CopyState("Mines", caState);

            worldMapFsm.CopyState("T Left", caLeftState);
            worldMapFsm.CopyState("T Right", caRightState);

            worldMapFsm.CopyState("Mi Left", caDownState);
            worldMapFsm.CopyState("To Zoom 10", caZoomState);

            worldMapFsm.CopyState("CR Up", extraUpState);
            #endregion

            #region Add Custom FSM Variable
            worldMapFsm.AddGameObjectVariable(customAreaName);
            #endregion
            #region Add FindChild Action to store Custom Area Sprite
            FindChild tmpActionFindChild = new FindChild();
            tmpActionFindChild.gameObject = worldMapFsm.GetAction<FindChild>("Init", 10).gameObject;
            tmpActionFindChild.childName = customAreaName;
            tmpActionFindChild.storeResult = wmUCFsmVars.GetFsmGameObject(customAreaName);
            worldMapFsm.InsertAction("Init", tmpActionFindChild, 11);
            #endregion

            #region Add Custom Global Transition
            var customGlobalEvent = worldMapFsm.AddGlobalTransition($"{customAreaName.ToUpper()}_GLOBAL", caState);
            #endregion

            DebugLog("... Added Custom Global Transition...");

            // Reference to GameManager for FSM actions
            var tmpGameObject = worldMapFsm.GetAction<PlayerDataBoolTest>("D Up", 2).gameObject;

            DebugLog("... Create Custom States...");

            #region Create Custom States
            worldMapFsm.GetAction<SendEventByName>(caState, 2).eventTarget = new FsmEventTarget() { target = FsmEventTarget.EventTarget.GameObject, gameObject = new FsmOwnerDefault() { OwnerOption = OwnerDefaultOption.SpecifyGameObject, GameObject = customPart } };
            worldMapFsm.GetAction<GetLanguageString>(caState, 3).convName = $"MAP_NAME_{customAreaName.ToUpper()}";
            worldMapFsm.GetAction<SetStringValue>(caState, 5).stringValue = customAreaName.ToUpper();
            worldMapFsm.GetAction<SetVector3Value>(caState, 6).vector3Value = CameraZoomPosition;

            worldMapFsm.InsertAction(extraUpState, new PlayerDataBoolTest() { gameObject = tmpGameObject, boolName = boolName, isTrue = customGlobalEvent }, 0);
            #endregion

            worldMapFsm.ChangeTransition(caState, "UI LEFT", caLeftState);
            worldMapFsm.ChangeTransition(caState, "UI RIGHT", caRightState);
            worldMapFsm.ChangeTransition(caState, "UI DOWN", caDownState);
            worldMapFsm.ChangeTransition(caState, "UI CONFIRM", caZoomState);
            worldMapFsm.ChangeTransition(caLeftState, "FINISHED", caState);
            worldMapFsm.ChangeTransition(caRightState, "FINISHED", caState);
            worldMapFsm.ChangeTransition(caDownState, "FINISHED", caState);

            worldMapFsm.AddTransition("Town", "UI UP", extraUpState);
            worldMapFsm.ChangeTransition("Town", "UI UP", extraUpState);
            worldMapFsm.AddTransition(extraUpState, "FINISHED", "Town");
            worldMapFsm.ChangeTransition(extraUpState, "FINISHED", "Town");

            var worldMapFsmVars = worldMapFsm.FsmVariables;
            #endregion
            wideMap.SetActive(tmpActive);

            DebugLog("~ChangeWpMap");
        }
        
        private void ChangeGhMap(GameObject worldMap, GameObject wideMap)
        {
            DebugLog($"!ChangeWpMap: \"{wideMap}\"");

            string customAreaName = "Godhome";
            string boolName = "AdditionalMapsGotGhMap";
            Vector3 CameraZoomPosition = new Vector3(-8.07f, -16f, -22f);
            Vector3 MapAreaPostion = new Vector3(6.02f, -2f, -2.3f);

            bool tmpActive = wideMap.activeSelf;
            wideMap.SetActive(false);

            #region temporary Variables

            string caState = $"{customAreaName} State";
            string caLeftState = $"{customAreaName} State Left";
            string caRightState = $"{customAreaName} State Right";
            string caUpState = $"{customAreaName} State Up";
            string caDownState = $"{customAreaName} State Down";
            string caZoomState = $"{customAreaName} State Zoom";

            string extraUpState = "Mi Up";

            #endregion

            #region Add sprite and text for custom area
            var customPart = GameObject.Instantiate(wideMap.transform.GetChild(0).gameObject, wideMap.transform, true);
            customPart.SetActive(false);
            customPart.name = customAreaName;
            customPart.transform.localPosition = MapAreaPostion;
            customPart.GetComponent<SpriteRenderer>().sprite = GetSprite(TextureStrings.CustomAreaKey);
            customPart.GetComponentInChildren<SetTextMeshProGameText>().convName = customAreaName.ToUpper();
            customPart.transform.Find("Area Name").localPosition += new Vector3(-1.0f, 0, 0);
            customPart.LocateMyFSM("deactivate").FsmVariables.GetFsmString("playerData bool").Value = boolName;
            #endregion

            #region Edit World Map - UI Control FSM
            var worldMapFsm = worldMap.LocateMyFSM("UI Control");

            if (worldMapFsm.GetState("Mines").Fsm == null)
            {
                worldMapFsm.Preprocess();
            }

            var wmUCFsmVars = worldMapFsm.FsmVariables;

            #region Create Custom States
            worldMapFsm.CopyState("Mines", caState);

            worldMapFsm.CopyState("Mi Left", caLeftState);
            worldMapFsm.CopyState("Mi Right", caRightState);

            worldMapFsm.CopyState("T Right", caDownState);
            worldMapFsm.CopyState("To Zoom 11", caZoomState);

            worldMapFsm.CopyState("RG Up", extraUpState);
            #endregion

            #region Add Custom FSM Variable
            worldMapFsm.AddGameObjectVariable(customAreaName);
            #endregion
            #region Add FindChild Action to store Custom Area Sprite
            FindChild tmpActionFindChild = new FindChild();
            tmpActionFindChild.gameObject = worldMapFsm.GetAction<FindChild>("Init", 10).gameObject;
            tmpActionFindChild.childName = customAreaName;
            tmpActionFindChild.storeResult = wmUCFsmVars.GetFsmGameObject(customAreaName);
            worldMapFsm.InsertAction("Init", tmpActionFindChild, 11);
            #endregion

            #region Add Custom Global Transition
            var customGlobalEvent = worldMapFsm.AddGlobalTransition($"{customAreaName.ToUpper()}_GLOBAL", caState);
            #endregion

            DebugLog("... Added Custom Global Transition...");

            // Reference to GameManager for FSM actions
            var tmpGameObject = worldMapFsm.GetAction<PlayerDataBoolTest>("D Up", 2).gameObject;

            DebugLog("... Create Custom States...");

            #region Create Custom States
            worldMapFsm.GetAction<SendEventByName>(caState, 2).eventTarget = new FsmEventTarget() { target = FsmEventTarget.EventTarget.GameObject, gameObject = new FsmOwnerDefault() { OwnerOption = OwnerDefaultOption.SpecifyGameObject, GameObject = customPart } };
            worldMapFsm.GetAction<GetLanguageString>(caState, 3).convName = $"MAP_NAME_{customAreaName.ToUpper()}";
            worldMapFsm.GetAction<SetStringValue>(caState, 5).stringValue = customAreaName.ToUpper();
            worldMapFsm.GetAction<SetVector3Value>(caState, 6).vector3Value = CameraZoomPosition;

            worldMapFsm.InsertAction(extraUpState, new PlayerDataBoolTest() { gameObject = tmpGameObject, boolName = boolName, isTrue = customGlobalEvent }, 0);
            #endregion

            worldMapFsm.ChangeTransition(caState, "UI LEFT", caLeftState);
            worldMapFsm.ChangeTransition(caState, "UI RIGHT", caRightState);
            worldMapFsm.ChangeTransition(caState, "UI DOWN", caDownState);
            worldMapFsm.ChangeTransition(caState, "UI CONFIRM", caZoomState);
            worldMapFsm.ChangeTransition(caLeftState, "FINISHED", caState);
            worldMapFsm.ChangeTransition(caRightState, "FINISHED", caState);
            worldMapFsm.ChangeTransition(caDownState, "FINISHED", caState);

            worldMapFsm.AddTransition("Mines", "UI UP", extraUpState);
            worldMapFsm.ChangeTransition("Mines", "UI UP", extraUpState);
            worldMapFsm.AddTransition(extraUpState, "FINISHED", "Mines");
            worldMapFsm.ChangeTransition(extraUpState, "FINISHED", "Mines");

            var worldMapFsmVars = worldMapFsm.FsmVariables;
            #endregion
            wideMap.SetActive(tmpActive);

            DebugLog("~ChangeWpMap");
        }

        #endregion

        private static void DebugLog(String msg)
        {
            Logger.Log($"[AdditionalMaps] - {msg}");
            Debug.Log($"[AdditionalMaps] - {msg}");
        }
        private static void DebugLog(object msg)
        {
            DebugLog($"{msg}");
        }
        private static void SetInactive(GameObject go)
        {
            if (go != null)
            {
                UObject.DontDestroyOnLoad(go);
                go.SetActive(false);
            }
        }
        private static void SetInactive(UnityEngine.Object go)
        {
            if (go != null)
            {
                UObject.DontDestroyOnLoad(go);
            }
        }
    }
}
