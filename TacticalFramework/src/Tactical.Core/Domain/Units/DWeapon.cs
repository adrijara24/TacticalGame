using System.Text.Json;

namespace Tactical.Core.Domain.Units;

public class Weapon : IPersistent
{
    public Stats stats;
    public enum WeaponType
    {
        SWORD = 0, SPEAR, AXE, BOW, FISTS, STAFF, SCYTHE, NONE
    }

    public WeaponType type;

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

    public JsonElement ToJson()
    {
        throw new NotImplementedException();
    }

    public void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }
}