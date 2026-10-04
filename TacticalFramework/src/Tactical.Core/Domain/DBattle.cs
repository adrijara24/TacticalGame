using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace Tactical.Core.Domain
{
    public class DBattle : IAsset
    {

        string battleID;
        string name;
        string description;

        public string mapID;

        public List<KeyValuePair<string, Vector3>> team1;
        public List<KeyValuePair<string, Vector3>> team2;

        public DBattle(string battleID, string name = "", string description = "")
        {
            this.battleID = battleID;
            this.name = name;
            this.description = description;
            this.mapID = "";
            team1 = new List<KeyValuePair<string, Vector3>>();
            team2 = new List<KeyValuePair<string, Vector3>>();
        }

        public string ID => battleID;

        public string Name => name;

        public string Description => description;

        public void FromJson(JsonObject json)
        {
            battleID = json["ID"]!.GetValue<string>();
            name = json["Name"]!.GetValue<string>();
            description = json["Description"]!.GetValue<string>();
            mapID = json["Map"]!.GetValue<string>();

            team1.Clear();
            foreach (JsonNode? node in json["Team1"]!.AsArray())
            {
                JsonObject unit = node!.AsObject();

                string unitID = unit["ID"]!.GetValue<string>();
                JsonObject point = unit["StartPosition"]!.AsObject();

                Vector3 position = new Vector3(
                    point["X"]!.GetValue<float>(),
                    point["Y"]!.GetValue<float>(),
                    point["Z"]!.GetValue<float>()
                );

                team1.Add(new KeyValuePair<string, Vector3>(unitID, position));
            }

            team2.Clear();
            foreach (JsonNode? node in json["Team2"]!.AsArray())
            {
                JsonObject unit = node!.AsObject();

                string unitID = unit["ID"]!.GetValue<string>();
                JsonObject point = unit["StartPosition"]!.AsObject();

                Vector3 position = new Vector3(
                    point["X"]!.GetValue<float>(),
                    point["Y"]!.GetValue<float>(),
                    point["Z"]!.GetValue<float>()
                );

                team2.Add(new KeyValuePair<string, Vector3>(unitID, position));
            }
        }

        public JsonObject ToJson()
        {
            JsonObject json = new JsonObject();

            json.Add("ID", ID);
            json.Add("Name", name);
            json.Add("Description", description);
            json.Add("Map", mapID);
            JsonArray team1Json = new JsonArray();
            foreach(KeyValuePair<string, Vector3> pair in team1)
            {
                JsonObject unit = new JsonObject();
                unit.Add("ID", pair.Key);
                JsonObject pointJson = new JsonObject();
                pointJson.Add("X", pair.Value.X);
                pointJson.Add("Y", pair.Value.Y);
                pointJson.Add("Z", pair.Value.Z);
                unit.Add("StartPosition", pointJson);

                team1Json.Add(unit);
            }
            JsonArray team2Json = new JsonArray();
            foreach (KeyValuePair<string, Vector3> pair in team2)
            {
                JsonObject unit = new JsonObject();
                unit.Add("ID", pair.Key);
                JsonObject pointJson = new JsonObject();
                pointJson.Add("X", pair.Value.X);
                pointJson.Add("Y", pair.Value.Y);
                pointJson.Add("Z", pair.Value.Z);
                unit.Add("StartPosition", pointJson);

                team2Json.Add(unit);
            }

            json.Add("Team1", team1Json);
            json.Add("Team2", team2Json);

            return json;
        }
    }
}
