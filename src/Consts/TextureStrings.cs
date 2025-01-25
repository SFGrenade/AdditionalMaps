using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AdditionalMaps.Consts;

public class TextureStrings
{
    #region Texture Keys and Files
    // Custom Area & Rooms
    public const string CustomAreaKey = "CustomArea";
    private const string CustomAreaFile = "AdditionalMaps.Resources.CustomArea.png";
    public const string CustomGhAreaKey = "CustomArea.GH";
    private const string CustomGhAreaFile = "AdditionalMaps.Resources.CustomArea.GH.png";
    public const string CustomWpAreaKey = "CustomArea.WP";
    private const string CustomWpAreaFile = "AdditionalMaps.Resources.CustomArea.WP.png";
    public const string Wp01Key = "White_Palace_01";
    private const string Wp01File = "AdditionalMaps.Resources.White_Palace.WP01.png";
    public const string Wp02Key = "White_Palace_02";
    private const string Wp02File = "AdditionalMaps.Resources.White_Palace.WP02.png";
    public const string Wp03Key = "White_Palace_03_hub";
    private const string Wp03File = "AdditionalMaps.Resources.White_Palace.WP03.png";
    public const string Wp04Key = "White_Palace_04";
    private const string Wp04File = "AdditionalMaps.Resources.White_Palace.WP04.png";
    public const string Wp05Key = "White_Palace_05";
    private const string Wp05File = "AdditionalMaps.Resources.White_Palace.WP05.png";
    public const string Wp06Key = "White_Palace_06";
    private const string Wp06File = "AdditionalMaps.Resources.White_Palace.WP06.png";
    public const string Wp07Key = "White_Palace_07";
    private const string Wp07File = "AdditionalMaps.Resources.White_Palace.WP07.png";
    public const string Wp08Key = "White_Palace_08";
    private const string Wp08File = "AdditionalMaps.Resources.White_Palace.WP08.png";
    public const string Wp09Key = "White_Palace_09";
    private const string Wp09File = "AdditionalMaps.Resources.White_Palace.WP09.png";
    public const string Wp12Key = "White_Palace_12";
    private const string Wp12File = "AdditionalMaps.Resources.White_Palace.WP12.png";
    public const string Wp13Key = "White_Palace_13";
    private const string Wp13File = "AdditionalMaps.Resources.White_Palace.WP13.png";
    public const string Wp14Key = "White_Palace_14";
    private const string Wp14File = "AdditionalMaps.Resources.White_Palace.WP14.png";
    public const string Wp15Key = "White_Palace_15";
    private const string Wp15File = "AdditionalMaps.Resources.White_Palace.WP15.png";
    public const string Wp16Key = "White_Palace_16";
    private const string Wp16File = "AdditionalMaps.Resources.White_Palace.WP16.png";
    public const string Wp17Key = "White_Palace_17";
    private const string Wp17File = "AdditionalMaps.Resources.White_Palace.WP17.png";
    public const string Wp18Key = "White_Palace_18";
    private const string Wp18File = "AdditionalMaps.Resources.White_Palace.WP18.png";
    public const string Wp19Key = "White_Palace_19";
    private const string Wp19File = "AdditionalMaps.Resources.White_Palace.WP19.png";
    public const string Wp20Key = "White_Palace_20";
    private const string Wp20File = "AdditionalMaps.Resources.White_Palace.WP20.png";
    public const string WpMapKey = "WPMap";
    private const string WpMapFile = "AdditionalMaps.Resources.White_Palace.White_Palace_Transparent.png";
    public const string RealWp01Key = "RWP01";
    private const string RealWp01File = "AdditionalMaps.Resources.White_Palace.RWP01.png";
    public const string RealWp02Key = "RWP02";
    private const string RealWp02File = "AdditionalMaps.Resources.White_Palace.RWP02.png";
    public const string RealWp03Key = "RWP03";
    private const string RealWp03File = "AdditionalMaps.Resources.White_Palace.RWP03.png";
    public const string RealWp04Key = "RWP04";
    private const string RealWp04File = "AdditionalMaps.Resources.White_Palace.RWP04.png";
    public const string RealWp05Key = "RWP05";
    private const string RealWp05File = "AdditionalMaps.Resources.White_Palace.RWP05.png";
    public const string RealWp06Key = "RWP06";
    private const string RealWp06File = "AdditionalMaps.Resources.White_Palace.RWP06.png";
    public const string RealWp07Key = "RWP07";
    private const string RealWp07File = "AdditionalMaps.Resources.White_Palace.RWP07.png";
    public const string RealWp08Key = "RWP08";
    private const string RealWp08File = "AdditionalMaps.Resources.White_Palace.RWP08.png";
    public const string RealWp09Key = "RWP09";
    private const string RealWp09File = "AdditionalMaps.Resources.White_Palace.RWP09.png";
    public const string RealWp12Key = "RWP12";
    private const string RealWp12File = "AdditionalMaps.Resources.White_Palace.RWP12.png";
    public const string RealWp13Key = "RWP13";
    private const string RealWp13File = "AdditionalMaps.Resources.White_Palace.RWP13.png";
    public const string RealWp14Key = "RWP14";
    private const string RealWp14File = "AdditionalMaps.Resources.White_Palace.RWP14.png";
    public const string RealWp15Key = "RWP15";
    private const string RealWp15File = "AdditionalMaps.Resources.White_Palace.RWP15.png";
    public const string RealWp16Key = "RWP16";
    private const string RealWp16File = "AdditionalMaps.Resources.White_Palace.RWP16.png";
    public const string RealWp17Key = "RWP17";
    private const string RealWp17File = "AdditionalMaps.Resources.White_Palace.RWP17.png";
    public const string RealWp18Key = "RWP18";
    private const string RealWp18File = "AdditionalMaps.Resources.White_Palace.RWP18.png";
    public const string RealWp19Key = "RWP19";
    private const string RealWp19File = "AdditionalMaps.Resources.White_Palace.RWP19.png";
    public const string RealWp20Key = "RWP20";
    private const string RealWp20File = "AdditionalMaps.Resources.White_Palace.RWP20.png";
    public const string Sm1Key = "SM1";
    private const string Sm1File = "AdditionalMaps.Resources.scattermaps1.png";
    public const string Sm2Key = "SM2";
    private const string Sm2File = "AdditionalMaps.Resources.scattermaps2.png";
    public const string Sm3Key = "SM3";
    private const string Sm3File = "AdditionalMaps.Resources.scattermaps3.png";
    public const string MapKey = "MapIcon";
    private const string MapFile = "AdditionalMaps.Resources.MapIcon.png";
    public const string GhAKey = "GG_Atrium";
    private const string GhAFile = "AdditionalMaps.Resources.Godhome.GH0.png";
    public const string GhArKey = "GG_Atrium_Roof";
    private const string GhArFile = "AdditionalMaps.Resources.Godhome.GH1.png";
    public const string RealGhAKey = "RGH0";
    private const string RealGhAFile = "AdditionalMaps.Resources.Godhome.RGH0.png";
    public const string RealGhArKey = "RGH1";
    private const string RealGhArFile = "AdditionalMaps.Resources.Godhome.RGH1.png";
    #endregion

