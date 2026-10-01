using System.Text.Json.Nodes;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Units;

public struct Stats : IPersistent
{
    public int hp;
    public int mana;
    public int strength;
    public int magic;
    public int defense;
    public int magicDefense;
    public int precision;
    public int evasion;
    public int movement;
    public int range;
    public int speed;
    public int criticalRate;

    public void FromJson(JsonObject json)
    {
        hp =            json["MHP"]!.GetValue<int>();
        mana =          json["MMP"]!.GetValue<int>();
        strength =      json["STR"]!.GetValue<int>();
        magic =         json["MAG"]!.GetValue<int>();
        defense =       json["DEF"]!.GetValue<int>();
        magicDefense =  json["RES"]!.GetValue<int>();
        precision =     json["PRE"]!.GetValue<int>();
        evasion =       json["EVA"]!.GetValue<int>();
        movement =      json["MOV"]!.GetValue<int>();
        range =         json["RNG"]!.GetValue<int>();
        speed =         json["SPD"]!.GetValue<int>();
        criticalRate =  json["CRI"]!.GetValue<int>();
    }

    public void Log()
    {
        Console.WriteLine("Max HP: " + hp);
        Console.WriteLine("Max Mana: " + mana);
        Console.WriteLine("Strength: " + strength);
        Console.WriteLine("Magic: " + magic);
        Console.WriteLine("Defense: " + defense);
        Console.WriteLine("Magic Defense: " + magicDefense);
        Console.WriteLine("Precision: " + precision);
        Console.WriteLine("Evasion: " + evasion);
        Console.WriteLine("Movement: " + movement);
        Console.WriteLine("Range: " + range);
        Console.WriteLine("Speed: " + speed);
        Console.WriteLine("Critical Rate: " + criticalRate);
    }

    public JsonObject ToJson()
    {
        JsonObject json = new JsonObject
        {
            { "MHP", hp },
            { "MMP", mana },
            { "STR", strength },
            { "MAG", magic },
            { "DEF", defense },
            { "RES", magicDefense },
            { "PRE", precision },
            { "EVA", evasion },
            { "MOV", movement },
            { "RNG", range },
            { "SPD", speed },
            { "CRI", criticalRate }
        };

        return json;
    }

    public static Stats operator +(Stats A, Stats B)
    {
        Stats result = new Stats();
        result.hp = A.hp + B.hp;
        result.mana = A.mana + B.mana;
        result.strength = A.strength + B.strength;
        result.magic = A.magic + B.magic;
        result.defense = A.defense + B.defense;
        result.magicDefense = A.magicDefense + B.magicDefense;
        result.precision = A.precision + B.precision;
        result.evasion = A.evasion + B.evasion;
        result.movement = A.movement + B.movement;
        result.range = A.range + B.range;
        result.speed = A.speed + B.speed;
        result.criticalRate = A.criticalRate + B.criticalRate;
        return result;
    }
}