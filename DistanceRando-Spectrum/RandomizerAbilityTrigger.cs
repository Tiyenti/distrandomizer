using System;
using System.Collections.Generic;
using System.Linq;
// Custom ability trigger to fix the wacky incosistent behaviour of stock SetAbilitiesTriggers
// (also allows for more flexibility)

using System.Text;
using UnityEngine;
using Centrifuge.Distance.Game;

namespace DistanceRando
{
    class RandomizerAbilityTrigger : MonoBehaviour
    {
        internal bool enableBoosting = false;
        internal bool enableJumping = false;
        internal bool enableFlying = false;
        internal bool enableJetRotating = false;

        internal bool oneTimeTrigger = true;
        bool triggered = false;

        internal bool showAbilityBattery = true;

        void OnTriggerEnter(Collider other)
        {
            if (!triggered)
            {
                var playerData = GUtils.IsRelevantLocalCar(other);
                if (playerData != null)
                {
                    AbilityBatteryChange[] abilityBatteryChanges = new AbilityBatteryChange[4];

                    if (enableBoosting)
                    {
                        playerData.EnableOrDisableBoost(true, true);
                        abilityBatteryChanges[0] = AbilityBatteryChange.Enable;

                        playerData.CarLogic_.Boost_.AbilityEnabled_ = true;
                    }
                    if (enableJumping)
                    {
                        playerData.EnableOrDisableJump(true, true);
                        abilityBatteryChanges[1] = AbilityBatteryChange.Enable;

                        playerData.CarLogic_.Jump_.AbilityEnabled_ = true;
                    }
                    if (enableFlying)
                    {
                        playerData.EnableOrDisableWings(true, true);
                        abilityBatteryChanges[2] = AbilityBatteryChange.Enable;

                        playerData.CarLogic_.Wings_.AbilityEnabled_ = true;
                    }
                    if (enableJetRotating)
                    {
                        playerData.EnableOrDisableJets(true, true);
                        abilityBatteryChanges[3] = AbilityBatteryChange.Enable;

                        playerData.CarLogic_.Jets_.AbilityEnabled_ = true;
                    }

                    if (showAbilityBattery)
                    {
                        var screen = playerData.CarScreenLogic_;
                        if (screen)
                        {
                            screen.EnableAbilityBattery(abilityBatteryChanges, "downloading");
                            AudioManager.PostEvent("Play_SystemActivate", playerData.Car_);
                        }
                    }

                    if (oneTimeTrigger)
                    {
                        triggered = true;
                    }
                }
            }
        }
    }
}