    private readonly Dictionary<string, Sprite> _dict;

    public TextureStrings()
    {
        var asm = Assembly.GetExecutingAssembly();
        _dict = new Dictionary<string, Sprite>();
        var textureFiles = new Dictionary<string, string>
        {
            {CustomAreaKey, CustomAreaFile},
            {CustomGhAreaKey, CustomGhAreaFile},
            {CustomWpAreaKey, CustomWpAreaFile},
            {Wp01Key, Wp01File},
            {Wp02Key, Wp02File},
            {Wp03Key, Wp03File},
            {Wp04Key, Wp04File},
            {Wp05Key, Wp05File},
            {Wp06Key, Wp06File},
            {Wp07Key, Wp07File},
            {Wp08Key, Wp08File},
            {Wp09Key, Wp09File},
            {Wp12Key, Wp12File},
            {Wp13Key, Wp13File},
            {Wp14Key, Wp14File},
            {Wp15Key, Wp15File},
            {Wp16Key, Wp16File},
            {Wp17Key, Wp17File},
            {Wp18Key, Wp18File},
            {Wp19Key, Wp19File},
            {Wp20Key, Wp20File},
            {WpMapKey, WpMapFile},
            {RealWp01Key, RealWp01File},
            {RealWp02Key, RealWp02File},
            {RealWp03Key, RealWp03File},
            {RealWp04Key, RealWp04File},
            {RealWp05Key, RealWp05File},
            {RealWp06Key, RealWp06File},
            {RealWp07Key, RealWp07File},
            {RealWp08Key, RealWp08File},
            {RealWp09Key, RealWp09File},
            {RealWp12Key, RealWp12File},
            {RealWp13Key, RealWp13File},
            {RealWp14Key, RealWp14File},
            {RealWp15Key, RealWp15File},
            {RealWp16Key, RealWp16File},
            {RealWp17Key, RealWp17File},
            {RealWp18Key, RealWp18File},
            {RealWp19Key, RealWp19File},
            {RealWp20Key, RealWp20File},
            {Sm1Key, Sm1File},
            {Sm2Key, Sm2File},
            {Sm3Key, Sm3File},
            {MapKey, MapFile},
            {GhAKey, GhAFile},
            {GhArKey, GhArFile},
            {RealGhAKey, RealGhAFile},
            {RealGhArKey, RealGhArFile}
        };

        foreach (var pair in textureFiles)
        {
            using var s = asm.GetManifestResourceStream(pair.Value);
            if (s == null) continue;
            var buffer = new byte[s.Length];
            s.Read(buffer, 0, buffer.Length);
            s.Dispose();

            //Create texture from bytes
            var tex = new Texture2D(2, 2);

            tex.LoadImage(buffer, true);

            // Create sprite from texture
            // Split is to cut off the TestOfTeamwork.Resources. and the .png
            float pixelsPerUnit = pair.Value.Contains("scattermaps") ? 64 : 100;
            _dict.Add(pair.Key, Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect));
        }
    }

    public Sprite Get(string key)
    {
        return _dict[key];
    }
}