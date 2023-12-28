using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using PortableMultiTool.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;

namespace PortableMultiTool;

[Serializable, HarmonyPatch]
public class Config : SyncedInstance<Config>
{
    public static ConfigEntry<int> hackPadCost;
    public static ConfigEntry<float> hackPadHackDuration;
    public static ConfigEntry<float> hackPadBatteryLife;

    public Config(ConfigFile cfg)
    {
        InitInstance(this);

        hackPadCost = cfg.Bind(
            PortableMultiToolBase.MODGUID,
            "HackPad_Cost",
            60,
            "Cost (in credits) to purchase the Hack Pad from the ship computer."
        );

        hackPadHackDuration = cfg.Bind(
            PortableMultiToolBase.MODGUID,
            "HackPad_HackDuration",
            5f,
            "Number of seconds it takes for the Hack Pad to complete its hack."
        );

        hackPadBatteryLife = cfg.Bind(
            PortableMultiToolBase.MODGUID,
            "HackPad_BatteryLife",
            60f,
            "Number of seconds the Hack Pad battery lasts from a full charge."
        );
    }

    public static void RequestSync()
    {
        if (!IsClient) return;

        using FastBufferWriter stream = new(IntSize, Allocator.Temp);
        MessageManager.SendNamedMessage("HackPad_OnRequestConfigSync", 0uL, stream);
    }

    public static void OnRequestSync(ulong clientId, FastBufferReader _)
    {
        if (!IsHost) return;

        PortableMultiToolBase.Instance.Logger.LogInfo($"Config sync request received from client: {clientId}");

        byte[] array = SerializeToBytes(Instance);
        int value = array.Length;

        using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

        try
        {
            stream.WriteValueSafe(in value, default);
            stream.WriteBytesSafe(array);

            MessageManager.SendNamedMessage("HackPad_OnReceiveConfigSync", clientId, stream);
        }
        catch (Exception e)
        {
            PortableMultiToolBase.Instance.Logger.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
        }
    }

    public static void OnReceiveSync(ulong _, FastBufferReader reader)
    {
        if (!reader.TryBeginRead(IntSize))
        {
            PortableMultiToolBase.Instance.Logger.LogError("Config sync error: Could not begin reading buffer.");
            return;
        }

        reader.ReadValueSafe(out int val, default);
        if (!reader.TryBeginRead(val))
        {
            PortableMultiToolBase.Instance.Logger.LogError("Config sync error: Host could not sync.");
            return;
        }

        byte[] data = new byte[val];
        reader.ReadBytesSafe(ref data, val);

        SyncInstance(data);

        PortableMultiToolBase.Instance.Logger.LogInfo("Successfully synced config with host.");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    public static void InitializeLocalPlayer()
    {
        if (IsHost)
        {
            MessageManager.RegisterNamedMessageHandler("HackPad_OnRequestConfigSync", OnRequestSync);
            Synced = true;

            return;
        }

        Synced = false;
        MessageManager.RegisterNamedMessageHandler("HackPad_OnReceiveConfigSync", OnReceiveSync);
        RequestSync();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.StartDisconnect))]
    public static void PlayerLeave()
    {
        RevertSync();
    }
}