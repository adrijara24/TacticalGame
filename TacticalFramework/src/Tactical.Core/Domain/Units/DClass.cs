using System.Text.Json.Nodes;
using System.Xml.Linq;
using Tactical.Core;
using Tactical.Core.Domain.Items;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

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

    private string name;
    private string description;

    public string Name => name;
    public string Description => description;

    public Stats Stats => baseStats;

    public DWeapon.WeaponType[] WeaponProficiency => weaponProficiency;

    public EMovementType MovementType => movementType;

    public string ID => classID;

    public DClass()
    {
        this.classID = "";
        this.name = "";
        this.description = "";
        this.weaponProficiency = new DWeapon.WeaponType[1];
        weaponProficiency[0] = DWeapon.WeaponType.NONE;
        movementType = EMovementType.GROUND;
    }
    public DClass(string classID, Stats baseStats, DWeapon.WeaponType[] weaponProficiency, EMovementType movementType)
    {
        this.classID = classID;
        this.name = "";
        this.description = "";
        this.baseStats = baseStats;
        this.weaponProficiency = weaponProficiency;
        this.movementType = movementType;
    }

    public void FromJson(JsonObject json)
    {
        classID = json["ID"]!.GetValue<string>();
        name = json["Name"]!.GetValue<string>();
        description = json["Description"]!.GetValue<string>();
        baseStats.FromJson(json["BaseStats"]!.AsObject());
        weaponProficiency = DWeapon.GetWeaponTypes(json["WeaponProficiency"]!.GetValue<int>());
        movementType = (EMovementType)json["MovementType"]!.GetValue<int>();
    }

    public JsonObject ToJson()
    {
        JsonObject json = new JsonObject();
        json.Add("ID", classID);
        json.Add("Name", Name);
        json.Add("Description", Description);
        json.Add("BaseStats", baseStats.ToJson());
        int wpr = 0;
        foreach(DWeapon.WeaponType i in weaponProficiency)
            wpr += (int)i;
        json.Add("WeaponProficiency", wpr);
        json.Add("MovementType", (int)movementType);
        return json;
    }
}