using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistanceRando.Randomizer.Logic
{
    public abstract class LogicRequirement
    {
        public abstract bool IsRequirementMet(bool jump, bool wings, bool jets);
    }

    public class SingleAbilityRequirement : LogicRequirement
    {
        readonly Ability Requirement;

        public SingleAbilityRequirement(Ability requirement)
        {
            this.Requirement = requirement; 
        }

        public override bool IsRequirementMet(bool jump, bool wings, bool jets)
        {
            if ((Requirement == Ability.Jump && jump == true) ||
                (Requirement == Ability.Wings && wings == true) ||
                (Requirement == Ability.Jets && jets == true) ||
                (Requirement == Ability.Boost) || // boost is currently always enabled, so this should return true no matter what
                (Requirement == Ability.None)) // and obviously none means no requirement, so that should also always return true

            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class OrAbilityRequirement : LogicRequirement
    {
        readonly SingleAbilityRequirement Req1;
        readonly SingleAbilityRequirement Req2;

        public OrAbilityRequirement(Ability a, Ability b)
        {
            Req1 = new SingleAbilityRequirement(a);
            Req2 = new SingleAbilityRequirement(b);
        }

        public override bool IsRequirementMet(bool jump, bool wings, bool jets)
        {
            var result1 = Req1.IsRequirementMet(jump, wings, jets);
            var result2 = Req2.IsRequirementMet(jump, wings, jets);

            return result1 || result2;
        }
    }
}
