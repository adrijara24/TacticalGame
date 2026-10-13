using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Tactical.Core.Domain.Battle
{
    /// <summary>
    /// Defines the a position inside a Map
    /// </summary>
    /// <param name="X">Horizontal axis</param>
    /// <param name="Y">Elevation axis</param>
    /// <param name="Z">Depth axis</param>
    public readonly record struct BattlePosition(int X, int Y, int Z)
    {
        public Vector3 ToVector3() => new Vector3(X, Y, Z);

        public static bool TryFromVector3(Vector3 value, out BattlePosition position)
        {
            position = default;
            if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z))
                return false;
            if (!IsGridCoordinate(value.X) || !IsGridCoordinate(value.Y) || !IsGridCoordinate(value.Z))
                return false;
            position = new BattlePosition((int)MathF.Round(value.X), (int)MathF.Round(value.Y), (int)MathF.Round(value.Z));
            return true;

        }

        private static bool IsGridCoordinate(float value)
        {
            if (value < int.MinValue || value > int.MaxValue)
                return false;
            return MathF.Abs(value - MathF.Round(value)) <= 0.0001f;
        }

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
