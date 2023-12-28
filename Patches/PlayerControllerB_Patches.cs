using GameNetcodeStuff;
using HarmonyLib;
using PortableMultiTool.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace PortableMultiTool.Patches;

/*
[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerB_Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.Start))]
    private static void Start_Patch(PlayerControllerB __instance)
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            var customNetworkedPlayer = GameObject.Instantiate(Assets.CustomNetworkedPlayerPrefab, __instance.transform);
            customNetworkedPlayer.SetActive(true);
            customNetworkedPlayer.GetComponent<CustomNetworkedPlayer>().Initialize(__instance);
            customNetworkedPlayer.GetComponent<NetworkObject>().Spawn();
        }
    }
}
*/
