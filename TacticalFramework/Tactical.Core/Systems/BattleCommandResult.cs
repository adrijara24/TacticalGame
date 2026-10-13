using System;
using System.Collections.Generic;
using System.Text;

namespace Tactical.Core.Systems
{
    public readonly struct BattleCommandResult(bool status, string message)
    {
        public bool Status => status;

        public string Error => status ? "" : message;

        public string Output => status ? message : "";
    }
}
