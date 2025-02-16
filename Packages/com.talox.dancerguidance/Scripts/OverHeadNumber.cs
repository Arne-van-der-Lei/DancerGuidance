
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
    [UdonSynced]
    private int uniqueId;
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
    
    private bool IsMasterRestored = false;
    private bool IsLocalRestored = false;
    
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
        if(player.isLocal)
        {
            IsLocalRestored = true;

            if (IsMasterRestored)
            {
                CheckStartTime();
            }
        }
        
        if (player.isMaster)
        {
            if (player.isLocal)
            {
                PlayerData.SetLong("Talox.DancerGuidance.OverHeadNumberStartTime", DateTime.Now.Ticks);
            }
            else
            {
                IsMasterRestored = true;
                if(IsLocalRestored)
                    CheckStartTime();
            }
        }
        
        UpdateEnabled();
    }

    private void CheckStartTime()
    {
        long masterStartTime = PlayerData.GetLong(Networking.Master,"Talox.DancerGuidance.OverHeadNumberStartTime");
        long localStartTime = PlayerData.GetLong(player,"Talox.DancerGuidance.OverHeadNumberStartTime");
        
        if (masterStartTime > localStartTime + 36_000_000_000L * 8L)
        {
            PlayerData.SetLong("Talox.DancerGuidance.OverHeadNumberStartTime", masterStartTime);
            number = 0;
            PlayerData.SetInt("Talox.DancerGuidance.OverHeadNumberCount",0);
        }
        else
        {
            number = PlayerData.GetInt(player,"Talox.DancerGuidance.OverHeadNumberCount");
        }
    }
    
    public override void OnDeserialization()
    {
        text.text = number.ToString();
        nameplate.color = number >= dancesNeeded ? green : number > 0 ? orange : red;
    }

    public void Update()
    {
        if (!IsEnabled)
        {
            text.text = "";
            return;
        }
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
            text.text = number.ToString();
            nameplate.color = number >= dancesNeeded ? green : number > 0 ? orange : red;
            PlayerData.SetInt("Talox.DancerGuidance.OverHeadNumberCount", number);
            CanClick = false;
            return;
        }
        
        CanClick = false;
        number++;
        text.text = number.ToString();
        nameplate.color = number >= dancesNeeded ? green : number > 0 ? orange : red;
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
