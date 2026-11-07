using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json.Nodes;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Battle;
using Tactical.Core.Domain.Items;
using Tactical.Core.Domain.Terrain;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;
using Tactical.Core.Systems;

namespace Tactical.Test;

public static class ActiveAbilityTests
{
    private sealed class Fixture
    {
        public BattleProcessor Processor { get; }
        public DAbility Ability { get; }

        public Fixture(BattleProcessor processor, DAbility ability)
        {
            Processor = processor;
            Ability = ability;
        }
    }

    public static void RunTests()
    {
        int passed = 0;
        int failed = 0;

        Console.WriteLine("Running Active Ability Tests...");
        Console.WriteLine("------------------------------------------");

        // Target validation
        RunTest(nameof(TestSelfTarget), TestSelfTarget, ref passed, ref failed);
        RunTest(nameof(TestSelfTargetRejectsEnemy), TestSelfTargetRejectsEnemy, ref passed, ref failed);
        RunTest(nameof(TestAllyTarget), TestAllyTarget, ref passed, ref failed);
        RunTest(nameof(TestAllyTargetRejectsSelf), TestAllyTargetRejectsSelf, ref passed, ref failed);
        RunTest(nameof(TestEnemyTarget), TestEnemyTarget, ref passed, ref failed);
        RunTest(nameof(TestEnemyTargetRejectsAlly), TestEnemyTargetRejectsAlly, ref passed, ref failed);
        RunTest(nameof(TestAnyTargetAcceptsAlly), TestAnyTargetAcceptsAlly, ref passed, ref failed);
        RunTest(nameof(TestMissingTarget), TestMissingTarget, ref passed, ref failed);
        RunTest(nameof(TestDeadTarget), TestDeadTarget, ref passed, ref failed);

        // Range validation
        RunTest(nameof(TestMaxRangeInclusive), TestMaxRangeInclusive, ref passed, ref failed);
        RunTest(nameof(TestOutOfRange), TestOutOfRange, ref passed, ref failed);
        RunTest(nameof(TestBelowMinimumRange), TestBelowMinimumRange, ref passed, ref failed);

        // Ability validation
        RunTest(nameof(TestUnownedAbility), TestUnownedAbility, ref passed, ref failed);
        RunTest(nameof(TestPassiveAbilityCannotBeUsed), TestPassiveAbilityCannotBeUsed, ref passed, ref failed);
        RunTest(nameof(TestInsufficientMana), TestInsufficientMana, ref passed, ref failed);
        RunTest(nameof(TestManaConsumedOnce), TestManaConsumedOnce, ref passed, ref failed);
        RunTest(nameof(TestZeroManaCost), TestZeroManaCost, ref passed, ref failed);
        RunTest(nameof(TestNoOnUseRuntimeGuard), TestNoOnUseRuntimeGuard, ref passed, ref failed);

        // Campaign validation
        RunTest(nameof(TestNoOnUseRejectedOnLoad), TestNoOnUseRejectedOnLoad, ref passed, ref failed);
        RunTest(nameof(TestInvalidTargetRejectedOnLoad), TestInvalidTargetRejectedOnLoad, ref passed, ref failed);
        RunTest(nameof(TestInvalidRangeRejectedOnLoad), TestInvalidRangeRejectedOnLoad, ref passed, ref failed);
        RunTest(nameof(TestInvalidAreaRejectedOnLoad), TestInvalidAreaRejectedOnLoad, ref passed, ref failed);
        RunTest(nameof(TestInvalidDamageTypeRejectedOnLoad), TestInvalidDamageTypeRejectedOnLoad, ref passed, ref failed);

        // Area resolution
        RunTest(nameof(TestAreaHitsPrimaryAndOffsetTargets), TestAreaHitsPrimaryAndOffsetTargets, ref passed, ref failed);
        RunTest(nameof(TestAreaCanHitAllies), TestAreaCanHitAllies, ref passed, ref failed);
        RunTest(nameof(TestDuplicateAreaOffsets), TestDuplicateAreaOffsets, ref passed, ref failed);
        RunTest(nameof(TestDeadAreaUnitsAreIgnored), TestDeadAreaUnitsAreIgnored, ref passed, ref failed);
        RunTest(nameof(TestAreaCanReachBeyondPrimaryRange), TestAreaCanReachBeyondPrimaryRange, ref passed, ref failed);

        // Action execution
        RunTest(nameof(TestActionsExecuteInOrder), TestActionsExecuteInOrder, ref passed, ref failed);
        RunTest(nameof(TestDamageThenHealing), TestDamageThenHealing, ref passed, ref failed);
        RunTest(nameof(TestEffectIsAdded), TestEffectIsAdded, ref passed, ref failed);
        RunTest(nameof(TestEffectIsRemoved), TestEffectIsRemoved, ref passed, ref failed);
        RunTest(nameof(TestEffectChangesLaterFormula), TestEffectChangesLaterFormula, ref passed, ref failed);

        // Formula calculation errors
        RunTest(nameof(TestFormulaErrorReturnsZeroAndContinues), TestFormulaErrorReturnsZeroAndContinues, ref passed, ref failed);
        RunTest(nameof(TestEffectFormulaErrorReturnsZero), TestEffectFormulaErrorReturnsZero, ref passed, ref failed);

        Console.WriteLine("------------------------------------------");
        Console.WriteLine("Results: " + passed + "/" + (passed + failed) + " passed.");
        Console.WriteLine("Failed: " + failed);
    }

