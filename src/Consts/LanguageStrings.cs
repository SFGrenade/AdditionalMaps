using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;

namespace AdditionalMaps.Consts;

public class LanguageStrings
{
    #region Language Strings
    // Places
    public const string PathOfPainKey = "PathOfPainArea";
    public const string WorkshopKey = "WorkshopArea";
    public const string CreditsKey = "CreditsArea";
    public const string WpMapKey = "SFGrenadeAdditionalMaps_WpMapName";

    public const string PantheonKey = "PantheonArea";
    public const string PoHKey = "PohArea";
    public const string GhMapKey = "SFGrenadeAdditionalMaps_GhMapName";
    #endregion

    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _jsonDict;

    public LanguageStrings()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var s = asm.GetManifestResourceStream("AdditionalMaps.Resources.Language.json");
        if (s == null) return;
        var buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);
        s.Dispose();

        var json = System.Text.Encoding.Default.GetString(buffer);

        _jsonDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json);
        Modding.Logger.Log("[AdditionalMaps.Consts.LanguageStrings] - Loaded Language");
    }

    public string Get(string key, string sheet)
    {
        var lang = GameManager.instance.gameSettings.gameLanguage;
        try
        {
            return _jsonDict[lang.ToString()][sheet][key].Replace("<br>", "\n");
        }
        catch
        {
            return _jsonDict[GlobalEnums.SupportedLanguages.EN.ToString()][sheet][key].Replace("<br>", "\n");
        }
    }

    public bool ContainsKey(string key, string sheet)
    {
        try
        {
            var lang = GameManager.instance.gameSettings.gameLanguage;
            try
            {
                return _jsonDict[lang.ToString()][sheet].ContainsKey(key);
            }
            catch
            {
                try
                {
                    return _jsonDict[GlobalEnums.SupportedLanguages.EN.ToString()][sheet].ContainsKey(key);
                }
                catch
                {
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
    }

}