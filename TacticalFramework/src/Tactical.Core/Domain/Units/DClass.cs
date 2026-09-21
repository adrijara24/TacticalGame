using Tactical.Core.Domain.Units;
using Tactical.Core.Domain;
using System.Text.Json;

public class DClass : IPersistent
{
    Stats baseStats;

    Weapon.WeaponType []weaponProficiency;      // Damage reduced for weapons that are not proficient

    public enum EMovementType
    {
        GROUND = 1, FLYING = 2, WATER = 4
    }

    EMovementType movementType;

    String classID;

    public Stats Stats => baseStats;

    public Weapon.WeaponType[] WeaponProficiency => weaponProficiency;

    public EMovementType MovementType => movementType;

    public String ID => classID;

    public DClass(String classID, Stats baseStats, Weapon.WeaponType[] weaponProficiency, EMovementType movementType)
    {
        this.classID = classID;
        this.baseStats = baseStats;
        this.weaponProficiency = weaponProficiency;
        this.movementType = movementType;
    }

    public void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }

    public JsonElement ToJson()
    {
        throw new NotImplementedException();
    }
}