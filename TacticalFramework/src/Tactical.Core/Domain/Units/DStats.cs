using System.Text.Json;

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

    public void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }

    public void Log()
    {
        Console.WriteLine("HP: " + hp);
        Console.WriteLine("Mana: " + mana);
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

    public JsonElement ToJson()
    {
        throw new NotImplementedException();
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