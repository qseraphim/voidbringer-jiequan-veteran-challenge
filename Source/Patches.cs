using System;
using System.Collections.Generic;
using HarmonyLib;
using I2.Loc;
using InControl.NativeDeviceProfiles;
using NineSolsAPI;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Collections.IEnumerator;
using static TormentedJiequan.PATH_LIST;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TormentedJiequan;

    [HarmonyPatch]
    public class Patches {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BossGeneralState), nameof(BossGeneralState.OnAnimationMove))]
        private static void ChasingAttackEnhancer(ref BossGeneralState __instance, ref Vector3 delta) {
            if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME) {
                if (__instance.GetStateType() == MonsterBase.States.AttackParrying1) 
                    delta.x *= 1.05f;                                           
                else delta.x *= 1.2f;
            }
        }

        [HarmonyPrefix] //THANK YOU GREG YOU JUST SAVED ME LIKE 20 HOURS OF SUFFERING
        [HarmonyPatch(typeof(MonsterBase), "CheckInit")]
        private static void OnMonsterInit(ref MonsterBase __instance) {
            if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME) {
                ApplyNewState(ref __instance);
            }
        }
        
        private static void ApplyNewState(ref MonsterBase __instance) {
            var states = __instance.transform.Find("States");
            var changePhase = states.GetComponentInChildren<BossPhaseChangeState>();
            var changePhase3 = Object.Instantiate(changePhase, states);
        
            changePhase3.exitState = MonsterBase.States.Engaging;
            var scriptableThing =
                changePhase3.stateTypeScriptable = ScriptableObject.CreateInstance<MonsterStateScriptable>();
            scriptableThing.overrideStateType = MonsterBase.States.Trolling;
            scriptableThing.stateName = changePhase.BindingAnimation;

            __instance.postureSystem.DieHandleingStates = new() {
                MonsterBase.States.BossAngry,
                MonsterBase.States.BossAngry,
                MonsterBase.States.LastHit,
                MonsterBase.States.Dead,
            };
            __instance.postureSystem.GenerateCurrentDieHandleStacks();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BossGeneralState), nameof(BossGeneralState.OnStateEnter))]
        private static void Check(ref BossGeneralState __instance) {

            var i = Random.Range(0, 99);
            if (i < 50) {
                if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.Attack5) {
                    TormentedJiequan.Instance.StartCoroutine(TormentedJiequan.StateInterruptCoroutine(0.75f, MonsterManager.Instance.ClosetMonster, MonsterBase.States.Attack5, MonsterBase.States.Attack2));
                } //crimson smash into quick dagger fakeout (50% of base fakeout prob so 25% total)
            } else if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.Attack5) {
                TormentedJiequan.Instance.StartCoroutine(TormentedJiequan.StateInterruptCoroutine(0.4f, MonsterManager.Instance.ClosetMonster, MonsterBase.States.Attack5, MonsterBase.States.Attack9));
            } //crimson smash into teleport fakeout (same story, 25%)
            //alternating
            
            if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.AttackParrying1) {
                TormentedJiequan.Instance.StartCoroutine(TormentedJiequan.StateInterruptCoroutine(0.35f, MonsterManager.Instance.ClosetMonster, MonsterBase.States.AttackParrying1, MonsterBase.States.Attack9));
            }
            
            if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.AttackParrying2) {
                TormentedJiequan.Instance.StartCoroutine(TormentedJiequan.StateInterruptCoroutine(0.35f, MonsterManager.Instance.ClosetMonster, MonsterBase.States.AttackParrying2, MonsterBase.States.Attack9));
            }
            
            if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.AttackParrying3) {
                TormentedJiequan.Instance.StartCoroutine(TormentedJiequan.StateInterruptCoroutine(0.35f, MonsterManager.Instance.ClosetMonster, MonsterBase.States.AttackParrying3, MonsterBase.States.Attack9));
            }
            
            if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.AttackParrying4) {
                TormentedJiequan.Instance.StartCoroutine(TormentedJiequan.StateInterruptCoroutine(0.35f, MonsterManager.Instance.ClosetMonster, MonsterBase.States.AttackParrying4, MonsterBase.States.Attack9));
            }
            
            if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.AttackParrying5) {
                TormentedJiequan.Instance.StartCoroutine(TormentedJiequan.StateInterruptCoroutine(0.35f, MonsterManager.Instance.ClosetMonster, MonsterBase.States.AttackParrying5, MonsterBase.States.Attack9));
            }//quick stagger into teleport

            if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.Attack5) {
                
            }
            
            //if (SceneManager.GetActiveScene().name == PATH_LIST.BOSS_ROOM.SCENE_NAME && __instance.GetStateType() == MonsterBase.States.Attack12) {
                //TormentedJiequan.Instance.StartCoroutine(TormentedJiequan.StateInterruptCoroutine(1.0f, MonsterManager.Instance.ClosetMonster, MonsterBase.States.Attack12, MonsterBase.States.Attack9));
            //} //crimson thrust repeat
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DamageDealer), nameof(DamageDealer.DamageAmount), MethodType.Getter)]
        private static void DamageBuff(ref DamageDealer __instance, ref float __result) {
            if (SceneManager.GetActiveScene().name != BOSS_ROOM.SCENE_NAME) return;
            float unbuffedDamage = __result;

            float multiplierUniversal = DAMAGE_MULTIPLIER.UNIVERSAL;
            float multiplierCrimsonSmash = DAMAGE_MULTIPLIER.ONESHOT;

            if (MonsterManager.Instance.ClosetMonster.CurrentState == MonsterBase.States.Attack5) {
                __result *= multiplierCrimsonSmash;
            } else {
                __result *= multiplierUniversal;
            }
        }
    

    [HarmonyPostfix]
        [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
        private static void TormentedNameChange(string Term, ref string __result) {
            if (Term != "Characters/NameTag_JieChuan") return;
            __result = $"{__result}, Demon of The Abyss";
    }
        
        
}