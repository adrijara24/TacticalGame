using Tactical.Core;
using Tactical.Core.Domain.Items;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;
using Tactical.Core.Domain;
using System.Numerics;

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

DClass horseClass = new DClass("Horse", horseStats, [DWeapon.WeaponType.SWORD, DWeapon.WeaponType.SPEAR], DClass.EMovementType.GROUND);

DWeapon A = new DWeapon("SwordTest", test2, DWeapon.WeaponType.SWORD);
DWeapon B = new DWeapon("SpearTest", test2, DWeapon.WeaponType.SPEAR);
DWeapon C = new DWeapon("AxeTest", test2, DWeapon.WeaponType.AXE);
DWeapon D = new DWeapon("ScytheTest", test2, DWeapon.WeaponType.SCYTHE);

DUnit myObj = new DUnit("HorseRider#1", test1, "Horse");
myObj.Inventory[1] = "POTION#1";
myObj.Stats.Log();

Console.WriteLine(DWeapon.HasAdvantage(A, B));
Console.WriteLine(DWeapon.HasAdvantage(A, C));
Console.WriteLine(DWeapon.HasAdvantage(A, D));
Console.WriteLine();

Console.WriteLine(DWeapon.HasAdvantage(B, A));
Console.WriteLine(DWeapon.HasAdvantage(B, B));
Console.WriteLine(DWeapon.HasAdvantage(B, C));
Console.WriteLine(DWeapon.HasAdvantage(B, D));
Console.WriteLine();

Console.WriteLine(DWeapon.HasAdvantage(C, A));
Console.WriteLine(DWeapon.HasAdvantage(C, B));
Console.WriteLine(DWeapon.HasAdvantage(C, D));
Console.WriteLine();

Console.WriteLine(DWeapon.HasAdvantage(D, A));
Console.WriteLine(DWeapon.HasAdvantage(D, B));
Console.WriteLine(DWeapon.HasAdvantage(D, C));
Console.WriteLine();

Console.WriteLine("Inventory: ");
for (int i = 0; i < 5; ++i)
    Console.WriteLine(myObj.Inventory[i] is not null ? myObj.Inventory[i] : "NO ITEM");
Console.WriteLine();

CampaignAssets assets = new CampaignAssets();
assets.Add<DClass>(horseClass);
//assets.Add<DItem>(new DItem("TestItem2"));
assets.Add<DUnit>(myObj);
//assets.Add<DItem>(new DItem("TestItem"));
assets.Add<DItem>(new DWeapon("TestWeapon", new Stats(), DWeapon.WeaponType.SWORD));
// assets.Add<DItem>(new DWeapon("TestWeapon", new Stats(), DWeapon.WeaponType.SPEAR)); Throws exception (as it should)

foreach (IAsset asset in assets.GetAllAssets())
    Console.WriteLine($"[{asset.GetType().Name}]: {asset.ID}");

DEffectAction action1 = new DEffectAction();
action1.damageDefinition = new DamageDefinition("10", EDamageType.HEALING);
action1.trigger = EEffectTrigger.ONAPPLY;

DEffectAction action2 = new DEffectAction();
action2.damageDefinition = new DamageDefinition("2", EDamageType.HEALING);
action2.trigger = EEffectTrigger.ONTURNSTART;
DEffectAction action3 = new DEffectAction();
action3.stats.movement = 1;
action3.trigger = EEffectTrigger.ONAPPLY;

DEffectAction action4 = new DEffectAction();
action4.damageDefinition = new DamageDefinition("3", EDamageType.PURE);
action4.trigger = EEffectTrigger.ONTURNEND;

DEffect effect = new DEffect("SmallHeal", EEffectDuration.TURNS, 3, EEffectStacking.STACK, 2, [action1, action2, action3]);

DEffect burnEffect = new DEffect("Burn", EEffectDuration.TURNS, 2, EEffectStacking.REFRESH, 1, [action4]);

DAbilityAction cleaveAction = new DAbilityAction(EAbilityTrigger.ONHIT);
cleaveAction.damageDefinition = new DamageDefinition("5 + user.STR", EDamageType.PHYSICAL);
cleaveAction.addEffects.Add("Bleed");
DAbilityAction buffAction = new DAbilityAction(EAbilityTrigger.ONACTIVE);
buffAction.stats.strength = 10;

DAbility ability1 = new DAbility();
ability1.abilityID = "Cleave";
ability1.minRange = 1;
ability1.maxRange = 1;
ability1.target = EAbilityTarget.ENEMY;
ability1.type = EAbilityType.PASSIVE;
ability1.area = [new Vector3(-1, 0, 1), new Vector3(0, 0, 1), new Vector3(1, 0, 1)];
ability1.actions.Add(cleaveAction);
ability1.actions.Add(buffAction);

DBattle battle = new DBattle("TestBattle");
battle.mapID = "DefaultMap";
battle.team1.Add(new KeyValuePair<string, Vector3>("HorseRider#1", new Vector3(1, 0, 1)));
battle.team2.Add(new KeyValuePair<string, Vector3>("HorseRider#1", new Vector3(3, 0, 3)));

CampaignAssets assets2 = new CampaignAssets();
assets2.Add<DClass>(horseClass);
assets2.Add<DUnit>(myObj);
assets2.Add<DWeapon>(A);
assets2.Add<DWeapon>(B);
assets2.Add<DWeapon>(C);
assets2.Add<DWeapon>(D);
assets2.Add<DItemConsumable>(new DItemConsumable("Potion#1", "Potion", "A potion", 3, "SmallHeal"));
assets2.Add<DEffect>(effect);
assets2.Add<DEffect>(burnEffect);
assets2.Add<DAbility>(ability1);
assets2.Add<DBattle>(battle);
CampaignSerializer.SaveCampaign("TestCampaign", "", assets2);

CampaignAssets assets3 = CampaignSerializer.LoadCampaign("TestCampaign", "");
Console.WriteLine("Printing loaded campaign from disk");
foreach (IAsset asset in assets3.GetAllAssets())
    Console.WriteLine($"[{asset.GetType().Name}]: {asset.ID}");
