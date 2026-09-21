using System.Text.Json;

namespace Tactical.Core.Domain.Units;

public class Weapon : DItem, IPersistent
{
    Stats stats;
    public enum WeaponType
    {
        SWORD = 0, SPEAR, AXE, BOW, FISTS, STAFF, SCYTHE, NONE
    }

    WeaponType type;

    public Weapon(String itemID, Stats stats, WeaponType type) : base(itemID)
    {
        this.stats = stats;
        this.type = type;
    }

    public WeaponType Type => type;

    public Stats Stats => stats;

    public static bool HasAdvantage(Weapon A, Weapon B)
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

    public new JsonElement ToJson()
    {
        throw new NotImplementedException();
    }

    public new void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }
}