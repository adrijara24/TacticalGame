
using System;
using System.Globalization;
using Tactical.Core.Domain.Units;

namespace Tactical.Core.Domain;

// ============================================================================
// DAMAGE FORMULA REFERENCE
// ============================================================================
//
// A damage formula calculates the RAW damage value before defense,
// magic resistance, or healing rules are applied.
//
// ---------------------------------------------------------------------------
// 1. UNIT REFERENCES
// ---------------------------------------------------------------------------
//
// There are two available unit references:
//
//     user    -> The unit executing the attack or ability.
//     target  -> The unit receiving the damage or healing.
//
// A stat is accessed using:
//
//     user.STAT
//     target.STAT
//
// References are case-insensitive. For example, user.STR and USER.str
// are both valid.
//
// ---------------------------------------------------------------------------
// 2. AVAILABLE STATS
// ---------------------------------------------------------------------------
//
// Both user and target support the following stats:
//
//     MHP -> Maximum health
//     MMP -> Maximum mana
//     STR -> Strength
//     MAG -> Magic
//     DEF -> Physical defense
//     RES -> Magical defense (magicDefense)
//     PRE -> Precision
//     EVA -> Evasion
//     MOV -> Movement
//     RNG -> Range
//     SPD -> Speed
//     CRI -> Critical rate
//
// Examples:
//
//     user.STR
//     target.DEF
//     user.MAG + target.RES
//
// NOTE: Current HP and current mana are not available as formula references.
// MHP and MMP refer to maximum health and maximum mana.
//
// ---------------------------------------------------------------------------
// 3. OPERATORS
// ---------------------------------------------------------------------------
//
// Supported arithmetic operators:
//
//     +   Addition
//     -   Subtraction
//     *   Multiplication
//     /   Division
//
// Unary operators are also supported:
//
//     +5
//     -5
//
// Parentheses can be used to group expressions:
//
//     (user.STR + user.MAG) / 2
//
// Operator precedence follows standard arithmetic rules:
//     1. Parentheses
//     2. Unary + and -
//     3. Multiplication and division
//     4. Addition and subtraction
//
// ---------------------------------------------------------------------------
// 4. NUMBERS AND WHITESPACE
// ---------------------------------------------------------------------------
//
// Integer and decimal numbers are supported.
// A dot (.) is used as the decimal separator.
//
// Examples:
//
//     10
//     10.5
//     .5
//     10.
//
// Whitespace and line breaks are ignored between tokens.
//
// Examples:
//
//     10 + user.STR
//     (user.STR + target.DEF) * 2
//
// ---------------------------------------------------------------------------
// 5. EXAMPLES
// ---------------------------------------------------------------------------
//
// Fixed damage:
//
//     10
//
// Damage based on strength:
//
//     user.STR * 2
//
// Damage reduced by target defense:
//
//     user.STR * 2 - target.DEF
//
// Damage based on magic:
//
//     user.MAG * 1.5 - target.RES
//
// Average of two stats:
//
//     (user.STR + user.MAG) / 2
//
// More complex formula:
//
//     10 + user.STR * 2 - target.DEF
//
// Healing based on the user's magic:
//
//     user.MAG * 2
//
//
// ---------------------------------------------------------------------------
// 6. VALIDATION AND EXECUTION
// ---------------------------------------------------------------------------
//
// Validate a formula before accepting an asset:
//
//     if (!DamageDefinition.TryValidateFormula(formula, out string error))
//         Console.WriteLine(error);
//
// Create a damage instance using the effective stats of both units:
//
//     DamageInstance damage = definition.CreateDamageInstance(source.Stats, target.Stats);
//
// The resulting decimal value is rounded to the nearest integer.
// Midpoint values are rounded away from zero.
//
// The formula calculates raw damage only. DamageInstance.Apply() is still
// responsible for applying defense, resistance, healing, and HP limits.
//
// IMPORTANT:
// TryValidateFormula() checks syntax and stat references. It does not
// evaluate the formula using actual unit stats. A formula with a divisor
// that evaluates to zero can therefore pass validation and fail at runtime, returning 0.
// ============================================================================
internal sealed class DamageFormulaParser
{
    private readonly string formula;
    private readonly Stats userStats;
    private readonly Stats targetStats;
    private readonly bool validateOnly;

    private int position;

    private DamageFormulaParser(string formula, Stats userStats, Stats targetStats, bool validateOnly)
    {
        this.formula = formula;
        this.userStats = userStats;
        this.targetStats = targetStats;
        this.validateOnly = validateOnly;
    }

