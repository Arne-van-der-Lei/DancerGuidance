
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
    [UdonSynced]
    private bool CanClick = true;
    public Vector3 offset;
    public int dancesNeeded;
    public float MaxDistanceForClick = 5.0f;
    public float ClickDelay = 60.0f;
    [SerializeField]
    public TMP_Text nameplate;
    [SerializeField]
    public TMP_Text text;
    [SerializeField]
    private CanvasGroup canvasGroup;
    
    private VRCPlayerApi player;
    private bool IsEnabled = false;
    
    private Color red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    private Color green = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    private Color orange = new Color(1.0f, 0.5f, 0.0f, 1.0f);
    
    private void Start()
    {
        player = Networking.GetOwner(gameObject);
        UpdateEnabled();
        nameplate.text = player.displayName;
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
    {
        foreach (PlayerData.Info info in infos)
        {
            if (info.Key == "Talox.DancerGuidance.OverHeadNumber")
            {
                UpdateEnabled();
            }
        }
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        UpdateEnabled();
    }

    public void Update()
    {
        if (!IsEnabled)
        {
            text.text = "";
            return;
        }
        text.text = number.ToString();
        nameplate.color = number >= dancesNeeded ? green : number > 0 ? orange : red;
        VRCPlayerApi.TrackingData HeadOwner = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        VRCPlayerApi.TrackingData HeadLocalPlayer = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        transform.position = HeadOwner.position + offset;
        transform.rotation = Quaternion.LookRotation(-HeadLocalPlayer.position + transform.position, Vector3.up);
    }
    
    public void OnClick()
    {
        if (!CanClick)
            return;
        
        if (!player.isLocal)
        {
            if ((transform.position - Networking.LocalPlayer.GetPosition()).magnitude > MaxDistanceForClick) 
                return;
            
            SendCustomNetworkEvent(NetworkEventTarget.Owner,nameof(OnClick));
            number++;
            CanClick = false;
            return;
        }
        
        CanClick = false;
        number++;
        RequestSerialization();
        SendCustomEventDelayedSeconds(nameof(OnClickEnd),ClickDelay);
    }
    
    public void OnClickEnd()
    {
        CanClick = true;
        RequestSerialization();
    }
    
    private void UpdateEnabled()
    {
        IsEnabled = PlayerData.GetBool(Networking.LocalPlayer, "Talox.DancerGuidance.OverHeadNumber");
        bool OwnerEnabled = PlayerData.GetBool(player, "Talox.DancerGuidance.OverHeadNumber");
        canvasGroup.alpha = !OwnerEnabled & IsEnabled ? 1 : 0;
    }
}
