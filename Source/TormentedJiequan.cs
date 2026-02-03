using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using static TormentedJiequan.PATH_LIST;
using Random = UnityEngine.Random;

namespace TormentedJiequan;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class TormentedJiequan : BaseUnityPlugin {

    private Harmony _harmony = null!;

    private ConfigEntry<bool> _enableDarkness = null!;
    private ConfigEntry<bool> _enableJiequanLight = null!;
    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);
        _harmony = Harmony.CreateAndPatchAll(typeof(TormentedJiequan).Assembly);
        ToastManager.Toast($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!"); //toaster
        Log.Info("Mod is loaded!");
        SceneManager.sceneLoaded += OnSceneLoaded;

        _enableDarkness = Config.Bind("Options", "Lights Out!", false, "(REQUIRES RELOADING THE SCENE TO WORK! JUST PRESS <<Retry>> OR LEAVE AND REJOIN JIEQUAN'S FIGHT) Turns off the lights in Jiequan's room. Not recommended for first-time clears.");
        _enableJiequanLight = Config.Bind("Options", "Jiequan Tracking Light", false, "(REQUIRES RELOADING THE SCENE TO WORK! JUST PRESS <<Retry>> OR LEAVE AND REJOIN JIEQUAN'S FIGHT) Makes a light follow Jiequan around. Highly recommended when used with <<Lights Out!>>");
        Instance = this;
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private static void HealthChanger() {
        var monster = MonsterManager.Instance.ClosetMonster;
        var baseHealth = AccessTools.FieldRefAccess<MonsterStat, float>("BaseHealthValue");
        baseHealth(monster.monsterStat) = 6000f;          
        monster.monsterStat.Phase2HealthRatio = 1 + 0.0666667f;
        monster.monsterStat.Phase3HealthRatio = 1 + 0.1333333f;
        monster.monsterStat.BossMemoryHealthScale = 3f;
        monster.postureSystem.CurrentHealthValue = 9000f;
    }
    
    private static IEnumerator WaitForBossAndInit() {
        while (!MonsterManager.Instance || !MonsterManager.Instance.ClosetMonster) {
            yield return null;
        }
        HealthChanger();
    }

    public static IEnumerator StateInterruptCoroutine(float duration, MonsterBase monster, MonsterBase.States cancelableState, MonsterBase.States newState) {
        while (monster.CurrentState != cancelableState) yield return null;
        float timeElapsed = 0f;
        while (timeElapsed <= duration) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (monster.CurrentState == MonsterBase.States.BossAngry || monster.CurrentState == MonsterBase.States.Trolling ||
            monster.CurrentState == MonsterBase.States.LastHit || monster.CurrentState == MonsterBase.States.Dead) yield break;  //bug fix so phase transitions don't happen instantly (v1.0.1)
        
        int random = Random.Range(0, 99); 
        if (random < 50) {
            monster.ChangeState(
                monster.GetState(newState)); //50/50
        }
    }

    public static TormentedJiequan? Instance {
        get; 
        private set;
    }
        
    private _2dxFX_ColorChange _tj = null!;

    private _2dxFX_ColorChange _weaponA = null!;
    private _2dxFX_ColorChange _weaponB = null!;
    private _2dxFX_ColorChange _weaponC = null!;
    private _2dxFX_ColorChange _weaponD = null!;
    private _2dxFX_ColorChange _teleportSmoke = null!;
    private _2dxFX_ColorChange _knifething0 = null!;
    private _2dxFX_ColorChange _knifething1 = null!;
    private _2dxFX_ColorChange _knifething2 = null!;
    private void ColorChanger() {
        var color = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH + "MonsterCore/Animator(Proxy)/Animator/Boss_JieChuan"); 
        _tj = color.AddComponent<_2dxFX_ColorChange>();
        _tj._Saturation = 0f;
        _tj._ValueBrightness = 0.2f;

        var weaponAcolor = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH +
                                           "MonsterCore/Animator(Proxy)/Animator/Boss_JieChuan/Weapons/Weapon_A/Weapon");
        var weaponBcolor = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH +
                                           "MonsterCore/Animator(Proxy)/Animator/Boss_JieChuan/Weapons/Weapon_B/Weapon");
        var weaponCcolor = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH +
                                           "MonsterCore/Animator(Proxy)/Animator/Boss_JieChuan/Weapons/Weapon_C/Weapon");
        var weaponDcolor = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH +
                            "MonsterCore/Animator(Proxy)/Animator/Boss_JieChuan/Weapons/Weapon_D/Weapon");
        var smokeColor = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH +
                                         "MonsterCore/Animator(Proxy)/Animator/Boss_JieChuan/Effect/TeleportSmoke/Effect_TeleportSmoke");
        var knifeColor0 = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH +
                                         "MonsterCore/Animator(Proxy)/Animator/LogicRoot/Circle Shooter/Flying Weapon Path/View/Bullet/BulletSprite");
        var knifeColor1 = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH +
                                          "MonsterCore/Animator(Proxy)/Animator/LogicRoot/Circle Shooter/Flying Weapon Path (1)/View/Bullet/BulletSprite");
        var knifeColor2 = GameObject.Find(STEALTH_GAME_MONSTER.BOSS_PATH +
                                          "MonsterCore/Animator(Proxy)/Animator/LogicRoot/Circle Shooter/Flying Weapon Path (2)/View/Bullet/BulletSprite");
        
        
        _weaponA = weaponAcolor.AddComponent<_2dxFX_ColorChange>();
        _weaponB = weaponBcolor.AddComponent<_2dxFX_ColorChange>();
        _weaponC = weaponCcolor.AddComponent<_2dxFX_ColorChange>();
        _weaponD = weaponDcolor.AddComponent<_2dxFX_ColorChange>();
        _teleportSmoke = smokeColor.AddComponent<_2dxFX_ColorChange>();
        _knifething0 = knifeColor0.AddComponent<_2dxFX_ColorChange>();
        _knifething1 = knifeColor1.AddComponent<_2dxFX_ColorChange>();
        _knifething2 = knifeColor2.AddComponent<_2dxFX_ColorChange>();
        
        _weaponA._Saturation = 0f;
        _weaponB._Saturation = 0f;
        _weaponC._Saturation = 0f;
        _weaponD._Saturation = 0f;
        _teleportSmoke._Saturation = 0f;
        _knifething0._Saturation = 0f;
        _knifething1._Saturation = 0f;
        _knifething2._Saturation = 0f;
        
        _weaponA._ValueBrightness = 0.95f;
        _weaponB._ValueBrightness = 0.95f;
        _weaponC._ValueBrightness = 0.95f;
        _weaponD._ValueBrightness = 0.95f;
        _teleportSmoke._ValueBrightness = 0.2f;
        _knifething0._ValueBrightness = 0.95f;
        _knifething1._ValueBrightness = 0.95f;
        _knifething2._ValueBrightness = 0.95f;

        //notes:
        //Crimson visual spark - A5_S5/Room/EventBinder/General Boss Fight FSM Object_結權/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_JieChuan/MonsterCore/Animator(Proxy)/Animator/LogicRoot/Circle Shooter/Flying Weapon Path/PathShootCore_Attack1/Docker/DangerHintEffect (1)

    }
    
    
    private void LightingHandler() {
        var lastStandJadeTint = GameObject.Find("A5_S5/CameraCore/DockObj/OffsetObj/ShakeObj/SceneCamera/AmplifyLightingSystem/EffectCamera(請開著，Runtime會撈到SceneCamera做)");
        lastStandJadeTint.SetActive(false);
    }

    private LinkNextMoveStateWeight phase2DoubleSlashWeight = null!;
    private LinkNextMoveStateWeight phase2DaggersWeight = null!;
    private LinkNextMoveStateWeight phase2DiagonalWeight = null!;
    private LinkNextMoveStateWeight phase2PlungeWeight = null!;
    private LinkNextMoveStateWeight phase2StabWeight = null!;
    private LinkNextMoveStateWeight phase2GrenadesWeight = null!;
    private LinkNextMoveStateWeight phase2TeleportWeight = null!;
    private LinkNextMoveStateWeight phase2HorizontalWeight = null!;
    private LinkNextMoveStateWeight phase2CrimsonThrustWeight = null!;
    
    private void Phase3Enabler(){
        var doubleSlash = GameObject.Find(JIEQUAN_ATTACK.DOUBLE_SLASH);
        phase2DoubleSlashWeight = GameObject.Find(JIEQUAN_ATTACK.DOUBLE_SLASH + "/phase2")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3DoubleSlashWeight = Instantiate(phase2DoubleSlashWeight, doubleSlash.transform);
        phase3DoubleSlashWeight.name = "phase3";

        var daggers = GameObject.Find(JIEQUAN_ATTACK.DAGGERS);
        phase2DaggersWeight = GameObject.Find(JIEQUAN_ATTACK.DAGGERS + "/phase2")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3DaggersWeight = Instantiate(phase2DaggersWeight, daggers.transform);
        phase3DaggersWeight.name = "phase3";

        var diagonal = GameObject.Find(JIEQUAN_ATTACK.DIAGONAL);
        phase2DiagonalWeight = GameObject.Find(JIEQUAN_ATTACK.DIAGONAL + "/phase2")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3DiagonalWeight = Instantiate(phase2DiagonalWeight, diagonal.transform);
        phase3DiagonalWeight.name = "phase3";

        var plunge = GameObject.Find(JIEQUAN_ATTACK.PLUNGE);
        phase2PlungeWeight = GameObject.Find(JIEQUAN_ATTACK.PLUNGE + "/phase2")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3PlungeWeight = Instantiate(phase2PlungeWeight, plunge.transform);
        phase3PlungeWeight.name = "phase3";

        var stab = GameObject.Find(JIEQUAN_ATTACK.STAB);
        phase2StabWeight = GameObject.Find(JIEQUAN_ATTACK.STAB + "/phase2")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3StabWeight = Instantiate(phase2StabWeight, stab.transform);
        phase3StabWeight.name = "phase3";

        var grenades = GameObject.Find(JIEQUAN_ATTACK.GRENADES);
        phase2GrenadesWeight = GameObject.Find(JIEQUAN_ATTACK.STAB + "/phase2")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3GrenadesWeight = Instantiate(phase2GrenadesWeight, grenades.transform);
        phase3GrenadesWeight.name = "phase3";

        var teleport = GameObject.Find(JIEQUAN_ATTACK.TELEPORT);
        phase2TeleportWeight = GameObject.Find(JIEQUAN_ATTACK.TELEPORT + "/phase2")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3TeleportWeight = Instantiate(phase2TeleportWeight, teleport.transform);
        phase3TeleportWeight.name = "phase3";

        var horizontal = GameObject.Find(JIEQUAN_ATTACK.LINK_REVERSE_STAB);
        phase2HorizontalWeight = GameObject.Find(JIEQUAN_ATTACK.LINK_REVERSE_STAB + "/phase2")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3HorizontalWeight = Instantiate(phase2HorizontalWeight, horizontal.transform);
        phase3HorizontalWeight.name = "phase3";

        var crimsonThrust = GameObject.Find(JIEQUAN_ATTACK.CRIMSON_THRUST);
        phase2CrimsonThrustWeight = GameObject.Find(JIEQUAN_ATTACK.CRIMSON_THRUST + "/weight")
            .GetComponent<LinkNextMoveStateWeight>();
        var phase3CrimsonThrustWeight = Instantiate(phase2CrimsonThrustWeight, crimsonThrust.transform);
        phase3CrimsonThrustWeight.name = "phase3";
    }
    
    private static void JiDecoration() {
        var ji = GameObject.Find(BOSS_ROOM.ROOM_NAME + "DressingSpace/Jee_Chill");
        ji.SetActive(true);

        var jiEyeDarkness = GameObject.Find(BOSS_ROOM.ROOM_NAME + "DressingSpace/Jee_Chill/Jee/Track_Eye");
        jiEyeDarkness.SetActive(false);
    }

    private static void StaggerRemover() {
        var fullControlStagger = GameObject.Find(JIEQUAN_STATE.STAGGER);
        fullControlStagger.SetActive(false);
        var shieldBreakStagger = GameObject.Find(JIEQUAN_STATE.SHIELD_BREAK_STAGGER);
        shieldBreakStagger.SetActive(false);
        //var unboundCounterStagger = GameObject.Find(JIEQUAN_STATE.UC_STAGGER);
        //unboundCounterStagger.SetActive(false);           //removed as balance change (v1.0.1)
    }

    private static void AttackSpeedChanger() {

        var unboundCounterStaggerSpeed = GameObject.Find(JIEQUAN_STATE.UC_STAGGER)
            .GetComponent<BossGeneralState>();
        if (unboundCounterStaggerSpeed != null) {
            unboundCounterStaggerSpeed.AnimationSpeed = SPEED_MULTIPLIER.STUN_STATE_SPEED;
            unboundCounterStaggerSpeed.OverideAnimationSpeed = true;
        }

        var doubleSlashSpeed = (GameObject.Find(JIEQUAN_ATTACK.DOUBLE_SLASH))
            .GetComponent<BossGeneralState>();
        doubleSlashSpeed.AnimationSpeed = SPEED_MULTIPLIER.DOUBLE_SLASH_SPEED;
        doubleSlashSpeed.OverideAnimationSpeed = true;

        var daggersSpeed = (GameObject.Find(JIEQUAN_ATTACK.DAGGERS))
            .GetComponent<BossGeneralState>();
        daggersSpeed.AnimationSpeed = SPEED_MULTIPLIER.DAGGERS_SPEED;
        daggersSpeed.OverideAnimationSpeed = true;

        var diagonalSpeed = (GameObject.Find(JIEQUAN_ATTACK.DIAGONAL))
            .GetComponent<BossGeneralState>();
        diagonalSpeed.AnimationSpeed = SPEED_MULTIPLIER.DIAGONAL_SPEED;
        diagonalSpeed.OverideAnimationSpeed = true;

        var plungeSpeed = (GameObject.Find(JIEQUAN_ATTACK.PLUNGE))
            .GetComponent<BossGeneralState>();
        plungeSpeed.AnimationSpeed = SPEED_MULTIPLIER.PLUNGE_SPEED;
        plungeSpeed.OverideAnimationSpeed = true;

        var crimsonSmashSpeed = (GameObject.Find(JIEQUAN_ATTACK.CRIMSON_SMASH))
            .GetComponent<BossGeneralState>();
        crimsonSmashSpeed.AnimationSpeed = SPEED_MULTIPLIER.CRIMSON_SMASH_SPEED;
        crimsonSmashSpeed.OverideAnimationSpeed = true;

        var stabSpeed = (GameObject.Find(JIEQUAN_ATTACK.STAB))
            .GetComponent<BossGeneralState>();
        stabSpeed.AnimationSpeed = SPEED_MULTIPLIER.STAB_SPEED;
        stabSpeed.OverideAnimationSpeed = true;

        var grenadesSpeed = (GameObject.Find(JIEQUAN_ATTACK.GRENADES))
            .GetComponent<BossGeneralState>();
        grenadesSpeed.AnimationSpeed = SPEED_MULTIPLIER.GRENADES_SPEED;
        grenadesSpeed.OverideAnimationSpeed = true;

        var shieldSpeed = (GameObject.Find(JIEQUAN_ATTACK.SHIELD))
            .GetComponent<BossGeneralState>();
        shieldSpeed.AnimationSpeed = SPEED_MULTIPLIER.SHIELD_SPEED;
        shieldSpeed.OverideAnimationSpeed = true;

        var teleportSpeed = (GameObject.Find(JIEQUAN_ATTACK.TELEPORT))
            .GetComponent<BossGeneralState>();
        teleportSpeed.AnimationSpeed = SPEED_MULTIPLIER.TELEPORT_SPEED;
        teleportSpeed.OverideAnimationSpeed = true;

        var horizontalSpeed = (GameObject.Find(JIEQUAN_ATTACK.HORIZONTAL_THRUST))
            .GetComponent<BossGeneralState>();
        horizontalSpeed.AnimationSpeed = SPEED_MULTIPLIER.HORIZONTAL_SPEED;
        horizontalSpeed.OverideAnimationSpeed = true;

        var linkRevSpeed = (GameObject.Find(JIEQUAN_ATTACK.LINK_REVERSE_STAB))
            .GetComponent<BossGeneralState>();
        linkRevSpeed.AnimationSpeed = SPEED_MULTIPLIER.HORIZONTAL_SPEED;
        linkRevSpeed.OverideAnimationSpeed = true;

        var crimsonThrustSpeed = (GameObject.Find(JIEQUAN_ATTACK.CRIMSON_THRUST))
            .GetComponent<BossGeneralState>();
        crimsonThrustSpeed.AnimationSpeed = SPEED_MULTIPLIER.CRIMSON_THRUST_SPEED;
        crimsonThrustSpeed.OverideAnimationSpeed = true;
    }

    private static void WeightChanger() {
        //attacks
        AttackWeight doubleSlash = new AttackWeight();
        doubleSlash.state = GameObject.Find(JIEQUAN_ATTACK.DOUBLE_SLASH).GetComponent<BossGeneralState>();
        doubleSlash.weight = 1;

        AttackWeight daggers = new AttackWeight();
        daggers.state = GameObject.Find(JIEQUAN_ATTACK.DAGGERS).GetComponent<BossGeneralState>();
        daggers.weight = 1;

        AttackWeight diagonal = new AttackWeight();
        diagonal.state = GameObject.Find(JIEQUAN_ATTACK.DIAGONAL).GetComponent<BossGeneralState>();
        diagonal.weight = 1;

        AttackWeight plunge = new AttackWeight();
        plunge.state = GameObject.Find(JIEQUAN_ATTACK.PLUNGE).GetComponent<BossGeneralState>();
        plunge.weight = 1;

        AttackWeight crimsonSmash = new AttackWeight();
        crimsonSmash.state = GameObject.Find(JIEQUAN_ATTACK.CRIMSON_SMASH).GetComponent<BossGeneralState>();
        crimsonSmash.weight = 1;

        AttackWeight stab = new AttackWeight();
        stab.state = GameObject.Find(JIEQUAN_ATTACK.STAB).GetComponent<BossGeneralState>();
        stab.weight = 1;
        
        AttackWeight grenades = new AttackWeight();
        grenades.state = GameObject.Find(JIEQUAN_ATTACK.GRENADES).GetComponent<BossGeneralState>();
        grenades.weight = 1;
        
        AttackWeight shield = new AttackWeight();
        shield.state = GameObject.Find(JIEQUAN_ATTACK.SHIELD).GetComponent<BossGeneralState>();
        shield.weight = 1;

        AttackWeight teleport = new AttackWeight();
        teleport.state = GameObject.Find(JIEQUAN_ATTACK.TELEPORT).GetComponent<BossGeneralState>();
        teleport.weight = 1;

        AttackWeight horizontal = new AttackWeight();
        horizontal.state = GameObject.Find(JIEQUAN_ATTACK.LINK_REVERSE_STAB).GetComponent<BossGeneralState>();
        horizontal.weight = 1;

        AttackWeight crimsonThrust = new AttackWeight();
        crimsonThrust.state = GameObject.Find(JIEQUAN_ATTACK.CRIMSON_THRUST).GetComponent<BossGeneralState>();
        crimsonThrust.weight = 1;
        
        //lists and possible attack transitions, phase 1
            var doubleSlashP1 = GameObject.Find(JIEQUAN_ATTACK.DOUBLE_SLASH + "/phase1").GetComponent<LinkNextMoveStateWeight>();
            doubleSlashP1.stateWeightList.Clear();
            doubleSlashP1.mustUseStates.Clear();
            doubleSlashP1.stateWeightList.Add(crimsonSmash);
            doubleSlashP1.stateWeightList.Add(teleport);
            doubleSlashP1.stateWeightList.Add(doubleSlash);
            doubleSlashP1.stateWeightList.Add(stab);
            doubleSlashP1.stateWeightList.Add(horizontal);
        
            var stabP1 = GameObject.Find(JIEQUAN_ATTACK.STAB + "/phase1").GetComponent<LinkNextMoveStateWeight>();
            stabP1.stateWeightList.Clear();
            stabP1.stateWeightList.Add(doubleSlash);
            stabP1.stateWeightList.Add(teleport);
            stabP1.stateWeightList.Add(plunge);

            var crimsonThrustP1 = GameObject.Find(JIEQUAN_ATTACK.CRIMSON_THRUST + "/weight").GetComponent<LinkNextMoveStateWeight>();
            crimsonThrustP1.mustUseStates.Clear();
            crimsonThrustP1.stateWeightList.Clear();
            crimsonThrustP1.stateWeightList.Add(plunge);
            crimsonThrustP1.stateWeightList.Add(teleport);
            crimsonThrustP1.stateWeightList.Add(crimsonThrust);
            
            var teleportP1 = GameObject.Find(JIEQUAN_ATTACK.TELEPORT + "/phase1").GetComponent<LinkNextMoveStateWeight>();
            teleportP1.stateWeightList.Add(stab);
            teleportP1.stateWeightList.Add(doubleSlash);
            
            var plungeP1 = GameObject.Find(JIEQUAN_ATTACK.PLUNGE + "/phase1").GetComponent<LinkNextMoveStateWeight>();
            plungeP1.stateWeightList.Clear();
            plungeP1.stateWeightList.Add(diagonal);
            plungeP1.stateWeightList.Add(crimsonSmash);
            
            var diagonalP1 = GameObject.Find(JIEQUAN_ATTACK.DIAGONAL + "/phase1").GetComponent<LinkNextMoveStateWeight>();
            diagonalP1.stateWeightList.Clear();
            diagonalP1.stateWeightList.Add(crimsonThrust);
            
            var horizontalP1 = GameObject.Find(JIEQUAN_ATTACK.LINK_REVERSE_STAB + "/phase1").GetComponent<LinkNextMoveStateWeight>();
            horizontalP1.stateWeightList.Clear();
            horizontalP1.stateWeightList.Add(shield);
            horizontalP1.stateWeightList.Add(doubleSlash);
            horizontalP1.stateWeightList.Add(grenades);
            
            var grenadesP1 = GameObject.Find(JIEQUAN_ATTACK.GRENADES + "/phase1").GetComponent<LinkNextMoveStateWeight>();
            grenadesP1.stateWeightList.Clear();
            grenadesP1.stateWeightList.Add(shield);
            
            var daggersP1 = GameObject.Find(JIEQUAN_ATTACK.DAGGERS + "/phase1 ").GetComponent<LinkNextMoveStateWeight>();
            daggersP1.stateWeightList.Clear();
            daggersP1.stateWeightList.Add(shield);

            //Crimson Smash and Shield are finisher attacks and do not link with LinkNextMoveStateWeight
            
            //phase 2
            var doubleSlashP2 = GameObject.Find(JIEQUAN_ATTACK.DOUBLE_SLASH + "/phase2").GetComponent<LinkNextMoveStateWeight>();
            doubleSlashP2.stateWeightList.Clear();
            doubleSlashP2.mustUseStates.Clear();
            doubleSlashP2.stateWeightList.Add(crimsonThrust);
            
            var stabP2 = GameObject.Find(JIEQUAN_ATTACK.STAB + "/phase2").GetComponent<LinkNextMoveStateWeight>();
            stabP2.stateWeightList.Clear();
            stabP2.mustUseStates.Clear();
            stabP2.stateWeightList.Add(shield);
            stabP2.stateWeightList.Add(diagonal);
            stabP2.stateWeightList.Add(crimsonThrust);
            stabP2.stateWeightList.Add(teleport);
            
            //crimson thrust's LinkNextMoveStateWeight is used for both phases
            var teleportP2 = GameObject.Find(JIEQUAN_ATTACK.TELEPORT + "/phase2").GetComponent<LinkNextMoveStateWeight>();
            teleportP2.stateWeightList.Clear();
            teleportP2.mustUseStates.Clear();
            teleportP2.stateWeightList.Add(teleport);
            teleportP2.stateWeightList.Add(crimsonSmash);
            teleportP2.stateWeightList.Add(crimsonThrust);
            teleportP2.stateWeightList.Add(grenades);
            teleportP2.stateWeightList.Add(plunge);
            
            var plungeP2 = GameObject.Find(JIEQUAN_ATTACK.PLUNGE + "/phase2").GetComponent<LinkNextMoveStateWeight>();
            plungeP2.stateWeightList.Clear();
            plungeP2.mustUseStates.Clear();
            plungeP2.stateWeightList.Add(teleport);
            plungeP2.stateWeightList.Add(doubleSlash);
            plungeP2.stateWeightList.Add(stab);
            plungeP2.stateWeightList.Add(horizontal);
            
            var diagonalP2 = GameObject.Find(JIEQUAN_ATTACK.DIAGONAL + "/phase2").GetComponent<LinkNextMoveStateWeight>();
            diagonalP2.stateWeightList.Clear();
            diagonalP2.mustUseStates.Clear();
            diagonalP2.stateWeightList.Add(plunge);
            diagonalP2.stateWeightList.Add(doubleSlash);
            diagonalP2.stateWeightList.Add(diagonal);
            diagonalP2.stateWeightList.Add(teleport);
            
            var horizontalP2 = GameObject.Find(JIEQUAN_ATTACK.LINK_REVERSE_STAB + "/phase2").GetComponent<LinkNextMoveStateWeight>();
            horizontalP2.stateWeightList.Clear();
            horizontalP2.mustUseStates.Clear();
            horizontalP2.stateWeightList.Add(crimsonThrust);
            horizontalP2.stateWeightList.Add(daggers);
            
            var grenadesP2 = GameObject.Find(JIEQUAN_ATTACK.GRENADES + "/phase2").GetComponent<LinkNextMoveStateWeight>();
            grenadesP2.stateWeightList.Clear();
            grenadesP2.mustUseStates.Clear();
            grenadesP2.stateWeightList.Add(teleport);
            grenadesP2.stateWeightList.Add(shield);
            grenadesP2.stateWeightList.Add(crimsonThrust);
            
            
            var daggersP2 = GameObject.Find(JIEQUAN_ATTACK.DAGGERS + "/phase2").GetComponent<LinkNextMoveStateWeight>();
            daggersP2.stateWeightList.Clear();
            daggersP2.mustUseStates.Clear();
            daggersP2.stateWeightList.Add(teleport);
            
            //phase 3
            var doubleSlashP3 = GameObject.Find(JIEQUAN_ATTACK.DOUBLE_SLASH + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            doubleSlashP3.stateWeightList.Clear();
            doubleSlashP3.mustUseStates.Clear();
            doubleSlashP3.stateWeightList.Add(doubleSlash);
            doubleSlashP3.stateWeightList.Add(crimsonThrust);
            doubleSlashP3.stateWeightList.Add(crimsonSmash);
            doubleSlashP3.stateWeightList.Add(diagonal);
            doubleSlashP3.stateWeightList.Add(plunge);
            doubleSlashP3.stateWeightList.Add(teleport);
            doubleSlashP3.stateWeightList.Add(horizontal);
        
            var stabP3 = GameObject.Find(JIEQUAN_ATTACK.STAB + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            stabP3.stateWeightList.Clear();
            stabP3.mustUseStates.Clear();
            stabP3.stateWeightList.Add(diagonal);
            stabP3.stateWeightList.Add(plunge);
            stabP3.stateWeightList.Add(crimsonSmash);
            stabP3.stateWeightList.Add(shield);
            stabP3.stateWeightList.Add(daggers);
            
            var plungeP3 = GameObject.Find(JIEQUAN_ATTACK.PLUNGE + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            plungeP3.stateWeightList.Clear();
            plungeP3.mustUseStates.Clear();
            plungeP3.stateWeightList.Add(teleport);
            plungeP3.stateWeightList.Add(diagonal);
            
            var teleportP3 = GameObject.Find(JIEQUAN_ATTACK.TELEPORT + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            teleportP3.stateWeightList.Add(teleport);
            teleportP3.stateWeightList.Add(doubleSlash);
            teleportP3.stateWeightList.Add(crimsonSmash);
            teleportP3.stateWeightList.Add(crimsonThrust);
            teleportP3.stateWeightList.Add(grenades);
            teleportP3.stateWeightList.Add(daggers);
            teleportP3.stateWeightList.Add(stab);
            
            var diagonalP3 = GameObject.Find(JIEQUAN_ATTACK.DIAGONAL + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            diagonalP3.stateWeightList.Clear();
            diagonalP3.mustUseStates.Clear();
            diagonalP3.stateWeightList.Add(crimsonSmash);
            diagonalP3.stateWeightList.Add(crimsonThrust);
 
            
            var horizontalP3 = GameObject.Find(JIEQUAN_ATTACK.LINK_REVERSE_STAB + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            horizontalP3.stateWeightList.Clear();
            horizontalP3.mustUseStates.Clear();
            horizontalP3.stateWeightList.Add(horizontal);
            horizontalP3.stateWeightList.Add(crimsonThrust);
            horizontalP3.stateWeightList.Add(daggers);
            horizontalP3.stateWeightList.Add(shield);
            horizontalP3.stateWeightList.Add(crimsonSmash);
            
            var grenadesP3 = GameObject.Find(JIEQUAN_ATTACK.GRENADES + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            grenadesP3.stateWeightList.Clear();
            grenadesP3.stateWeightList.Add(crimsonThrust);
            grenadesP3.stateWeightList.Add(crimsonSmash);
            grenadesP3.stateWeightList.Add(shield);
            
            var daggersP3 = GameObject.Find(JIEQUAN_ATTACK.DAGGERS + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            daggersP3.stateWeightList.Clear();
            daggersP3.mustUseStates.Clear();
            daggersP3.stateWeightList.Add(doubleSlash);
            daggersP3.stateWeightList.Add(crimsonThrust);
            daggersP3.stateWeightList.Add(crimsonSmash);
            daggersP3.stateWeightList.Add(shield);
            daggersP3.stateWeightList.Add(diagonal);

            var crimsonThrustP3 = GameObject.Find(JIEQUAN_ATTACK.CRIMSON_THRUST + "/phase3").GetComponent<LinkNextMoveStateWeight>();
            crimsonThrustP3.stateWeightList.Add(crimsonThrust);
            crimsonThrustP3.stateWeightList.Add(crimsonSmash);
            crimsonThrustP3.stateWeightList.Add(diagonal);
            crimsonThrustP3.stateWeightList.Add(plunge);
    }
    
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == BOSS_ROOM.SCENE_NAME)
        {
            StartCoroutine(WaitForBossAndInit());
            
            ColorChanger();
            JiDecoration();
            LightingHandler();
            StaggerRemover();
            AttackSpeedChanger();
            Phase3Enabler();
            WeightChanger();
            //ToastManager.Toast("Success");
        }
    }

    private void OnDestroy() {
        _harmony.UnpatchSelf();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}
