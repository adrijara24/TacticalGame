using Tactical.Core.Domain.Units;

var myObj = new Unit();


Weapon A = new Weapon();
Weapon B = new Weapon();
Weapon C = new Weapon();
Weapon D = new Weapon();

Stats test1 = new Stats();
test1.hp = 10;
test1.mana = 1;
test1.strength = 5;

Stats test2 = new Stats();
test2.hp = 2;
test2.mana = 0;
test2.strength = 20;

myObj.baseStats = test1;
myObj.baseStats.Log();
A.stats = test2;
myObj.weapon = A;
Console.WriteLine();

myObj.GetAccumulatedStats().Log();
Console.WriteLine();

A.type = Weapon.WeaponType.SWORD;
B.type = Weapon.WeaponType.SPEAR;
C.type = Weapon.WeaponType.AXE;
D.type = Weapon.WeaponType.SCYTHE;

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

