
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class OverHeadNumber : UdonSharpBehaviour
{
    [UdonSynced]
    public int number;
    public Vector3 offset;
    [SerializeField]
    public TMP_Text text;
    [SerializeField]
    private CanvasGroup canvasGroup;
    
    private VRCPlayerApi player;
    private bool IsEnabled = false;
    
    private void Start()
    {
        player = Networking.GetOwner(gameObject);
        IsEnabled = PlayerData.GetBool(Networking.LocalPlayer, "Talox.DancerGuidance.OverHeadNumber");
        canvasGroup.alpha = IsEnabled ? 1 : 0;
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
    {
        if (player.isLocal)
        {
            foreach (PlayerData.Info info in infos)
            {
                if (info.Key == "Talox.DancerGuidance.OverHeadNumber")
                {
                    IsEnabled = PlayerData.GetBool(player, "Talox.DancerGuidance.OverHeadNumber");
                    canvasGroup.alpha = IsEnabled ? 1 : 0;
                }
            }
        }
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        IsEnabled = PlayerData.GetBool(Networking.LocalPlayer, "Talox.DancerGuidance.OverHeadNumber");
        canvasGroup.alpha = IsEnabled ? 1 : 0;
    }

    public void Update()
    {
        if (!IsEnabled)
        {
            text.text = "";
            return;
        }
        text.text = number.ToString();
        VRCPlayerApi.TrackingData HeadOwner = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        VRCPlayerApi.TrackingData HeadLocalPlayer = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        transform.position = HeadOwner.position + offset;
        transform.rotation = Quaternion.LookRotation(-HeadLocalPlayer.position + transform.position, Vector3.up);
    }
    
    public void OnClick()
    {
        if (!player.isLocal)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner,nameof(OnClick));
            number++;
            Debug.Log("SendCustomNetworkEvent");
            return;
        }
        
        Debug.Log("OnClick");
        
        number++;
        RequestSerialization();
    }
}