    private static void RunTest(string name, Action test, ref int passed, ref int failed)
    {
        try
        {
            test();
            passed++;
            Console.WriteLine("[PASS] " + name);
        }
        catch (Exception exception)
        {
            failed++;
            Console.WriteLine("[FAIL] " + name + ": " + exception.Message);
        }
    }

    // ========================================================================
    // FIXTURE CREATION
    // ========================================================================

    private static Stats CreateStats(int hp = 100, int mana = 30, int strength = 10, int defense = 2)
    {
        return new Stats
        {
            hp = hp,
            mana = mana,
            strength = strength,
            magic = 8,
            defense = defense,
            magicDefense = 2,
            precision = 75,
            evasion = 10,
            movement = 5,
            range = 3,
            speed = 7,
            criticalRate = 15
        };
    }

    private static DUnit CreateUnit(string id, Stats stats, string? abilityID = null)
    {
        DUnit unit = new DUnit(id, stats, "TestClass");

        if (!string.IsNullOrWhiteSpace(abilityID))
        {
            JsonObject json = unit.ToJson();
            json["Abilities"]!.AsObject()["Ability1"] = abilityID;
            unit.FromJson(json);
        }

        return unit;
    }

    private static DAbility CreateAbility(EAbilityTarget target = EAbilityTarget.ENEMY, string formula = "10", EDamageType damageType = EDamageType.PURE, int manaCost = 5)
    {
        DAbility ability = new DAbility();

        ability.abilityID = "TestAbility";
        ability.name = "Test Ability";
        ability.description = "";
        ability.type = EAbilityType.ACTIVE;
        ability.manaCost = manaCost;
        ability.minRange = 0;
        ability.maxRange = 5;
        ability.target = target;

        ability.actions.Add(new DAbilityAction(EAbilityTrigger.ONUSE)
        {
            damageDefinition = new DamageDefinition(formula, damageType)
        });

        return ability;
    }

    private static DEffect CreateDefenseEffect(string id, int bonus)
    {
        DEffectAction action = new DEffectAction(EEffectTrigger.ONAPPLY);
        action.stats = new Stats { defense = bonus };

        return new DEffect(id, EEffectDuration.PERMANENT, 0, EEffectStacking.IGNORE, 1, new[] { action });
    }

