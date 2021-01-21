using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AdditionalMaps.Consts
{
    public class TextureStrings
    {
        #region Texture Keys and Files
        // Custom Area & Rooms
        public const string CustomAreaKey = "CustomArea";
        private const string CustomAreaFile = "AdditionalMaps.Resources.CustomArea.png";
        public const string WP01Key = "White_Palace_01";
        private const string WP01File = "AdditionalMaps.Resources.White_Palace.WP01.png";
        public const string WP02Key = "White_Palace_02";
        private const string WP02File = "AdditionalMaps.Resources.White_Palace.WP02.png";
        public const string WP03Key = "White_Palace_03_hub";
        private const string WP03File = "AdditionalMaps.Resources.White_Palace.WP03.png";
        public const string WP04Key = "White_Palace_04";
        private const string WP04File = "AdditionalMaps.Resources.White_Palace.WP04.png";
        public const string WP05Key = "White_Palace_05";
        private const string WP05File = "AdditionalMaps.Resources.White_Palace.WP05.png";
        public const string WP06Key = "White_Palace_06";
        private const string WP06File = "AdditionalMaps.Resources.White_Palace.WP06.png";
        public const string WP07Key = "White_Palace_07";
        private const string WP07File = "AdditionalMaps.Resources.White_Palace.WP07.png";
        public const string WP08Key = "White_Palace_08";
        private const string WP08File = "AdditionalMaps.Resources.White_Palace.WP08.png";
        public const string WP09Key = "White_Palace_09";
        private const string WP09File = "AdditionalMaps.Resources.White_Palace.WP09.png";
        public const string WP12Key = "White_Palace_12";
        private const string WP12File = "AdditionalMaps.Resources.White_Palace.WP12.png";
        public const string WP13Key = "White_Palace_13";
        private const string WP13File = "AdditionalMaps.Resources.White_Palace.WP13.png";
        public const string WP14Key = "White_Palace_14";
        private const string WP14File = "AdditionalMaps.Resources.White_Palace.WP14.png";
        public const string WP15Key = "White_Palace_15";
        private const string WP15File = "AdditionalMaps.Resources.White_Palace.WP15.png";
        public const string WP16Key = "White_Palace_16";
        private const string WP16File = "AdditionalMaps.Resources.White_Palace.WP16.png";
        public const string WP17Key = "White_Palace_17";
        private const string WP17File = "AdditionalMaps.Resources.White_Palace.WP17.png";
        public const string WP18Key = "White_Palace_18";
        private const string WP18File = "AdditionalMaps.Resources.White_Palace.WP18.png";
        public const string WP19Key = "White_Palace_19";
        private const string WP19File = "AdditionalMaps.Resources.White_Palace.WP19.png";
        public const string WP20Key = "White_Palace_20";
        private const string WP20File = "AdditionalMaps.Resources.White_Palace.WP20.png";
        public const string WPMapKey = "WPMap";
        private const string WPMapFile = "AdditionalMaps.Resources.White_Palace.White_Palace_Transparent.png";
        public const string RealWP01Key = "RWP01";
        private const string RealWP01File = "AdditionalMaps.Resources.White_Palace.RWP01.png";
        public const string RealWP02Key = "RWP02";
        private const string RealWP02File = "AdditionalMaps.Resources.White_Palace.RWP02.png";
        public const string RealWP03Key = "RWP03";
        private const string RealWP03File = "AdditionalMaps.Resources.White_Palace.RWP03.png";
        public const string RealWP04Key = "RWP04";
        private const string RealWP04File = "AdditionalMaps.Resources.White_Palace.RWP04.png";
        public const string RealWP05Key = "RWP05";
        private const string RealWP05File = "AdditionalMaps.Resources.White_Palace.RWP05.png";
        public const string RealWP06Key = "RWP06";
        private const string RealWP06File = "AdditionalMaps.Resources.White_Palace.RWP06.png";
        public const string RealWP07Key = "RWP07";
        private const string RealWP07File = "AdditionalMaps.Resources.White_Palace.RWP07.png";
        public const string RealWP08Key = "RWP08";
        private const string RealWP08File = "AdditionalMaps.Resources.White_Palace.RWP08.png";
        public const string RealWP09Key = "RWP09";
        private const string RealWP09File = "AdditionalMaps.Resources.White_Palace.RWP09.png";
        public const string RealWP12Key = "RWP12";
        private const string RealWP12File = "AdditionalMaps.Resources.White_Palace.RWP12.png";
        public const string RealWP13Key = "RWP13";
        private const string RealWP13File = "AdditionalMaps.Resources.White_Palace.RWP13.png";
        public const string RealWP14Key = "RWP14";
        private const string RealWP14File = "AdditionalMaps.Resources.White_Palace.RWP14.png";
        public const string RealWP15Key = "RWP15";
        private const string RealWP15File = "AdditionalMaps.Resources.White_Palace.RWP15.png";
        public const string RealWP16Key = "RWP16";
        private const string RealWP16File = "AdditionalMaps.Resources.White_Palace.RWP16.png";
        public const string RealWP17Key = "RWP17";
        private const string RealWP17File = "AdditionalMaps.Resources.White_Palace.RWP17.png";
        public const string RealWP18Key = "RWP18";
        private const string RealWP18File = "AdditionalMaps.Resources.White_Palace.RWP18.png";
        public const string RealWP19Key = "RWP19";
        private const string RealWP19File = "AdditionalMaps.Resources.White_Palace.RWP19.png";
        public const string RealWP20Key = "RWP20";
        private const string RealWP20File = "AdditionalMaps.Resources.White_Palace.RWP20.png";
        #endregion

        private Dictionary<string, Sprite> dict;

        public TextureStrings()
        {
            Assembly _asm = Assembly.GetExecutingAssembly();
            dict = new Dictionary<string, Sprite>();
            string[] tmpTextureFiles = {
                CustomAreaFile,
                WP01File,
                WP02File,
                WP03File,
                WP04File,
                WP05File,
                WP06File,
                WP07File,
                WP08File,
                WP09File,
                WP12File,
                WP13File,
                WP14File,
                WP15File,
                WP16File,
                WP17File,
                WP18File,
                WP19File,
                WP20File,
                WPMapFile,
                RealWP01File,
                RealWP02File,
                RealWP03File,
                RealWP04File,
                RealWP05File,
                RealWP06File,
                RealWP07File,
                RealWP08File,
                RealWP09File,
                RealWP12File,
                RealWP13File,
                RealWP14File,
                RealWP15File,
                RealWP16File,
                RealWP17File,
                RealWP18File,
                RealWP19File,
                RealWP20File
            };
            string[] tmpTextureKeys = {
                CustomAreaKey,
                WP01Key,
                WP02Key,
                WP03Key,
                WP04Key,
                WP05Key,
                WP06Key,
                WP07Key,
                WP08Key,
                WP09Key,
                WP12Key,
                WP13Key,
                WP14Key,
                WP15Key,
                WP16Key,
                WP17Key,
                WP18Key,
                WP19Key,
                WP20Key,
                WPMapKey,
                RealWP01Key,
                RealWP02Key,
                RealWP03Key,
                RealWP04Key,
                RealWP05Key,
                RealWP06Key,
                RealWP07Key,
                RealWP08Key,
                RealWP09Key,
                RealWP12Key,
                RealWP13Key,
                RealWP14Key,
                RealWP15Key,
                RealWP16Key,
                RealWP17Key,
                RealWP18Key,
                RealWP19Key,
                RealWP20Key
            };
            for (int i = 0; i < tmpTextureFiles.Length; i++)
            {
                using (Stream s = _asm.GetManifestResourceStream(tmpTextureFiles[i]))
                {
                    if (s != null)
                    {
                        byte[] buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Dispose();

                        //Create texture from bytes
                        var tex = new Texture2D(2, 2);

                        tex.LoadImage(buffer, true);

                        // Create sprite from texture
                        // Split is to cut off the TestOfTeamwork.Resources. and the .png
                        dict.Add(tmpTextureKeys[i], Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                    }
                }
            }
        }

        public Sprite Get(string key)
        {
            return dict[key];
        }
    }
}
