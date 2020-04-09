using BepInEx;
using RoR2;
using BepInEx.Configuration;
using LogLevel = DebugToolkit.Log.LogLevel;
using DebugToolkit.Commands;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using R2API;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace DebugToolkit
{
    [BepInDependency("com.bepis.r2api")]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    [BepInPlugin(GUID, modname, modver)]
    public class DebugToolkit : BaseUnityPlugin
    {
        public const string modname = "DebugToolkit", modver = "3.3.0";
        public const string GUID = "com.harbingerofme." + modname;

        private static MethodInfo RunCmdMethod;

        internal static ConfigFile Configuration;

        internal static GameObject DebugToolKitComponents;

        private void Awake()
        {
            Configuration = base.Config;

            DebugToolKitComponents = new GameObject("dtcn");
            DebugToolKitComponents.AddComponent<NetworkIdentity>();
            DebugToolKitComponents = DebugToolKitComponents.InstantiateClone("DebugToolKitComponentsNetworked");

            new Log(Logger);

            #region Not Release Message
#if !RELEASE   //Additional references in this block must be fully qualifed as to not use them in Release Builds.
            string gitVersion = "";
            using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream($"{this.GetType().Namespace}.Resources.CurrentCommit"))
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                gitVersion= reader.ReadToEnd();
            }
            Log.MessageWarning(
#if DEBUG       
                $"This is a debug build!"
#elif BLEEDING  
                $"This is a Bleeding-Edge build!"
#endif          
                ,Log.Target.Bepinex);
            Log.MessageWarning($"Commit: {gitVersion.Trim()}",Log.Target.Bepinex);
#endif
            #endregion

            Log.Message("Created by Harb, iDeathHD and . Based on RoR2Cheats by Morris1927.", LogLevel.Info, Log.Target.Bepinex);

            PermissionSystem.Init();
            Hooks.InitializeHooks();
            Command_Noclip.InitRPC();
            Command_Teleport.InitRPC();
            CurrentRun.timescaleNet = DebugToolKitComponents.AddComponent<TimescaleNet>();

            RunCmdMethod = typeof(Console).GetMethod("RunCmd", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void Start()
        {
            var _ = StringFinder.Instance;
            ArgsAutoCompletion.GatherCommandsAndFillStaticArgs();
        }

        private void Update()
        {
            if (Run.instance && Command_Noclip.IsActivated)
            {
                Command_Noclip.Update();
            }
        }

        public static void InvokeCMD(NetworkUser user, string commandname, params string[] arguments)
        {
            List<string> args = arguments.ToList<string>(); 
            if (Console.instance)
                RunCmdMethod.Invoke(Console.instance, new object []  { user, commandname, args});
            else
                Log.Message($"InvokeCMD called whilst no console instance exists.",LogLevel.Error,Log.Target.Bepinex);
        }


        /// <summary>
        /// Required for automated manifest building.
        /// </summary>
        /// <returns>Returns the TS manifest Version</returns>
        public static string GetModVer()
        {
            return modver;
        }
    }
}