    private static Fixture CreateFixture(DAbility? ability = null, int userMana = 30, int enemyHP = 100, int enemy2HP = 100, bool assignAbility = true, DEffect[]? extraEffects = null)
    {
        ability ??= CreateAbility();

        CampaignAssets assets = new CampaignAssets();

        int allMovement = (int)(DClass.EMovementType.GROUND | DClass.EMovementType.FLYING | DClass.EMovementType.WATER);

        DMap map = new DMap("TestMap", 8, 8, "Floor");
        DTile floor = new DTile("Floor", "Floor", "", "NONE", default, 1, allMovement);
        DClass unitClass = new DClass("TestClass", default, new[] { DWeapon.WeaponType.NONE }, DClass.EMovementType.GROUND);

        DUnit caster = CreateUnit("Caster", CreateStats(mana: userMana), assignAbility ? ability.ID : null);
        DUnit ally = CreateUnit("Ally", CreateStats(strength: 0));
        DUnit enemy = CreateUnit("Enemy", CreateStats(hp: enemyHP, strength: 5));
        DUnit enemy2 = CreateUnit("Enemy2", CreateStats(hp: enemy2HP, strength: 5));

        assets.Add(map);
        assets.Add(floor);
        assets.Add(unitClass);
        assets.Add(caster);
        assets.Add(ally);
        assets.Add(enemy);
        assets.Add(enemy2);
        assets.Add(ability);

        if (extraEffects != null)
        {
            foreach (DEffect effect in extraEffects)
                assets.Add(effect);
        }

        DBattle battle = new DBattle("TestBattle");
        battle.mapID = map.ID;

        battle.team1.Add(new KeyValuePair<string, Vector3>("Caster", new Vector3(0, 0, 0)));
        battle.team1.Add(new KeyValuePair<string, Vector3>("Ally", new Vector3(1, 0, 0)));

        battle.team2.Add(new KeyValuePair<string, Vector3>("Enemy", new Vector3(3, 0, 0)));
        battle.team2.Add(new KeyValuePair<string, Vector3>("Enemy2", new Vector3(4, 0, 0)));

        AssetManager.Instance.LoadBattle(battle, assets);

        return new Fixture(new BattleProcessor(battle), ability);
    }

    // ========================================================================
    // ASSERTION HELPERS
    // ========================================================================

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void AssertEqual(int expected, int actual, string name)
    {
        if (expected != actual)
            throw new InvalidOperationException(name + ": expected " + expected + ", got " + actual + ".");
    }

    private static void AssertAccepted(BattleCommandResult result)
    {
        Assert(result.Status, "Expected command to succeed, but got: " + result.Error);
    }

    private static void AssertRejected(BattleCommandResult result)
    {
        Assert(!result.Status, "Expected command to fail, but it succeeded.");
    }

    private static void AssertInvalidBattle(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidDataException)
        {
            return;
        }

