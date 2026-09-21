using Tactical.Core.Domain.Units;

Stats test1 = new Stats();
test1.hp = 10;
test1.mana = 1;
test1.strength = 5;

Stats test2 = new Stats();
test2.hp = 2;
test2.mana = 0;
test2.strength = 20;

Stats horseStats = new Stats();
horseStats.hp = 5;
horseStats.movement = 4;

DClass horseClass = new DClass("Horse", horseStats, [Weapon.WeaponType.SWORD, Weapon.WeaponType.SPEAR], DClass.EMovementType.GROUND);

Weapon A = new Weapon("SwordTest", test2, Weapon.WeaponType.SWORD);
Weapon B = new Weapon("SpearTest", test2, Weapon.WeaponType.SPEAR);
Weapon C = new Weapon("AxeTest", test2, Weapon.WeaponType.AXE);
Weapon D = new Weapon("ScytheTest", test2, Weapon.WeaponType.SCYTHE);

Unit myObj = new Unit("HorseRider#1", horseClass, A);
myObj.Stats.Log();

Console.WriteLine(Weapon.HasAdvantage(A, B));
Console.WriteLine(Weapon.HasAdvantage(A, C));
Console.WriteLine(Weapon.HasAdvantage(A, D));
Console.WriteLine();

Console.WriteLine(Weapon.HasAdvantage(B, A));
Console.WriteLine(Weapon.HasAdvantage(B, B));
Console.WriteLine(Weapon.HasAdvantage(B, C));
Console.WriteLine(Weapon.HasAdvantage(B, D));
Console.WriteLine();

Console.WriteLine(Weapon.HasAdvantage(C, A));
Console.WriteLine(Weapon.HasAdvantage(C, B));
Console.WriteLine(Weapon.HasAdvantage(C, D));
Console.WriteLine();

Console.WriteLine(Weapon.HasAdvantage(D, A));
Console.WriteLine(Weapon.HasAdvantage(D, B));
Console.WriteLine(Weapon.HasAdvantage(D, C));
Console.WriteLine();

