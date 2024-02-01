using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

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
        gameObject.SetActive(true);
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
        var target = Physics.OverlapSphere(playerController.gameplayCamera.transform.position, 10f)
            .Select(c => c.gameObject.GetComponentInChildren<GrabbableObject>())
            .Where(g => g != null)
            .OrderBy(g => Vector3.Distance(g.transform.position, playerController.gameplayCamera.transform.position))
            .FirstOrDefault();

        if (target == null) return;

        Vector3 targetDirection = target.transform.position - playerController.gameplayCamera.transform.position;
        Vector3 relativeDirection = playerController.gameplayCamera.transform.InverseTransformDirection(targetDirection);
        Vector2 simulatedMouseMovement = new Vector2(relativeDirection.x, relativeDirection.y);
        playerController.CalculateNormalLookingInput(simulatedMouseMovement);
        */
    }
}
