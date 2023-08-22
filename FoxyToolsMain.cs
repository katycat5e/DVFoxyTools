using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandTerminal;
using System.Reflection.Emit;
using UnityModManagerNet;
using HarmonyLib;

namespace FoxyTools
{
    public static class PluginInfo
    {
        public const string Guid = "cc.foxden.foxy_tools";
        public const string Name = "Fox Debug Tools";
        public const string Version = "2.0.0";
    }

    public static class FoxyToolsMain
    {
        public static UnityModManager.ModEntry Instance { get; private set; }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Instance = modEntry;

            var harmony = new Harmony("cc.foxden.foxytools");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        public static void Log(string message)
        {
            Instance.Logger.Log(message);
        }

        public static void Error(string message)
        {
            Instance.Logger.Error(message);
        }

        public static void Warning(string message)
        {
            Instance.Logger.Warning(message);
        }
    }

    [HarmonyPatch(typeof(Terminal))]
    public static class CommandPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void FixAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            var domain = (AppDomain)sender;

            foreach (var assembly in domain.GetAssemblies())
            {
                if (assembly.FullName == args.Name)
                {
                    return assembly;
                }
            }

            return null;
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void AddCommandDefinitions()
        {
            FTCommandAttribute.RegisterAll();
            FoxyToolsMain.Log("Registered FT Commands");
        }
    }

    [HarmonyPatch(typeof(CommandShell))]
    public static class ShellPatch
    {
        [HarmonyPatch(nameof(CommandShell.RegisterCommands))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileRegisterCommands(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_2)
                {
                    yield return CodeInstruction.Call(typeof(ShellPatch), nameof(AddIgnoredAssemblies));
                }
                yield return instruction;
            }
        }

        private static string[] AddIgnoredAssemblies(string[] assemblies)
        {
            return assemblies.Concat(new[]
            {
                "RuntimeUnityEditor."
            })
            .ToArray();
        }
    }
}
