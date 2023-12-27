using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace PortableMultiTool.Util;

public class AudioMixerFixer : MonoBehaviour
{
    private static AudioMixerGroup _masterDiageticMixer;
    public static AudioMixerGroup MasterDiageticMixer
    {
        get
        {
            if (_masterDiageticMixer == null)
            {
                var referenceAudioSource = GameNetworkManager.Instance.GetComponent<NetworkManager>().NetworkConfig.Prefabs.Prefabs
                    .Select(p => p.Prefab.GetComponentInChildren<NoisemakerProp>())
                    .Where(p => p != null)
                    .Select(p => p.GetComponentInChildren<AudioSource>())
                    .Where(p => p != null)
                    .FirstOrDefault();
                if (referenceAudioSource == null)
                {
                    throw new Exception("Failed to locate a suitable AudioSource output mixer to reference! Could you be calling this method before the GameNetworkManager is initialized?");
                }
                _masterDiageticMixer = referenceAudioSource.outputAudioMixerGroup;
            }
            return _masterDiageticMixer;
        }
    }

    [SerializeField]
    private List<AudioSource> sourcesToFix;

    private void Start()
    {
        foreach (var source in GetComponentsInChildren<AudioSource>())
        {
            if (sourcesToFix.Contains(source)) continue;
            sourcesToFix.Add(source);
        }
        foreach (var source in sourcesToFix)
        {
            source.outputAudioMixerGroup = MasterDiageticMixer;
        }
    }
}