    public static int Evaluate(string formula, Stats userStats, Stats targetStats)
    {
        if (string.IsNullOrWhiteSpace(formula))
            throw new FormatException("Damage formula cannot be empty.");

        try
        {
            DamageFormulaParser parser = new DamageFormulaParser(formula, userStats, targetStats, false);
            decimal result = parser.ParseComplete();
            decimal rounded = decimal.Round(result, 0, MidpointRounding.AwayFromZero);

            if (rounded < int.MinValue || rounded > int.MaxValue)
                return 0;

            return decimal.ToInt32(rounded);
        }
        catch (DivideByZeroException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    public static void ValidateSyntax(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            throw new FormatException("Damage formula cannot be empty.");

        DamageFormulaParser parser = new DamageFormulaParser(formula, default, default, true);
        parser.ParseComplete();
    }

    private decimal ParseComplete()
    {
        decimal result = ParseExpression();

        SkipWhitespace();

        if (position < formula.Length)
            throw Error("Unexpected character '" + formula[position] + "'.");

        return result;
    }

    private decimal ParseExpression()
    {
        decimal value = ParseTerm();

        while (true)
        {
            SkipWhitespace();

            if (Match('+'))
            {
                decimal right = ParseTerm();
                value = validateOnly ? 1 : value + right;
            }
            else if (Match('-'))
            {
                decimal right = ParseTerm();
                value = validateOnly ? 1 : value - right;
            }
            else
                return value;
        }
    }

    private decimal ParseTerm()
    {
        decimal value = ParseUnary();

        while (true)
        {
            SkipWhitespace();

            if (Match('*'))
            {
                decimal right = ParseUnary();
                value = validateOnly ? 1 : value * right;
            }
            else if (Match('/'))
            {
                decimal right = ParseUnary();

                if (!validateOnly && right == 0)
                    throw new DivideByZeroException();

                value = validateOnly ? 1 : value / right;
            }
            else
                return value;
        }
    }

    private decimal ParseUnary()
    {
        SkipWhitespace();

        if (Match('+'))
            return ParseUnary();

        if (Match('-'))
        {
            decimal value = ParseUnary();
            return validateOnly ? 1 : -value;
        }

        return ParsePrimary();
    }

    private decimal ParsePrimary()
    {
        SkipWhitespace();

        if (Match('('))
        {
            decimal value = ParseExpression();

            SkipWhitespace();

            if (!Match(')'))
                throw Error("Missing closing parenthesis.");

            return value;
        }

        if (position < formula.Length && (char.IsDigit(formula[position]) || formula[position] == '.'))
            return ParseNumber();

        return ParseIdentifier();
    }

    private decimal ParseNumber()
    {
        int start = position;
        bool hasDigit = false;

        while (position < formula.Length && char.IsDigit(formula[position]))
        {
            position++;
            hasDigit = true;
        }

        if (position < formula.Length && formula[position] == '.')
        {
            position++;

            while (position < formula.Length && char.IsDigit(formula[position]))
            {
                position++;
                hasDigit = true;
            }
        }

        if (!hasDigit)
            throw Error("Invalid numeric literal.");

        string number = formula.Substring(start, position - start);

        if (!decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal value))
            throw Error("Invalid numeric literal '" + number + "'.");

        return validateOnly ? 1 : value;
    }

    private decimal ParseIdentifier()
    {
        int start = position;

        while (position < formula.Length &&
               (char.IsLetterOrDigit(formula[position]) || formula[position] == '_' || formula[position] == '.'))
            position++;

        if (start == position)
            throw Error("Expected a number, stat reference, or opening parenthesis.");

        string identifier = formula.Substring(start, position - start);
        string[] parts = identifier.Split('.');

        if (parts.Length != 2)
            throw Error("Invalid stat reference '" + identifier + "'. Expected user.STAT or target.STAT.");

        Stats stats;

        if (parts[0].Equals("user", StringComparison.OrdinalIgnoreCase))
            stats = userStats;
        else if (parts[0].Equals("target", StringComparison.OrdinalIgnoreCase))
            stats = targetStats;
        else
            throw Error("Unknown unit reference '" + parts[0] + "'.");

        int value = GetStat(stats, parts[1]);

        return validateOnly ? 1 : value;
    }

    private int GetStat(Stats stats, string stat)
    {
        return stat.ToUpperInvariant() switch
        {
            "MHP" => stats.hp,
            "MMP" => stats.mana,
            "STR" => stats.strength,
            "MAG" => stats.magic,
            "DEF" => stats.defense,
            "RES" => stats.magicDefense,
            "PRE" => stats.precision,
            "EVA" => stats.evasion,
            "MOV" => stats.movement,
            "RNG" => stats.range,
            "SPD" => stats.speed,
            "CRI" => stats.criticalRate,
            _ => throw Error("Unknown stat '" + stat + "'.")
        };
    }

    private bool Match(char character)
    {
        if (position >= formula.Length || formula[position] != character)
            return false;

        position++;
        return true;
    }

    private void SkipWhitespace()
    {
        while (position < formula.Length && char.IsWhiteSpace(formula[position]))
            position++;
    }

    private FormatException Error(string message)
    {
        return new FormatException(message + " Position " + (position + 1) + " in formula '" + formula + "'.");
    }
}
