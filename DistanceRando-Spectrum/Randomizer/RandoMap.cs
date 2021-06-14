using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistanceRando
{
    public enum Ability { None, Boost, Jump, Wings, Jets }

    public class RandoMap
    {
        public Ability abilityEnabled = Ability.None;
        public bool boostEnabled = false;
        public bool jumpEnabled = false;
        public bool wingsEnabled = false;
        public bool jetsEnabled = false;

        public RandoMap(Ability abilityEnabled, bool boostEnabled, bool jumpEnabled, bool wingsEnabled, bool jetsEnabled)
        {
            this.abilityEnabled = abilityEnabled;
            this.boostEnabled = boostEnabled;
            this.jumpEnabled = jumpEnabled;
            this.wingsEnabled = wingsEnabled;
            this.jetsEnabled = jetsEnabled;
        }
    }
}
