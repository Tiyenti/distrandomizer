using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Centrifuge.Distance;
using Centrifuge.Distance.Game;
using UnityEngine;

namespace DistanceRando
{
    static class ApplyRandoChanges
    {
        internal static void OnPostLoad(RandoGame randoGame)
        {
            foreach (var obj in GameObject.FindObjectsOfType<AdventureSpecialIntro>())
            {
                GameObject.Destroy(obj.gameObject);
            }

            if (!(Game.LevelName == "Enemy" || Game.LevelName == "Credits"))
            {
                // remove warpanchor cutscenes but keep all warpanchors present in arcade mode
                // (this could allow for abyss to unlock an ability)
                foreach (var obj in MonoBehaviour.FindObjectsOfType<WarpAnchor>())
                {
                    if (obj.ignoreInArcade_)
                    {
                        obj.ignoreInAdventure_ = true;
                    }
                    else if (obj.ignoreInAdventure_)
                    {
                        obj.ignoreInAdventure_ = false;
                    }
                }

                foreach (var obj in GameObject.FindObjectsOfType<GlitchFieldLogic>())
                {
                    if (obj.ignoreInArcade_)
                    {
                        GameObject.Destroy(obj.gameObject);
                    }
                }
            }
            else if (Game.LevelName == "Enemy")
            {
                // Enemy specific changes

                // Shorten the warp to earth cutscene by grabbing the relevant warp anchor
                // (in this case, the one just before the waterfall area) and altering its properties a bit
                foreach (var obj in GameObject.FindObjectsOfType<WarpAnchor>())
                {
                    if (obj.myID_ == 0 && obj.otherID_ == 51)
                    {
                        obj.otherID_ = 53;
                        obj.slowmoSpeed_ = 1.0f;
                        obj.time_ = 0.2f;
                        obj.transitionEffect_ = WarpAnchor.TransitionEffect.Teleport;
                        obj.audioEventAfter_ = "Stop_DU_ending_sequence";
                    }
                }

                // Remove the SetAbilitiesTrigger that disables all your abilties at the end
                foreach (var obj in GameObject.FindObjectsOfType<SetAbilitiesTrigger>())
                {
                    if (obj.enableBoosting_ == false &&
                        obj.enableJumping_ == false &&
                        obj.enableFlying_ == false &&
                        obj.enableJetRotating_ == false)
                    {
                        obj.Destroy();
                    }
                }
            }
        }

        internal static void OnModeStarted(RandoGame randoGame)
        {
            foreach (var obj in GameObject.FindObjectsOfType<WingCorruptionZone>())
            {
                GameObject.Destroy(obj.gameObject);
            }

            // Set map subtitle
            var titleObj = GameObject.FindObjectOfType<LevelIntroTitleLogic>();

            int curMap = G.Sys.GameManager_.GetCurrentPlaylistIndex();

            if (titleObj)
            {
                titleObj.subtitleText_.text = $"-  MAP {curMap}/16  -";
            }
            else
            {
                MonoBehaviour.print("[RANDOMIZER] title obj null");
            }
        }