        throw new InvalidOperationException("Expected battle loading to reject the invalid ability.");
    }

    private static BattleCommandResult Use(Fixture fixture, string target = "Enemy")
    {
        return fixture.Processor.Execute("ABILITY Caster TestAbility " + target);
    }

    private static JsonObject ReadUnit(Fixture fixture, string unitID)
    {
        BattleCommandResult result = fixture.Processor.Execute("UNIT " + unitID);
        AssertAccepted(result);
        return JsonNode.Parse(result.Output)!.AsObject();
    }

    private static int GetHP(Fixture fixture, string unitID)
    {
        return ReadUnit(fixture, unitID)["CurrentHP"]!.GetValue<int>();
    }

    private static int GetMP(Fixture fixture, string unitID)
    {
        return ReadUnit(fixture, unitID)["CurrentMP"]!.GetValue<int>();
    }

    private static int GetStat(Fixture fixture, string unitID, string stat)
    {
        return ReadUnit(fixture, unitID)["Stats"]!.AsObject()[stat]!.GetValue<int>();
    }

    // ========================================================================
    // TARGET VALIDATION
    // ========================================================================

    private static void TestSelfTarget()
    {
        Fixture fixture = CreateFixture(CreateAbility(EAbilityTarget.SELF));

        AssertAccepted(Use(fixture, "Caster"));
        AssertEqual(90, GetHP(fixture, "Caster"), "Caster HP");
    }

    private static void TestSelfTargetRejectsEnemy()
    {
        Fixture fixture = CreateFixture(CreateAbility(EAbilityTarget.SELF));

        AssertRejected(Use(fixture, "Enemy"));
        AssertEqual(100, GetHP(fixture, "Caster"), "Caster HP");
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestAllyTarget()
    {
        Fixture fixture = CreateFixture(CreateAbility(EAbilityTarget.ALLY));

        AssertAccepted(Use(fixture, "Ally"));
        AssertEqual(90, GetHP(fixture, "Ally"), "Ally HP");
    }

    private static void TestAllyTargetRejectsSelf()
    {
        Fixture fixture = CreateFixture(CreateAbility(EAbilityTarget.ALLY));

        AssertRejected(Use(fixture, "Caster"));
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestEnemyTarget()
    {
        Fixture fixture = CreateFixture(CreateAbility(EAbilityTarget.ENEMY));

        AssertAccepted(Use(fixture, "Enemy"));
        AssertEqual(90, GetHP(fixture, "Enemy"), "Enemy HP");
    }

    private static void TestEnemyTargetRejectsAlly()
    {
        Fixture fixture = CreateFixture(CreateAbility(EAbilityTarget.ENEMY));

        AssertRejected(Use(fixture, "Ally"));
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestAnyTargetAcceptsAlly()
    {
        Fixture fixture = CreateFixture(CreateAbility(EAbilityTarget.ANY));

        AssertAccepted(Use(fixture, "Ally"));
        AssertEqual(90, GetHP(fixture, "Ally"), "Ally HP");
    }

    private static void TestMissingTarget()
    {
        Fixture fixture = CreateFixture();

        AssertRejected(Use(fixture, "Missing"));
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestDeadTarget()
    {
        Fixture fixture = CreateFixture(enemyHP: 0);

        AssertRejected(Use(fixture));
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    // ========================================================================
    // RANGE VALIDATION
    // ========================================================================

    private static void TestMaxRangeInclusive()
    {
        DAbility ability = CreateAbility();
        ability.maxRange = 3;

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));
        AssertEqual(90, GetHP(fixture, "Enemy"), "Enemy HP");
    }

    private static void TestOutOfRange()
    {
        DAbility ability = CreateAbility();
        ability.maxRange = 2;

        Fixture fixture = CreateFixture(ability);

        AssertRejected(Use(fixture));
        AssertEqual(100, GetHP(fixture, "Enemy"), "Enemy HP");
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestBelowMinimumRange()
    {
        DAbility ability = CreateAbility(EAbilityTarget.ALLY);
        ability.minRange = 2;

        Fixture fixture = CreateFixture(ability);

        AssertRejected(Use(fixture, "Ally"));
        AssertEqual(100, GetHP(fixture, "Ally"), "Ally HP");
    }

    // ========================================================================
    // OWNERSHIP, TYPE, AND MANA
    // ========================================================================

    private static void TestUnownedAbility()
    {
        Fixture fixture = CreateFixture(assignAbility: false);

        AssertRejected(Use(fixture));
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestPassiveAbilityCannotBeUsed()
    {
        DAbility ability = CreateAbility();
        ability.type = EAbilityType.PASSIVE;
        ability.actions[0].trigger = EAbilityTrigger.ONATTACK;

        Fixture fixture = CreateFixture(ability);

        AssertRejected(Use(fixture));
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestInsufficientMana()
    {
        Fixture fixture = CreateFixture(userMana: 2);

        AssertRejected(Use(fixture));
        AssertEqual(2, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestManaConsumedOnce()
    {
        DAbility ability = CreateAbility(manaCost: 7);
        ability.area.Add(new Vector3(1, 0, 0));

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));

        AssertEqual(23, GetMP(fixture, "Caster"), "Caster MP");
        AssertEqual(90, GetHP(fixture, "Enemy"), "Primary target HP");
        AssertEqual(90, GetHP(fixture, "Enemy2"), "Area target HP");
    }

    private static void TestZeroManaCost()
    {
        DAbility ability = CreateAbility(manaCost: 0);
        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestNoOnUseRuntimeGuard()
    {
        Fixture fixture = CreateFixture();
        fixture.Ability.actions.Clear();

        AssertRejected(Use(fixture));
        AssertEqual(30, GetMP(fixture, "Caster"), "Caster MP");
    }

    // ========================================================================
    // CAMPAIGN VALIDATION
    // ========================================================================

    private static void TestNoOnUseRejectedOnLoad()
    {
        DAbility ability = CreateAbility();
        ability.actions[0].trigger = EAbilityTrigger.ONATTACK;

        AssertInvalidBattle(() => CreateFixture(ability));
    }

    private static void TestInvalidTargetRejectedOnLoad()
    {
        DAbility ability = CreateAbility();
        ability.target = (EAbilityTarget)16;

        AssertInvalidBattle(() => CreateFixture(ability));
    }

    private static void TestInvalidRangeRejectedOnLoad()
    {
        DAbility ability = CreateAbility();
        ability.minRange = 4;
        ability.maxRange = 3;

        AssertInvalidBattle(() => CreateFixture(ability));
    }

    private static void TestInvalidAreaRejectedOnLoad()
    {
        DAbility ability = CreateAbility();
        ability.area.Add(new Vector3(float.NaN, 0, 0));

        AssertInvalidBattle(() => CreateFixture(ability));
    }

    private static void TestInvalidDamageTypeRejectedOnLoad()
    {
        DAbility ability = CreateAbility();
        ability.actions[0].damageDefinition = new DamageDefinition("10", (EDamageType)3);

        AssertInvalidBattle(() => CreateFixture(ability));
    }

    // ========================================================================
    // AREA RESOLUTION
    // ========================================================================

    private static void TestAreaHitsPrimaryAndOffsetTargets()
    {
        DAbility ability = CreateAbility();
        ability.area.Add(new Vector3(1, 0, 0));

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));

        AssertEqual(90, GetHP(fixture, "Enemy"), "Primary target HP");
        AssertEqual(90, GetHP(fixture, "Enemy2"), "Area target HP");
    }

    private static void TestAreaCanHitAllies()
    {
        DAbility ability = CreateAbility();
        ability.area.Add(new Vector3(-2, 0, 0));

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));

        AssertEqual(90, GetHP(fixture, "Enemy"), "Primary target HP");
        AssertEqual(90, GetHP(fixture, "Ally"), "Allied area target HP");
        AssertEqual(100, GetHP(fixture, "Caster"), "Caster HP");
    }

    private static void TestDuplicateAreaOffsets()
    {
        DAbility ability = CreateAbility();
        ability.area.Add(Vector3.Zero);
        ability.area.Add(Vector3.Zero);

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));
        AssertEqual(90, GetHP(fixture, "Enemy"), "Primary target HP");
    }

    private static void TestDeadAreaUnitsAreIgnored()
    {
        DAbility ability = CreateAbility();
        ability.area.Add(new Vector3(1, 0, 0));

        Fixture fixture = CreateFixture(ability, enemy2HP: 0);

        AssertAccepted(Use(fixture));

        AssertEqual(90, GetHP(fixture, "Enemy"), "Primary target HP");
        AssertEqual(0, GetHP(fixture, "Enemy2"), "Defeated area target HP");
    }

    private static void TestAreaCanReachBeyondPrimaryRange()
    {
        DAbility ability = CreateAbility();
        ability.maxRange = 3;
        ability.area.Add(new Vector3(1, 0, 0));

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));
        AssertEqual(90, GetHP(fixture, "Enemy2"), "Area target HP");
    }

    // ========================================================================
    // ACTION EXECUTION, DAMAGE, HEALING, AND EFFECTS
    // ========================================================================

    private static void TestActionsExecuteInOrder()
    {
        DAbility ability = CreateAbility(formula: "5");
        ability.actions.Add(new DAbilityAction(EAbilityTrigger.ONUSE)
        {
            damageDefinition = new DamageDefinition("7", EDamageType.PURE)
        });

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));
        AssertEqual(88, GetHP(fixture, "Enemy"), "Enemy HP");
    }

    private static void TestDamageThenHealing()
    {
        DAbility ability = CreateAbility(EAbilityTarget.SELF, "20", EDamageType.PURE);
        ability.actions.Add(new DAbilityAction(EAbilityTrigger.ONUSE)
        {
            damageDefinition = new DamageDefinition("10", EDamageType.HEALING)
        });

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture, "Caster"));
        AssertEqual(90, GetHP(fixture, "Caster"), "Caster HP");
    }

    private static void TestEffectIsAdded()
    {
        DAbility ability = CreateAbility(formula: "0");
        ability.actions[0].addEffects.Add("DefenseBuff");

        DEffect effect = CreateDefenseEffect("DefenseBuff", 5);
        Fixture fixture = CreateFixture(ability, extraEffects: new[] { effect });

        AssertAccepted(Use(fixture));
        AssertEqual(7, GetStat(fixture, "Enemy", "DEF"), "Enemy defense");
    }

    private static void TestEffectIsRemoved()
    {
        DAbility ability = CreateAbility(formula: "0");
        ability.actions[0].addEffects.Add("DefenseBuff");

        DAbilityAction removeAction = new DAbilityAction(EAbilityTrigger.ONUSE)
        {
            damageDefinition = new DamageDefinition("0", EDamageType.PURE)
        };
        removeAction.remEffects.Add("DefenseBuff");
        ability.actions.Add(removeAction);

        DEffect effect = CreateDefenseEffect("DefenseBuff", 5);
        Fixture fixture = CreateFixture(ability, extraEffects: new[] { effect });

        AssertAccepted(Use(fixture));
        AssertEqual(2, GetStat(fixture, "Enemy", "DEF"), "Enemy defense");
    }

    private static void TestEffectChangesLaterFormula()
    {
        DAbility ability = CreateAbility(formula: "0");
        ability.actions[0].addEffects.Add("DefenseBuff");

        ability.actions.Add(new DAbilityAction(EAbilityTrigger.ONUSE)
        {
            damageDefinition = new DamageDefinition("target.DEF", EDamageType.PURE)
        });

        DEffect effect = CreateDefenseEffect("DefenseBuff", 5);
        Fixture fixture = CreateFixture(ability, extraEffects: new[] { effect });

        AssertAccepted(Use(fixture));
        AssertEqual(93, GetHP(fixture, "Enemy"), "Enemy HP");
    }

    // ========================================================================
    // FORMULA CALCULATION ERRORS
    // ========================================================================

    private static void TestFormulaErrorReturnsZeroAndContinues()
    {
        DAbility ability = CreateAbility(formula: "10 / 0");
        ability.actions.Add(new DAbilityAction(EAbilityTrigger.ONUSE)
        {
            damageDefinition = new DamageDefinition("1", EDamageType.PURE)
        });

        Fixture fixture = CreateFixture(ability);

        AssertAccepted(Use(fixture));

        AssertEqual(99, GetHP(fixture, "Enemy"), "Enemy HP");
        AssertEqual(25, GetMP(fixture, "Caster"), "Caster MP");
    }

    private static void TestEffectFormulaErrorReturnsZero()
    {
        DAbility ability = CreateAbility(formula: "0");
        ability.actions[0].addEffects.Add("BrokenEffect");

        DEffectAction brokenAction = new DEffectAction(EEffectTrigger.ONAPPLY)
        {
            damageDefinition = new DamageDefinition("10 / 0", EDamageType.PURE),
            stats = new Stats { defense = 5 }
        };

        DEffect brokenEffect = new DEffect("BrokenEffect", EEffectDuration.PERMANENT, 0, EEffectStacking.IGNORE, 1, new[] { brokenAction });

        Fixture fixture = CreateFixture(ability, extraEffects: new[] { brokenEffect });

        AssertAccepted(Use(fixture));

        AssertEqual(100, GetHP(fixture, "Enemy"), "Enemy HP");
        AssertEqual(25, GetMP(fixture, "Caster"), "Caster MP");
        AssertEqual(7, GetStat(fixture, "Enemy", "DEF"), "Enemy defense");
    }
}