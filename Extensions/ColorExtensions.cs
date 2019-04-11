using System.Collections.Generic;
using LightJson;
using UnityEngine;

namespace BGC.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Formats to Hex with undercase char and 2-digit place
        /// </summary>
        private const string HexFormat = "X2";

        /// <summary>
        /// Converts Color to HexString in format 0xFFFFFFFF
        /// </summary>
        /// <param name="colorBase"></param>
        /// <returns></returns>
        public static string ColorToHex(this Color colorBase)
        {
            Color32 color = colorBase;

            string hex = string.Format("0x{0}{1}{2}{3}", 
                color.r.ToString(HexFormat), 
                color.g.ToString(HexFormat), 
                color.b.ToString(HexFormat), 
                color.a.ToString(HexFormat));

            return hex;
        }

        /// <summary>
        /// Converts HexString of format 0xFFFFFFFF to Color
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color HexToColor(string hex)
        {
            Color32 color = new Color32();
            hex = hex.Remove(0, 2);
            color.r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            hex = hex.Remove(0, 2);
            color.g = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            hex = hex.Remove(0, 2);
            color.b = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            hex = hex.Remove(0, 2);
            color.a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);

            return (Color)color;
        }

        /// <summary>
        /// Converts a JsonArray of Hex values to List<Color>
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static List<Color> JsonArrayToColorList(this JsonArray arr)
        {
            return arr.JsonArrayToList((JsonValue val) =>
            {
                return HexToColor(val);
            });
        }

        /// <summary>
        /// Converts a List<Color> to a JsonArray of Hex values
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray ColorListToJsonArray(this List<Color> list)
        {
            return list.ConvertToJsonArray((Color color) =>
            {
                return ColorToHex(color);
            });
        }
    }
}