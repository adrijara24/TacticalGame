using System;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Battle;
using Tactical.Core.Domain.Units;

namespace Tactical.Test;

public static class DamageFormulaValidatorTests
{
    public static void RunTests()
    {
        int passed = 0;
        int failed = 0;

        Console.WriteLine("Running Damage Formula Tests...");
        Console.WriteLine("------------------------------------------");

        // Validation: empty formulas
        RunTest(nameof(TestNullFormula), TestNullFormula, ref passed, ref failed);
        RunTest(nameof(TestEmptyFormula), TestEmptyFormula, ref passed, ref failed);
        RunTest(nameof(TestWhitespaceFormula), TestWhitespaceFormula, ref passed, ref failed);

        // Validation: numeric literals
        RunTest(nameof(TestInteger), TestInteger, ref passed, ref failed);
        RunTest(nameof(TestDecimal), TestDecimal, ref passed, ref failed);
        RunTest(nameof(TestLeadingDecimal), TestLeadingDecimal, ref passed, ref failed);
        RunTest(nameof(TestTrailingDecimal), TestTrailingDecimal, ref passed, ref failed);

        // Validation: arithmetic syntax
        RunTest(nameof(TestAdditionSyntax), TestAdditionSyntax, ref passed, ref failed);
        RunTest(nameof(TestSubtractionSyntax), TestSubtractionSyntax, ref passed, ref failed);
        RunTest(nameof(TestMultiplicationSyntax), TestMultiplicationSyntax, ref passed, ref failed);
        RunTest(nameof(TestDivisionSyntax), TestDivisionSyntax, ref passed, ref failed);
        RunTest(nameof(TestParenthesesSyntax), TestParenthesesSyntax, ref passed, ref failed);
        RunTest(nameof(TestNestedParenthesesSyntax), TestNestedParenthesesSyntax, ref passed, ref failed);
        RunTest(nameof(TestWhitespace), TestWhitespace, ref passed, ref failed);
        RunTest(nameof(TestNewlines), TestNewlines, ref passed, ref failed);

        // Validation: stat references
        RunTest(nameof(TestAllUserStatReferences), TestAllUserStatReferences, ref passed, ref failed);
        RunTest(nameof(TestAllTargetStatReferences), TestAllTargetStatReferences, ref passed, ref failed);
        RunTest(nameof(TestCaseInsensitiveReferences), TestCaseInsensitiveReferences, ref passed, ref failed);

        // Validation: invalid references
        RunTest(nameof(TestUnknownScope), TestUnknownScope, ref passed, ref failed);
        RunTest(nameof(TestUnknownStat), TestUnknownStat, ref passed, ref failed);
        RunTest(nameof(TestMissingStat), TestMissingStat, ref passed, ref failed);
        RunTest(nameof(TestStatWithoutScope), TestStatWithoutScope, ref passed, ref failed);
        RunTest(nameof(TestExtraReferenceSegment), TestExtraReferenceSegment, ref passed, ref failed);
        RunTest(nameof(TestEmptyReferenceSegment), TestEmptyReferenceSegment, ref passed, ref failed);

        // Validation: invalid expressions
        RunTest(nameof(TestMissingOperand), TestMissingOperand, ref passed, ref failed);
        RunTest(nameof(TestConsecutiveOperators), TestConsecutiveOperators, ref passed, ref failed);
        RunTest(nameof(TestMissingClosingParenthesis), TestMissingClosingParenthesis, ref passed, ref failed);
        RunTest(nameof(TestExtraClosingParenthesis), TestExtraClosingParenthesis, ref passed, ref failed);
        RunTest(nameof(TestEmptyParentheses), TestEmptyParentheses, ref passed, ref failed);
        RunTest(nameof(TestAdjacentOperands), TestAdjacentOperands, ref passed, ref failed);
        RunTest(nameof(TestBareDecimalPoint), TestBareDecimalPoint, ref passed, ref failed);
        RunTest(nameof(TestMultipleDecimalPoints), TestMultipleDecimalPoints, ref passed, ref failed);
        RunTest(nameof(TestUnsupportedModulo), TestUnsupportedModulo, ref passed, ref failed);
        RunTest(nameof(TestUnsupportedExponentiation), TestUnsupportedExponentiation, ref passed, ref failed);
        RunTest(nameof(TestScientificNotation), TestScientificNotation, ref passed, ref failed);
        RunTest(nameof(TestInvalidCharacters), TestInvalidCharacters, ref passed, ref failed);
        RunTest(nameof(TestWhitespaceInsideReference), TestWhitespaceInsideReference, ref passed, ref failed);

        // Evaluation: numeric literals and rounding
        RunTest(nameof(TestEvaluateInteger), TestEvaluateInteger, ref passed, ref failed);
        RunTest(nameof(TestEvaluateZero), TestEvaluateZero, ref passed, ref failed);
        RunTest(nameof(TestEvaluateDecimal), TestEvaluateDecimal, ref passed, ref failed);
        RunTest(nameof(TestEvaluateDecimalRoundingDown), TestEvaluateDecimalRoundingDown, ref passed, ref failed);
        RunTest(nameof(TestEvaluateDecimalRoundingUp), TestEvaluateDecimalRoundingUp, ref passed, ref failed);
        RunTest(nameof(TestEvaluateNegativeRounding), TestEvaluateNegativeRounding, ref passed, ref failed);

        // Evaluation: arithmetic
        RunTest(nameof(TestEvaluateAddition), TestEvaluateAddition, ref passed, ref failed);
        RunTest(nameof(TestEvaluateSubtraction), TestEvaluateSubtraction, ref passed, ref failed);
        RunTest(nameof(TestEvaluateMultiplication), TestEvaluateMultiplication, ref passed, ref failed);
        RunTest(nameof(TestEvaluateDivision), TestEvaluateDivision, ref passed, ref failed);
        RunTest(nameof(TestEvaluateDivisionWithRemainder), TestEvaluateDivisionWithRemainder, ref passed, ref failed);
        RunTest(nameof(TestEvaluateOperatorPrecedence), TestEvaluateOperatorPrecedence, ref passed, ref failed);
        RunTest(nameof(TestEvaluateParentheses), TestEvaluateParentheses, ref passed, ref failed);
        RunTest(nameof(TestEvaluateNestedParentheses), TestEvaluateNestedParentheses, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUnaryPlus), TestEvaluateUnaryPlus, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUnaryMinus), TestEvaluateUnaryMinus, ref passed, ref failed);
        RunTest(nameof(TestEvaluateConsecutiveUnaryOperators), TestEvaluateConsecutiveUnaryOperators, ref passed, ref failed);
        RunTest(nameof(TestEvaluateLeftAssociativity), TestEvaluateLeftAssociativity, ref passed, ref failed);

        // Evaluation: user stats
        RunTest(nameof(TestEvaluateUserMHP), TestEvaluateUserMHP, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserMMP), TestEvaluateUserMMP, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserSTR), TestEvaluateUserSTR, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserMAG), TestEvaluateUserMAG, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserDEF), TestEvaluateUserDEF, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserRES), TestEvaluateUserRES, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserPRE), TestEvaluateUserPRE, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserEVA), TestEvaluateUserEVA, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserMOV), TestEvaluateUserMOV, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserRNG), TestEvaluateUserRNG, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserSPD), TestEvaluateUserSPD, ref passed, ref failed);
        RunTest(nameof(TestEvaluateUserCRI), TestEvaluateUserCRI, ref passed, ref failed);

        // Evaluation: target stats
        RunTest(nameof(TestEvaluateTargetMHP), TestEvaluateTargetMHP, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetMMP), TestEvaluateTargetMMP, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetSTR), TestEvaluateTargetSTR, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetMAG), TestEvaluateTargetMAG, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetDEF), TestEvaluateTargetDEF, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetRES), TestEvaluateTargetRES, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetPRE), TestEvaluateTargetPRE, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetEVA), TestEvaluateTargetEVA, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetMOV), TestEvaluateTargetMOV, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetRNG), TestEvaluateTargetRNG, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetSPD), TestEvaluateTargetSPD, ref passed, ref failed);
        RunTest(nameof(TestEvaluateTargetCRI), TestEvaluateTargetCRI, ref passed, ref failed);

        // Evaluation: combined formulas
        RunTest(nameof(TestEvaluateUserAndTarget), TestEvaluateUserAndTarget, ref passed, ref failed);
        RunTest(nameof(TestEvaluateDamageFormula), TestEvaluateDamageFormula, ref passed, ref failed);
        RunTest(nameof(TestEvaluateMagicFormula), TestEvaluateMagicFormula, ref passed, ref failed);
        RunTest(nameof(TestEvaluateComplexFormula), TestEvaluateComplexFormula, ref passed, ref failed);
        RunTest(nameof(TestEvaluateNegativeResult), TestEvaluateNegativeResult, ref passed, ref failed);
        RunTest(nameof(TestEvaluateCaseInsensitiveReferences), TestEvaluateCaseInsensitiveReferences, ref passed, ref failed);

        // Evaluation: runtime errors
        RunTest(nameof(TestEvaluateDivisionByZero), TestEvaluateDivisionByZero, ref passed, ref failed);
        RunTest(nameof(TestEvaluateStatBasedDivisionByZero), TestEvaluateStatBasedDivisionByZero, ref passed, ref failed);

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

    // ------------------------------------------------------------------------
    // Test helpers
    // ------------------------------------------------------------------------

    private static Stats CreateUserStats()
    {
        return new Stats
        {
            hp = 100,
            mana = 50,
            strength = 12,
            magic = 8,
            defense = 6,
            magicDefense = 4,
            precision = 75,
            evasion = 10,
            movement = 5,
            range = 3,
            speed = 7,
            criticalRate = 15
        };
    }

    private static Stats CreateTargetStats()
    {
        return new Stats
        {
            hp = 80,
            mana = 30,
            strength = 4,
            magic = 6,
            defense = 8,
            magicDefense = 5,
            precision = 60,
            evasion = 12,
            movement = 4,
            range = 2,
            speed = 9,
            criticalRate = 20
        };
    }

    private static int Evaluate(string formula)
    {
        Stats user = CreateUserStats();
        Stats target = CreateTargetStats();

        DamageDefinition definition = new DamageDefinition(formula, EDamageType.PURE);
        DamageInstance damage = definition.CreateDamageInstance(user, target);

        return damage.Value;
    }

    private static void AssertResult(string formula, int expected)
    {
        int actual = Evaluate(formula);

        if (actual != expected)
            throw new InvalidOperationException("Formula '" + formula + "': expected " + expected + ", got " + actual + ".");
    }

    private static void AssertValid(string formula)
    {
        if (!DamageDefinition.TryValidateFormula(formula, out string error))
            throw new InvalidOperationException("Expected valid formula, but got: " + error);

        if (!string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Valid formula returned an error message: " + error);
    }

    private static void AssertInvalid(string formula)
    {
        if (DamageDefinition.TryValidateFormula(formula, out string error))
            throw new InvalidOperationException("Expected invalid formula, but validation succeeded.");

        if (string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Invalid formula did not provide an error message.");
    }

    private static void AssertThrowsFormatException(Action action)
    {
        try
        {
            action();
        }
        catch (FormatException)
        {
            return;
        }

        throw new InvalidOperationException("Expected a FormatException, but none was thrown.");
    }

    // ------------------------------------------------------------------------
    // Validation tests: valid formulas
    // ------------------------------------------------------------------------

    private static void TestNullFormula() => AssertInvalid(null!);
    private static void TestEmptyFormula() => AssertInvalid("");
    private static void TestWhitespaceFormula() => AssertInvalid("   ");

    private static void TestInteger() => AssertValid("10");
    private static void TestDecimal() => AssertValid("10.5");
    private static void TestLeadingDecimal() => AssertValid(".5");
    private static void TestTrailingDecimal() => AssertValid("10.");

    private static void TestAdditionSyntax() => AssertValid("10 + 5");
    private static void TestSubtractionSyntax() => AssertValid("10 - 5");
    private static void TestMultiplicationSyntax() => AssertValid("10 * 5");
    private static void TestDivisionSyntax() => AssertValid("10 / 5");
    private static void TestParenthesesSyntax() => AssertValid("(10 + 5) * 2");
    private static void TestNestedParenthesesSyntax() => AssertValid("((10 + 5) * (2 + 3)) / 5");
    private static void TestWhitespace() => AssertValid(" 10 + user.STR ");
    private static void TestNewlines() => AssertValid("10 +\nuser.STR");

    private static void TestAllUserStatReferences() => AssertValid("user.MHP + user.MMP + user.STR + user.MAG + user.DEF + user.RES + user.PRE + user.EVA + user.MOV + user.RNG + user.SPD + user.CRI");
    private static void TestAllTargetStatReferences() => AssertValid("target.MHP + target.MMP + target.STR + target.MAG + target.DEF + target.RES + target.PRE + target.EVA + target.MOV + target.RNG + target.SPD + target.CRI");
    private static void TestCaseInsensitiveReferences() => AssertValid("UsEr.StR + TaRgEt.DeF");

    // ------------------------------------------------------------------------
    // Validation tests: invalid references and expressions
    // ------------------------------------------------------------------------

    private static void TestUnknownScope() => AssertInvalid("attacker.STR");
    private static void TestUnknownStat() => AssertInvalid("user.POW");
    private static void TestMissingStat() => AssertInvalid("user.");
    private static void TestStatWithoutScope() => AssertInvalid("STR");
    private static void TestExtraReferenceSegment() => AssertInvalid("user.STR.extra");
    private static void TestEmptyReferenceSegment() => AssertInvalid("user..STR");

    private static void TestMissingOperand() => AssertInvalid("10 +");
    private static void TestConsecutiveOperators() => AssertInvalid("10 + * 2");
    private static void TestMissingClosingParenthesis() => AssertInvalid("(10 + 5");
    private static void TestExtraClosingParenthesis() => AssertInvalid("10 + 5)");
    private static void TestEmptyParentheses() => AssertInvalid("()");
    private static void TestAdjacentOperands() => AssertInvalid("10 20");
    private static void TestBareDecimalPoint() => AssertInvalid(".");
    private static void TestMultipleDecimalPoints() => AssertInvalid("1..2");
    private static void TestUnsupportedModulo() => AssertInvalid("10 % 3");
    private static void TestUnsupportedExponentiation() => AssertInvalid("10 ^ 2");
    private static void TestScientificNotation() => AssertInvalid("1e3");
    private static void TestInvalidCharacters() => AssertInvalid("10 & 5");
    private static void TestWhitespaceInsideReference() => AssertInvalid("user .STR");

    // ------------------------------------------------------------------------
    // Evaluation tests: numeric literals and rounding
    // ------------------------------------------------------------------------

    private static void TestEvaluateInteger() => AssertResult("10", 10);
    private static void TestEvaluateZero() => AssertResult("0", 0);
    private static void TestEvaluateDecimal() => AssertResult("10.5", 11);
    private static void TestEvaluateDecimalRoundingDown() => AssertResult("10.4", 10);
    private static void TestEvaluateDecimalRoundingUp() => AssertResult("10.6", 11);
    private static void TestEvaluateNegativeRounding() => AssertResult("-10.5", -11);

    // ------------------------------------------------------------------------
    // Evaluation tests: arithmetic
    // ------------------------------------------------------------------------

    private static void TestEvaluateAddition() => AssertResult("10 + 5", 15);
    private static void TestEvaluateSubtraction() => AssertResult("10 - 5", 5);
    private static void TestEvaluateMultiplication() => AssertResult("10 * 5", 50);
    private static void TestEvaluateDivision() => AssertResult("10 / 2", 5);
    private static void TestEvaluateDivisionWithRemainder() => AssertResult("10 / 4", 3);
    private static void TestEvaluateOperatorPrecedence() => AssertResult("10 + 5 * 2", 20);
    private static void TestEvaluateParentheses() => AssertResult("(10 + 5) * 2", 30);
    private static void TestEvaluateNestedParentheses() => AssertResult("((10 + 5) * (2 + 3)) / 5", 15);
    private static void TestEvaluateUnaryPlus() => AssertResult("+10", 10);
    private static void TestEvaluateUnaryMinus() => AssertResult("-10", -10);
    private static void TestEvaluateConsecutiveUnaryOperators() => AssertResult("1--2", 3);
    private static void TestEvaluateLeftAssociativity() => AssertResult("20 / 5 / 2", 2);

    // ------------------------------------------------------------------------
    // Evaluation tests: user stats
    // ------------------------------------------------------------------------

    private static void TestEvaluateUserMHP() => AssertResult("user.MHP", 100);
    private static void TestEvaluateUserMMP() => AssertResult("user.MMP", 50);
    private static void TestEvaluateUserSTR() => AssertResult("user.STR", 12);
    private static void TestEvaluateUserMAG() => AssertResult("user.MAG", 8);
    private static void TestEvaluateUserDEF() => AssertResult("user.DEF", 6);
    private static void TestEvaluateUserRES() => AssertResult("user.RES", 4);
    private static void TestEvaluateUserPRE() => AssertResult("user.PRE", 75);
    private static void TestEvaluateUserEVA() => AssertResult("user.EVA", 10);
    private static void TestEvaluateUserMOV() => AssertResult("user.MOV", 5);
    private static void TestEvaluateUserRNG() => AssertResult("user.RNG", 3);
    private static void TestEvaluateUserSPD() => AssertResult("user.SPD", 7);
    private static void TestEvaluateUserCRI() => AssertResult("user.CRI", 15);

    // ------------------------------------------------------------------------
    // Evaluation tests: target stats
    // ------------------------------------------------------------------------

    private static void TestEvaluateTargetMHP() => AssertResult("target.MHP", 80);
    private static void TestEvaluateTargetMMP() => AssertResult("target.MMP", 30);
    private static void TestEvaluateTargetSTR() => AssertResult("target.STR", 4);
    private static void TestEvaluateTargetMAG() => AssertResult("target.MAG", 6);
    private static void TestEvaluateTargetDEF() => AssertResult("target.DEF", 8);
    private static void TestEvaluateTargetRES() => AssertResult("target.RES", 5);
    private static void TestEvaluateTargetPRE() => AssertResult("target.PRE", 60);
    private static void TestEvaluateTargetEVA() => AssertResult("target.EVA", 12);
    private static void TestEvaluateTargetMOV() => AssertResult("target.MOV", 4);
    private static void TestEvaluateTargetRNG() => AssertResult("target.RNG", 2);
    private static void TestEvaluateTargetSPD() => AssertResult("target.SPD", 9);
    private static void TestEvaluateTargetCRI() => AssertResult("target.CRI", 20);

    // ------------------------------------------------------------------------
    // Evaluation tests: combined formulas
    // ------------------------------------------------------------------------

    private static void TestEvaluateUserAndTarget() => AssertResult("user.STR + target.DEF", 20);
    private static void TestEvaluateDamageFormula() => AssertResult("user.STR * 2 - target.DEF", 16);
    private static void TestEvaluateMagicFormula() => AssertResult("user.MAG * 2 - target.RES", 11);
    private static void TestEvaluateComplexFormula() => AssertResult("10 + user.STR * 2 - target.DEF + (user.MAG / 2)", 30);
    private static void TestEvaluateNegativeResult() => AssertResult("target.DEF - user.STR", -4);
    private static void TestEvaluateCaseInsensitiveReferences() => AssertResult("UsEr.StR + TaRgEt.DeF", 20);

    // ------------------------------------------------------------------------
    // Evaluation tests: runtime errors
    // ------------------------------------------------------------------------

    private static void TestEvaluateDivisionByZero() => AssertThrowsFormatException(() => Evaluate("10 / 0"));
    private static void TestEvaluateStatBasedDivisionByZero() => AssertThrowsFormatException(() => Evaluate("user.STR / (target.DEF - target.DEF)"));
}