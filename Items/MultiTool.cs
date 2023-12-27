using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace PortableMultiTool.Items;
public class MultiTool : GrabbableObject
{
    [SerializeField]
    private TextMeshProUGUI screenText;

    [SerializeField]
    private string screenTextPrefix, screenTextCaret;

    [SerializeField]
    private float caretBlinkRate = 0.5f;

    [SerializeField]
    private AudioSource audioSource;

    private float hackProgress = -1f;

    private bool displayingCompletedHack = false;

    private bool IsHacking { get => hackProgress > 0 && currentHackTarget != null; }

    private readonly Dictionary<TerminalAccessibleObject, InteractTrigger> hackTriggers = [];

    private TerminalAccessibleObject currentHackTarget = null;

    [SerializeField]
    private AudioClip idle, hackInProgress, hackFailure, hackSuccess, hackAbort, powerOn, powerOff;

    private Dictionary<AudioType, AudioClip> audioSourceMap = new Dictionary<AudioType, AudioClip>();

    private Coroutine idleCoroutine;

    private enum AudioType
    {
        IDLE = 0,
        HACK_PROGRESS,
        HACK_SUCCESS,
        HACK_FAILURE,
        HACK_ABORT,
        POWER_ON,
        POWER_OFF
    }

    public override void Start()
    {
        base.Start();
        audioSourceMap = new Dictionary<AudioType, AudioClip>
        {
            { AudioType.IDLE, idle },
            { AudioType.HACK_PROGRESS, hackInProgress },
            { AudioType.HACK_SUCCESS, hackSuccess },
            { AudioType.HACK_FAILURE, hackFailure },
            { AudioType.HACK_ABORT, hackAbort },
            { AudioType.POWER_ON, powerOn },
            { AudioType.POWER_OFF, powerOff }
        };
    }

    private void OnEnable()
    {
        idleCoroutine = StartCoroutine(IdleCoroutine());
    }

    private void OnDisable()
    {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
    }

    private void FixedUpdate()
    {
        if (isBeingUsed)
        {
            screenText.text = CalculateScreenText();
        }
        else
        {
            screenText.text = "";
            return;
        }

        if (playerHeldBy == null || !IsOwner || displayingCompletedHack) return;

        var keys = hackTriggers.Keys.ToList();
        foreach (var key in keys)
        {
            if (key.inCooldown)
            {
                Destroy(hackTriggers[key].gameObject);
                hackTriggers.Remove(key);
            }
        }

        var ray = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);
        var target = Physics.RaycastAll(ray, 3f)
            .OrderBy(hit => hit.distance)
            .SelectMany(hit => new TerminalAccessibleObject[]
            {
                hit.collider.GetComponentInChildren<TerminalAccessibleObject>(),
                hit.collider.GetComponentInParent<TerminalAccessibleObject>()
            })
            .Where(c => c != null && !c.inCooldown && c.isPoweredOn)
            .Where(c => !c.gameObject.name.ToLower().Contains("landmine"))
            .Where(c => !hackTriggers.ContainsKey(c))
            .FirstOrDefault();

