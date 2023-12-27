using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace PortableMultiTool;

[BepInDependency("atomic.terminalapi", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(MODGUID, MODNAME, MODVERSION)]
public class PortableMultiToolBase : BaseUnityPlugin
{
    public const string MODGUID = "com.willis.lc.portablehackpad";
    public const string MODNAME = "PortableHackPad";
    public const string MODVERSION = "1.0.0";

    public static PortableMultiToolBase Instance { get; private set; }

    public new ManualLogSource Logger => base.Logger;

    public void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MODGUID);
        NetcodeWeaver();
    }

    private static void NetcodeWeaver()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }
}
