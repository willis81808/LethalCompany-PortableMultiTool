using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

namespace PortableMultiTool.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManager_Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    private static void AddCustomNetworkBehaviors(GameNetworkManager __instance)
    {
        Assets.LoadAssets();

        /*
        foreach (var prefab in __instance.GetComponent<NetworkManager>().NetworkConfig.Prefabs.Prefabs)
        {
            if (prefab == null || prefab.Prefab == null) continue;
            if (prefab.Prefab.GetComponentInChildren<Turret>() is Turret turret && turret != null)
            {
                ConfigureHackInteraction(turret.gameObject);
            }
            else if (prefab.Prefab.GetComponentInChildren<Landmine>() is Landmine landmine && landmine != null)
            {
                ConfigureHackInteraction(landmine.gameObject);
            }
            else if (prefab.Prefab.name == "BigDoor")
            {
                ConfigureHackInteraction(prefab.Prefab);
            }
            else if (prefab.Prefab.GetComponentInChildren<BoxCollider>() is BoxCollider collider && collider != null)
            {
                ConfigureHackInteraction(collider.gameObject);
            }
        }
        */
    }
}
