using LethalLib.Modules;
using PortableMultiTool.Items;
using PortableMultiTool.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static LethalLib.Modules.Items;

namespace PortableMultiTool;
internal static class Assets
{
    private static AssetBundle bundle;

    internal static GameObject CustomNetworkedPlayerPrefab { get; private set; }
    internal static Sprite HandIcon { get; private set; }
    internal static MultiTool MultiToolPrefab { get; private set; }

    public static void LoadAssets()
    {
        PortableMultiToolBase.Instance.Logger.LogWarning("Loading custom assets");

        bundle = AssetBundle.LoadFromMemory(Properties.Resources.assets);

        HandIcon = bundle.LoadAsset<Sprite>("HandIcon");

        MultiToolPrefab = bundle.LoadAsset<GameObject>("MultiTool").GetComponent<MultiTool>();
        NetworkManager.Singleton.AddNetworkPrefab(MultiToolPrefab.gameObject);
        RegisterShopItem(MultiToolPrefab.itemProperties, MultiToolPrefab.itemProperties.creditsWorth);

        CustomNetworkedPlayerPrefab = new GameObject("Custom Networked Player Controller");
        CustomNetworkedPlayerPrefab.hideFlags = HideFlags.HideAndDontSave;
        CustomNetworkedPlayerPrefab.SetActive(false);
        CustomNetworkedPlayerPrefab.AddComponent<NetworkObject>();
        CustomNetworkedPlayerPrefab.AddComponent<CustomNetworkedPlayer>();

        NetworkManager.Singleton.AddNetworkPrefab(CustomNetworkedPlayerPrefab);
    }
}

