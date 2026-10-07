using System.Text.Json.Nodes;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Items;

public class DWeapon : DItem
{
    Stats stats;
    public enum WeaponType
    {
        SWORD = 1, SPEAR = 2, AXE = 4, BOW = 8, FISTS = 16, STAFF = 32, SCYTHE = 64, DAGGER = 128, NONE = 0
    }

    public static WeaponType[] GetWeaponTypes(int data)
    {
        int val = data;
        List<WeaponType> weapons = new List<WeaponType>();
        for (int i = 0; i < 8 && val != 0; ++i)
        {
            if ((val & 0x01) != 0)
                weapons.Add((WeaponType)(1 << i));
            val = (val >> 1);
        }
        return weapons.ToArray();
    }

    WeaponType type;

    public DWeapon() : base("")
    {

    }

    public DWeapon(string itemID, Stats stats, WeaponType type) : base(itemID)
    {
        this.stats = stats;
        this.type = type;
    }

    public DWeapon(string itemID, string name, string description, Stats stats, WeaponType type) : base(itemID, name, description)
    {
        this.stats = stats;
        this.type = type;
    }

    public WeaponType Type => type;

    public Stats Stats => stats;

    public static bool HasAdvantage(DWeapon A, DWeapon B)
    {
        switch (A.type)
        {
            case WeaponType.SWORD:
                return B.type == WeaponType.AXE;
            case WeaponType.SPEAR:
                return B.type == WeaponType.SWORD;
            case WeaponType.AXE:
                return B.type == WeaponType.SPEAR;
            default:
                return false;
        }
    }

    public override JsonObject ToJson()
    {
        JsonObject json = new JsonObject();
        json.Add("ID", ID);
        json.Add("Name", Name);
        json.Add("Description", Description);
        json.Add("Type", "DWeapon");
        json.Add("Stats", stats.ToJson());
        json.Add("WeaponType", (int)type);
        return json;
    }

    public override void FromJson(JsonObject json)
    {
        itemID = json["ID"]!.GetValue<string>();
        name = json["Name"]!.GetValue<string>();
        description = json["Description"]!.GetValue<string>();
        stats.FromJson(json["Stats"]!.AsObject());
        type = (WeaponType)json["WeaponType"]!.GetValue<int>();
    }
}