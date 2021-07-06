using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Centrifuge.Distance.Game;

namespace DistanceRando.Randomizer
{
    class RandoCarAbilityState
    {
        internal bool jumpShouldBeEnabled = false;
        internal bool wingsShouldBeEnabled = false;
        internal bool jetsShouldBeEnabled = false;

        readonly RandoGame randoGame;


        public RandoCarAbilityState(RandoGame randoGame)
        {
            this.randoGame = randoGame;
        }

        public void UpdateAbilityState()
        {
            RandoMap map = randoGame.maps[Game.LevelName];
            CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
            if (car.Jump_.AbilityEnabled_ && !map.jumpEnabled)
            {
                jumpShouldBeEnabled = true;
            }
            if (car.Wings_.AbilityEnabled_ && !map.wingsEnabled)
            {
                wingsShouldBeEnabled = true;
            }
            if (car.Jets_.AbilityEnabled_ && !map.jetsEnabled)
            {
                jetsShouldBeEnabled = true;
            }
        }

        public void SetCarAbilities(bool singleRaceStarted)
        {
            CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
            try
            {
                RandoMap map = randoGame.maps[Game.LevelName];
                if (!singleRaceStarted)
                {
                    jumpShouldBeEnabled = map.jumpEnabled;
                    wingsShouldBeEnabled = map.wingsEnabled;
                    jetsShouldBeEnabled = map.jetsEnabled;
                }
                car.Boost_.AbilityEnabled_ = map.boostEnabled;
                car.Jump_.AbilityEnabled_ = jumpShouldBeEnabled;
                car.Wings_.AbilityEnabled_ = wingsShouldBeEnabled;
                car.Jets_.AbilityEnabled_ = jetsShouldBeEnabled;

                // Disable stock show scores so we can display our own thing.
                car.CarLogicLocal_.PlayerDataLocal_.EnableOrDisableShowScores(false);

                Console.WriteLine($"Jump {car.Jump_.AbilityEnabled_} - Wings {car.Wings_.AbilityEnabled_} - Jets {car.Jets_.AbilityEnabled_}");

            }
            catch (KeyNotFoundException)
            {
                // this should only ever happen on Credits or Destination Unknown, so just enable everything
                car.Boost_.AbilityEnabled_ = true;
                car.Jump_.AbilityEnabled_ = true;
                car.Wings_.AbilityEnabled_ = true;
                car.Jets_.AbilityEnabled_ = true;
            }

            car.GetComponent<LocalPlayerControlledCar>().showBackToResetWarning_ = false;
        }
    }
}
