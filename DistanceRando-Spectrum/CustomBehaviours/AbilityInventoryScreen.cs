using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistanceRando
{
    class AbilityInventoryScreen
    {
        internal void Update()
        {
            var playerDataLocal = G.Sys.PlayerManager_.Current_.playerData_;
            if (playerDataLocal)
            {
                if (!playerDataLocal.EnableShowScores_ && playerDataLocal.InputStates_.GetTriggered(InputAction.ShowScore))
                {
                    AbilityBatteryChange[] abilityBatteryChanges = new AbilityBatteryChange[4];

                    if (playerDataLocal.CarLogic_.Boost_.AbilityEnabled_) abilityBatteryChanges[0] = AbilityBatteryChange.Enable;
                    if (playerDataLocal.CarLogic_.Jump_.AbilityEnabled_) abilityBatteryChanges[1] = AbilityBatteryChange.Enable;
                    if (playerDataLocal.CarLogic_.Wings_.AbilityEnabled_) abilityBatteryChanges[2] = AbilityBatteryChange.Enable;
                    if (playerDataLocal.CarLogic_.Jets_.AbilityEnabled_) abilityBatteryChanges[3] = AbilityBatteryChange.Enable;

                    if (playerDataLocal.CarScreenLogic_ != null)
                    {
                        playerDataLocal.CarScreenLogic_.StopScrensaver();

                        int curMap = G.Sys.GameManager_.GetCurrentPlaylistIndex();

                        playerDataLocal.CarScreenLogic_.EnableAbilityBattery(abilityBatteryChanges, $"map {curMap}/16");
                        AudioManager.PostEvent("Play_OpenMap", playerDataLocal.Car_);
                    }
                }
            }
        }
    }
}
