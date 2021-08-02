using System;
using System.Collections.Generic;
using LightJson;

namespace BGC.Audio.Audiometry
{
    public class TransducerProfile
    {
        public string Name { get; set; }

        public List<RETSPLEntry> RETSPLs { get; set; }

        public int Version { get; set; }
        public const int CURRENT_VERSION = 1;

        //Caches where this RETSPL is serialized
        public string filePath = "";

        public TransducerProfile()
        {
            Name = "";
            Version = CURRENT_VERSION;
            RETSPLs = new List<RETSPLEntry>();
        }

        public TransducerProfile(string name, IEnumerable<RETSPLEntry> values)
        {
            Name = name;
            Version = CURRENT_VERSION;
            RETSPLs = new List<RETSPLEntry>(values);
        }

        public TransducerProfile(JsonObject data)
        {
            Name = data["Name"];
            Version = data["Version"];
            RETSPLs = new List<RETSPLEntry>();

            foreach (JsonObject value in data["RETSPLs"].AsJsonArray)
            {
                RETSPLs.Add(new RETSPLEntry(value));
            }
        }

        public JsonObject Serialize()
        {
            JsonArray values = new JsonArray();
            foreach (RETSPLEntry value in RETSPLs)
            {
                values.Add(value.Serialize());
            }

            return new JsonObject()
            {
                ["Name"] = Name,
                ["Version"] = CURRENT_VERSION,
                ["RETSPLs"] = values
            };
        }

        public double GetRETSPL(double frequency)
        {
            if (frequency <= RETSPLs[0].Frequency)
            {
                return RETSPLs[0].RETSPL;
            }

            if (frequency >= RETSPLs[RETSPLs.Count - 1].Frequency)
            {
                return RETSPLs[RETSPLs.Count - 1].RETSPL;
            }

            //Frequency is between the upper and lower bounds of the RETSPLs

            int upperBound;

            for (upperBound = 1; upperBound < RETSPLs.Count - 1; upperBound++)
            {
                if (frequency < RETSPLs[upperBound].Frequency)
                {
                    //Found the first RETSPL frequency larger than the target
                    break;
                }
            }

            //Could be equal to the lowerbound
            if (frequency == RETSPLs[upperBound - 1].Frequency)
            {
                return RETSPLs[upperBound - 1].RETSPL;
            }

            //Interpolate exponentially between two adjacent RETSPLs
            //  L * ( U / L ) ^ t = f
            //  t = log ( f / L ) / log ( U / L )
            return Mathematics.GeneralMath.Lerp(
                initial: RETSPLs[upperBound - 1].RETSPL,
                final: RETSPLs[upperBound].RETSPL,
                t: Math.Log(frequency / RETSPLs[upperBound - 1].RETSPL) / Math.Log(RETSPLs[upperBound].RETSPL / RETSPLs[upperBound - 1].RETSPL));
        }

        public double GetSPL(double frequency, double levelHL) => levelHL + GetRETSPL(frequency);
        public double GetHL(double frequency, double levelSPL) => levelSPL - GetRETSPL(frequency);
    }

    public class RETSPLEntry
    {
        public double Frequency { get; set; }
        public double RETSPL { get; set; }


        public RETSPLEntry()
        {
            Frequency = 0;
            RETSPL = 0;
        }

        public RETSPLEntry(double frequency, double retspl)
        {
            Frequency = frequency;
            RETSPL = retspl;
        }

        public RETSPLEntry(JsonObject value)
        {
            Frequency = value["Frequency"];
            RETSPL = value["RETSPL"];
        }

        public JsonObject Serialize() => new JsonObject()
        {
            ["Frequency"] = Frequency,
            ["RETSPL"] = RETSPL
        };
    }
}
