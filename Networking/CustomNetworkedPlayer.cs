using GameNetcodeStuff;
using System;
using System.Collections;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PortableMultiTool.Networking;
internal class CustomNetworkedPlayer : NetworkBehaviour
{
    private PlayerControllerB playerController;

    public void Initialize(PlayerControllerB playerController)
    {
        this.playerController = playerController;
    }

    public override void OnNetworkSpawn()
    {
        enabled = true;
        transform.SetParent(playerController.transform);
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        var lpc = GameNetworkManager.Instance.localPlayerController;
        if (lpc != null && playerController == lpc)
        {
            LocalUpdate();
        }
    }

    private void LocalUpdate()
    {
        /*
        playerController.health = 5000;
        playerController.sprintMeter = 5000;
        playerController.sprintMultiplier = 5;

        if (Keyboard.current.numpad0Key.wasPressedThisFrame)
        {
            PortableMultiToolBase.Instance.Logger.LogWarning("Pressed numpad 0!");
        }
        */
    }
}
