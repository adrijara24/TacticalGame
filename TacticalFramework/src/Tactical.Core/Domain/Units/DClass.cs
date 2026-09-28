using Tactical.Core.Domain.Units;
using System.Text.Json.Nodes;
using Tactical.Core.Domain.Items;
using Tactical.Core.Persistence;
using Tactical.Core;

public class DClass : IAsset
{
    Stats baseStats;

    DWeapon.WeaponType []weaponProficiency;      // Damage reduced for weapons that are not proficient

    public enum EMovementType
    {
        GROUND = 1, FLYING = 2, WATER = 4
    }

    EMovementType movementType;

    string classID;

    public Stats Stats => baseStats;

    public DWeapon.WeaponType[] WeaponProficiency => weaponProficiency;

    public EMovementType MovementType => movementType;

    public string ID => classID;

    public DClass(string classID, Stats baseStats, DWeapon.WeaponType[] weaponProficiency, EMovementType movementType)
    {
        this.classID = classID;
        this.baseStats = baseStats;
        this.weaponProficiency = weaponProficiency;
        this.movementType = movementType;
    }

    public void FromJson(JsonObject json)
    {
        throw new NotImplementedException();
    }

    public JsonObject ToJson()
    {
        JsonObject json = new JsonObject();
        json.Add("ID", classID);
        json.Add("BaseStats", baseStats.ToJson());
        int wpr = 0;
        foreach(DWeapon.WeaponType i in weaponProficiency)
            wpr += (int)i;
        json.Add("WeaponProficiency", wpr);
        json.Add("MovementType", (int)movementType);
        return json;
    }
}