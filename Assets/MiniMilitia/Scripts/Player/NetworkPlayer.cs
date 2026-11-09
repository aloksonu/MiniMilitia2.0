using UnityEngine;
using Fusion;
public class NetworkPlayer : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnSetName))]
    public NetworkString<_32> PlayerName { get; set; }

    [Networked, OnChangedRender(nameof(OnReadyChanged))]
    public NetworkBool IsReady { get; set; }

    
    public override void Spawned()
    {
        
        if (Object.HasInputAuthority)
        {
            RPC_SetPlayerName("Player" + Object.InputAuthority.PlayerId);
            WaitingAreaUI.Instance.SetupReady(this);
        }
    }
    public void SetReady(bool ready)
    {
        if (Object.HasInputAuthority)
            RPC_SetPlayerReadyState(ready);
    }
    private void OnReadyChanged()
    {
        NetworkManager.Instance.NotifyPlayerListChanged();
    }
    private void OnSetName()
    {
        NetworkManager.Instance.NotifyPlayerListChanged();
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerReadyState(bool value)
    {
        IsReady = value;
    }
}
