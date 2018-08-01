using LightJson;
using BGC.UI;
using System.Collections.Generic;
using UnityEngine;
using BGC.Extensions;

namespace BGC.Utility
{
    public class ColorPalette
    {
        private class Keys
        {
            public const string PrimaryColors = "PrimaryColors";
            public const string SecondaryColors = "SecondaryColors";
        }
        public readonly string FileName;

        public List<Color> PrimaryColors;
        public List<Color> SecondaryColors;

        public ColorPalette(string FileName = "")
        {
            this.FileName = FileName;
        }

        public ColorPalette(JsonObject json, string FileName)
        {
            this.FileName = FileName;
            Deserialize(json);
        }

        public void AddPrimaryColors(params Color[] colors)
        {
            for(int i = 0; i < colors.Length; ++i)
            {
                PrimaryColors.Add(colors[i]);
            }
        }

        public void AddSecondaryColors(params Color[] colors)
        {
            for (int i = 0; i < colors.Length; ++i)
            {
                SecondaryColors.Add(colors[i]);
            }
        }

        public void SubPrimaryColors(params Color[] colors)
        {
            for (int i = 0; i < colors.Length; ++i)
            {
                PrimaryColors.Remove(colors[i]);
            }
        }

        public void SubSecondaryColors(params Color[] colors)
        {
            for (int i = 0; i < colors.Length; ++i)
            {
                SecondaryColors.Remove(colors[i]);
            }
        }

        public JsonObject Serialize()
        {
            JsonObject json = new JsonObject();

            json.Add(Keys.PrimaryColors, PrimaryColors.ColorListToJsonArray());
            json.Add(Keys.SecondaryColors, SecondaryColors.ColorListToJsonArray());

            return json;
        }

        public void Deserialize(JsonObject json)
        {
            PrimaryColors = json.TryGetArray(Keys.PrimaryColors).JsonArrayToColorList();
            SecondaryColors = json.TryGetArray(Keys.SecondaryColors).JsonArrayToColorList();
        }
    }
}