        if (target != null)
        {
            var trigger = ConfigureHackInteraction(target, OnHackProgress, OnFinishHack, OnStopHack, 5f);
            hackTriggers.Add(target, trigger);
        }
    }

    private IEnumerator IdleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            if (IsOwner && isBeingUsed)
            {
                PlayAudio(AudioType.IDLE);
            }
        }
    }

    [ServerRpc]
    private void PlayAudio_ServerRpc(int type, bool isLoud)
    {
        PlayAudio_ClientRpc(type, isLoud);
    }
    [ClientRpc]
    private void PlayAudio_ClientRpc(int type, bool isLoud)
    {
        if (audioSourceMap.TryGetValue((AudioType)type, out var audio))
        {
            PortableMultiToolBase.Instance.Logger.LogWarning($"Playing audio {type}");

            audioSource.Stop();
            audioSource.PlayOneShot(audio);
            //WalkieTalkie.TransmitOneShotAudio(audioSource, audio, audioSource.volume);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseIsInsideClosedShip: isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            if (isLoud && playerHeldBy != null)
            {
                playerHeldBy.timeSinceMakingLoudNoise = 0f;
            }
        }
        else
        {
            PortableMultiToolBase.Instance.Logger.LogWarning($"Tried to play invalid audio type of value: {type}");
        }
    }
    private void PlayAudio(AudioType type, bool isLoud = true)
    {
        if (!IsOwner) return;

        if (IsHost)
        {
            PlayAudio_ClientRpc((int)type, isLoud);
        }
        else
        {
            PlayAudio_ServerRpc((int)type, isLoud);
        }
    }

    [ServerRpc]
    private void StopAudio_ServerRpc()
    {
        StopAudio_ClientRpc();
    }
    [ClientRpc]
    private void StopAudio_ClientRpc()
    {
        audioSource.Stop();
    }
    private void StopAudio()
    {
        if (!IsOwner) return;
        if (IsHost || IsServer)
        {
            StopAudio_ClientRpc();
        }
        else
        {
            StopAudio_ServerRpc();
        }
    }

    private string CalculateScreenText()
    {
        if (IsHacking)
        {
            if (hackProgress >= 1)
            {
                return $"hacking...\n0%  . . . . . . . . . \n10% . . . . . . . . . \n20% . . . . . . . . . \n30% . . . . . . . . . \n40% . . . . . . . . . \n50% . . . . . . . . . \n60% . . . . . . . . . \n70% . . . . . . . . . \n80% . . . . . . . . . \n90% . . . . . . . . . \n100% - Passcode: {currentHackTarget.objectCode.ToUpper()}!";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("hacking...");

            int totalLines = 10; // 10 lines for 0% to 90%
            int totalDotsPerLine = 9;
            int totalDots = totalLines * totalDotsPerLine;
            int dotsToDisplay = (int)Math.Ceiling(hackProgress * totalDots);

            for (int i = 0; i <= 90; i += 10)
            {
                sb.Append(i == 0 ? "0%  " : $"{i}% ");

                int lineDots = Math.Min(totalDotsPerLine, dotsToDisplay);
                sb.Append(new string('.', lineDots).Replace(".", ". "));

                sb.AppendLine();

                dotsToDisplay -= totalDotsPerLine;
                if (dotsToDisplay <= 0)
                {
                    break;
                }
            }

            return sb.ToString();
        }
        else
        {
            int caretCount = (int)(Time.time / caretBlinkRate % 3.9);
            var caretString = new string(screenTextCaret[0], caretCount);
            return $"{screenTextPrefix}{caretString}";
        }
    }

    public override void DiscardItem()
    {
        isBeingUsed = false;
        base.DiscardItem();
        ClearTriggers();
    }

    public override void UseUpBatteries()
    {
        base.UseUpBatteries();
        ClearTriggers();
        PlayAudio(AudioType.POWER_OFF);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        PortableMultiToolBase.Instance.Logger.LogWarning($"Item used: {used}; button down: {buttonDown}");

        if (!used)
        {
            ClearTriggers();
            PlayAudio(AudioType.POWER_OFF);
        }
        else
        {
            PlayAudio(AudioType.POWER_ON);
        }
    }

    private void ClearTriggers()
    {
        foreach (var trigger in hackTriggers.Values)
        {
            Destroy(trigger.gameObject);
        }
        hackTriggers.Clear();
        ResetHackProgress();
    }

    private void ResetHackProgress()
    {
        hackProgress = -1;
    }

    private void OnHackProgress(TerminalAccessibleObject hackTarget, float progress)
    {
        if (!IsHacking)
        {
            OnStartHack(hackTarget, progress);
        }
        hackProgress = progress;
        currentHackTarget = hackTarget;
    }

    private void OnStartHack(TerminalAccessibleObject hackTarget, float progress)
    {
        PlayAudio(AudioType.HACK_PROGRESS);
    }

    private void OnFinishHack(TerminalAccessibleObject hackTarget, PlayerControllerB interactingPlayer)
    {
        PortableMultiToolBase.Instance.Logger.LogWarning($"Finished hacking");
        
        PlayAudio(AudioType.HACK_SUCCESS);
        
        ClearTriggers();
        displayingCompletedHack = true;
        hackProgress = 1f;
        StartCoroutine(ExecuteAfter(3f, () =>
        {
            displayingCompletedHack = false;
            ResetHackProgress();
            currentHackTarget = null;
        }));

        hackTarget.CallFunctionFromTerminal();
    }

    private void OnStopHack(TerminalAccessibleObject hackTarget, PlayerControllerB interactingPlayer)
    {
        if (displayingCompletedHack) return;
        
        PortableMultiToolBase.Instance.Logger.LogWarning($"Hacking aborted");

        PlayAudio(AudioType.HACK_ABORT);
        
        ResetHackProgress();
        currentHackTarget = null;
    }

    private IEnumerator ExecuteAfter(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }

    public override void EquipItem()
    {
        base.EquipItem();

        screenText.enabled = true;
    }

    public override void PocketItem()
    {
        base.PocketItem();

        StopAudio();

        screenText.enabled = false;
    }

    private static InteractTrigger ConfigureHackInteraction(
        TerminalAccessibleObject target,
        Action<TerminalAccessibleObject, float> onInteractionHold,
        Action<TerminalAccessibleObject, PlayerControllerB> onFinishInteract,
        Action<TerminalAccessibleObject, PlayerControllerB> onStopInteract,
        float hackDuration = 3f)
    {
        var interactObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        interactObject.transform.position = target.transform.position;
        if (target.isBigDoor)
        {
            interactObject.transform.position += Vector3.up * 2f;
            interactObject.transform.localScale = new Vector3 (1.0f, 3.0f, 1.0f);
        }
        if (interactObject.GetComponent<BoxCollider>() is BoxCollider b && b != null)
        {
            b.isTrigger = true;
        }
        else
        {
            interactObject.AddComponent<BoxCollider>().isTrigger = true;
        }
        interactObject.tag = "InteractTrigger";
        interactObject.layer = LayerMask.NameToLayer("InteractableObject");

        var interact = interactObject.AddComponent<InteractTrigger>();
        interact.hoverIcon = Assets.HandIcon;
        interact.hoverTip = "Hack: [E]";
        interact.holdTip = "";
        interact.interactable = true;
        interact.oneHandedItemAllowed = true;
        interact.holdInteraction = true;
        interact.timeToHold = hackDuration;
        interact.timeToHoldSpeedMultiplier = 1;
        interact.interactCooldown = true;
        interact.cooldownTime = hackDuration * 2f;
        interact.stopAnimationString = "SA_stopAnimation";
        interact.animationWaitTime = 2f;
        interact.lockPlayerPosition = true;

        interact.onInteract = new InteractEvent();
        interact.onInteractEarly = new InteractEvent();
        interact.onStopInteract = new InteractEvent();
        interact.holdingInteractEvent = new InteractEventFloat();
        interact.onCancelAnimation = new InteractEvent();

        interact.onInteract.AddListener(p => onFinishInteract?.Invoke(target, p));
        interact.holdingInteractEvent.AddListener(v => onInteractionHold?.Invoke(target, v));
        interact.onStopInteract.AddListener(p => onStopInteract?.Invoke(target, p));

        PortableMultiToolBase.Instance.Logger.LogWarning($"Added hacking interaction trigger to: \"{target.name}\"");

        return interact;
    }
}
