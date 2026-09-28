using System.Text.Json.Nodes;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Items;

public class DWeapon : DItem
{
    Stats stats;
    public enum WeaponType
    {
        SWORD = 1, SPEAR = 2, AXE = 4, BOW = 8, FISTS = 16, STAFF = 32, SCYTHE = 64, NONE = 0
    }

    WeaponType type;

    public DWeapon(string itemID, Stats stats, WeaponType type) : base(itemID)
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
        json.Add("Stats", stats.ToJson());
        json.Add("WeaponType", (int)type);
        return json;
    }

    public override void FromJson(JsonObject json)
    {
        throw new NotImplementedException();
    }
}