        internal static void OnGo(RandoGame randoGame)
        {
            if (Game.LevelName == "Enemy" || Game.LevelName == "Credits")
            {
                return;
            }

            RandoMap map = randoGame.maps[Game.LevelName];

            G.Sys.GameManager_.Level_.Settings_.disableBoosting_ = !map.boostEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableJumping_ = !map.jumpEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableFlying_ = !map.wingsEnabled;
            G.Sys.GameManager_.Level_.Settings_.disableJetRotating_ = !map.jetsEnabled;

            CarLogic car = G.Sys.PlayerManager_.Current_.playerData_.CarLogic_;
            /*car.Boost_.AbilityEnabled_ = map.boostEnabled;
            car.Jump_.AbilityEnabled_ = map.jumpEnabled;
            car.Wings_.AbilityEnabled_ = map.wingsEnabled;
            car.Jets_.AbilityEnabled_ = map.jetsEnabled;*/

            randoGame.abilityState.jumpShouldBeEnabled = map.jumpEnabled;
            randoGame.abilityState.wingsShouldBeEnabled = map.wingsEnabled;
            randoGame.abilityState.jetsShouldBeEnabled = map.jetsEnabled;
            //car.GetComponent<HornGadget>().enabled = true;

            foreach (var obj in UnityEngine.Object.FindObjectsOfType<InfoDisplayLogic>())
            {
                if (obj.gameObject.name == "InfoDisplayBox" || obj.gameObject.name == "InfoAndIndicatorDisplayBox")
                {
                    GameObject.Destroy(obj.gameObject);
                }
                else
                {
                    obj.gameObject.RemoveComponent<InfoDisplayLogic>();
                }
            }

            foreach (var obj in UnityEngine.Object.FindObjectsOfType<WingCorruptionZone>())
            {
                GameObject.Destroy(obj.gameObject);
            }

            foreach (var obj in UnityEngine.Object.FindObjectsOfType<AdventureAbilitySettings>())
            {
                GameObject.Destroy(obj.gameObject);
            }

            if (map.abilityEnabled != Ability.None)
            {
                Console.WriteLine($"enables {map.abilityEnabled.ToString()}");
                SetAbilitiesTrigger[] triggers = GameObject.FindObjectsOfType<SetAbilitiesTrigger>();

                foreach (var trigger in triggers)
                {
                    if (!(trigger.visualsOnly_ || !trigger.showAbilityAlert_))
                    {
                        // replace default triggers with a custom solution
                        // (this is an attempt to fix shockingly inconsistent behaviour with the default triggers)
                        // (also it lets us have progressive ability icons!! cool!!)
                        var newTrigger = trigger.gameObject.AddComponent<RandomizerAbilityTrigger>();

                        if (map.abilityEnabled == Ability.Jump)
                        {
                            newTrigger.enableJumping = true;
                            newTrigger.enableFlying = randoGame.abilityState.wingsShouldBeEnabled;
                            newTrigger.enableBoosting = true;
                            newTrigger.enableJetRotating = randoGame.abilityState.jetsShouldBeEnabled;
                        }
                        else if (map.abilityEnabled == Ability.Wings)
                        {
                            newTrigger.enableJumping = randoGame.abilityState.jumpShouldBeEnabled;
                            newTrigger.enableFlying = true;
                            newTrigger.enableBoosting = true;
                            newTrigger.enableJetRotating = randoGame.abilityState.jetsShouldBeEnabled;
                        }
                        else if (map.abilityEnabled == Ability.Jets)
                        {
                            newTrigger.enableJumping = randoGame.abilityState.jumpShouldBeEnabled;
                            newTrigger.enableFlying = randoGame.abilityState.wingsShouldBeEnabled;
                            newTrigger.enableBoosting = true;
                            newTrigger.enableJetRotating = true;
                        }
                        else if (map.abilityEnabled == Ability.Boost)
                        {
                            newTrigger.enableJumping = randoGame.abilityState.jumpShouldBeEnabled;
                            newTrigger.enableFlying = randoGame.abilityState.wingsShouldBeEnabled;
                            newTrigger.enableBoosting = true;
                            newTrigger.enableJetRotating = randoGame.abilityState.jetsShouldBeEnabled;
                        }

                        trigger.gameObject.RemoveComponent<SetAbilitiesTrigger>();
                    }
                    else
                    {
                        GameObject.Destroy(trigger.gameObject);
                    }
                }
            }
            else
            {
                SetAbilitiesTrigger[] triggers = GameObject.FindObjectsOfType<SetAbilitiesTrigger>();

                foreach (var trigger in triggers)
                {
                    GameObject.Destroy(trigger.gameObject);
                }
            }
        }
    }
}
