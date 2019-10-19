﻿using MonoMod.Cil;
using System;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace RoR2Cheats
{
    public class Hooks
    {
        private const ConVarFlags AllFlagsNoCheat = ConVarFlags.None | ConVarFlags.Archive | ConVarFlags.Engine | ConVarFlags.ExecuteOnServer | ConVarFlags.SenderMustBeServer;
        public static void InitializeHooks()
        {
            IL.RoR2.Console.Awake += UnlockConsole;
            On.RoR2.Console.InitConVars += InitCommandsAndFreeConvars;

            On.RoR2.PreGameController.Awake += SeedHook;

            On.RoR2.CombatDirector.SetNextSpawnAsBoss += CombatDirector_SetNextSpawnAsBoss;

            //IL.RoR2.Networking.GameNetworkManager.FixedUpdateServer += GameNetworkManager_FixedUpdateServer;
            //IL.RoR2.Networking.GameNetworkManager.cctor += GameNetworkManager_cctor;
        }

        private static void InitCommandsAndFreeConvars(On.RoR2.Console.orig_InitConVars orig, RoR2.Console self)
        {
            void removeCheatFlag (RoR2.ConVar.BaseConVar cv)
            {
                cv.flags &= AllFlagsNoCheat;
            }
            orig(self);
            CommandHelper.RegisterCommands(self);
            CommandHelper.RegisterConVars(self);
            removeCheatFlag(self.FindConVar("sv_time_transmit_interval"));
            removeCheatFlag(self.FindConVar("run_scene_override"));
            removeCheatFlag(self.FindConVar("stage1_pod"));
            self.FindConVar("timescale").helpText += " Use time_scale instead!";
            self.FindConVar("director_combat_disable").helpText += " Use no_enemies instead!";
            self.FindConVar("timestep").helpText += " Let the ror2cheats team know if you need this convar.";
            self.FindConVar("cmotor_safe_collision_step_threshold").helpText += " Let the ror2cheats team know if you need this convar.";
            self.FindConVar("cheats").helpText += " But you already have the RoR2Cheats mod installed...";
        }

        private static void UnlockConsole(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                MoveType.After,
                x => x.MatchCastclass(typeof(ConCommandAttribute))
                );
            c.EmitDelegate<Func<ConCommandAttribute, ConCommandAttribute>>(
                (conAttr) =>
                {
                    conAttr.flags &= AllFlagsNoCheat;
                    if (conAttr.commandName == "run_set_stages_cleared")
                    {
                        conAttr.helpText = MagicVars.RUNSETSTAGESCLEARED_HELP;
                    }
                    return conAttr;
                });
        }



        private static void CombatDirector_SetNextSpawnAsBoss(On.RoR2.CombatDirector.orig_SetNextSpawnAsBoss orig, CombatDirector self)
        {
            orig(self);
            if(RoR2Cheats.nextBoss)
            {
                //WeightedSelection<DirectorCard> weightedSelection = new WeightedSelection<DirectorCard>(1);
                //weightedSelection.AddChoice(ClassicStageInfo.instance.monsterSelection.GetChoice(1));
                var selection = ClassicStageInfo.instance.monsterSelection;
                //DirectorCard selected = selection.GetChoice(0).value;
                
                for (int i = 0; i < ClassicStageInfo.instance.monsterSelection.Count; i++)
                {
                    Debug.Log(selection.GetChoice(i).value.spawnCard.name.ToUpper());
                    if (selection.GetChoice(i).value.spawnCard.name.ToUpper().Contains(RoR2Cheats.nextBossName.ToUpper()))
                    {
                        var selected = selection.GetChoice(i).value;
                        Debug.Log("Matched: " + selected.spawnCard.name);
                        //self.OverrideCurrentMonsterCard(selected);
                        self.OverrideCurrentMonsterCard(selected);
                    }
                }
                //self.OverrideCurrentMonsterCard(selected);
                //ClassicStageInfo.instance.monsterSelection.
                //self.OverrideCurrentMonsterCard(selection.GetChoice(0).value);
            }
            //throw new NotImplementedException();
        }

        private static void TeleporterInteraction_OnStateChanged(On.RoR2.TeleporterInteraction.orig_OnStateChanged orig, TeleporterInteraction self, int oldActivationState, int newActivationState)
        {
            orig(self, oldActivationState, newActivationState);
            //throw new NotImplementedException();
        }

        private static void SeedHook(On.RoR2.PreGameController.orig_Awake orig, PreGameController self)
        {
            orig(self);
            if (RoR2Cheats.seed != 0)
            {
                self.runSeed = RoR2Cheats.seed;
            }
        }

        public static void onPrePopulateSetMonsterCreditZero(SceneDirector director)
        {
            //Note that this is not a hook, but an event subscription.
            director.SetFieldValue("monsterCredit", 0);
        }

        //private static void GameNetworkManager_cctor(ILContext il)
        //{
        //    ILCursor c = new ILCursor(il);
        //    c.GotoNext(
        //        x => x.MatchLdstr("sv_time_transmit_interval"),
        //        x => x.MatchLdcI4(out _),
        //        x => x.MatchLdcR4(out _)
        //        );
        //    c.Next.Next.Next.Operand = Cheats.TickIntervalMulti;

        //}

        //private static void GameNetworkManager_FixedUpdateServer(ILContext il)
        //{
        //    ILCursor c = new ILCursor(il);
        //    //c.GotoNext(
        //    //    x => x.MatchLdarg(0),
        //    //    x => x.MatchLdfld("RoR2.Networking.GameNetworkManager", "timeTransmitTimer"),
        //    //    x => x.MatchLdsfld("RoR2.Networking.GameNetworkManager", "svTimeTransmitInterval")
        //    //    );
        //    //c.Index += 4;
        //    //c.Emit(OpCodes.Ldc_R4, Cheats.TickIntervalMulti);
        //    //c.Emit(OpCodes.Mul);
        //    c.GotoNext(
        //        x => x.MatchLdarg(0),
        //        x => x.MatchLdfld("RoR2.Networking.GameNetworkManager", "timeTransmitTimer"),
        //        x => x.MatchLdsfld("RoR2.Networking.GameNetworkManager", "svTimeTransmitInterval")
        //        );
        //    //c.Index += 4;
        //    //c.Emit(OpCodes.Ldc_R4, Cheats.TickIntervalMulti);
        //    //c.Emit(OpCodes.Mul);
        //    //c.Prev.OpCode = OpCodes.Nop;
        //    c.Index += 2;
        //    c.RemoveRange(2);
        //    c.Emit(OpCodes.Ldc_R4, Cheats.TickRate);

        //}

    }
}
