using UnityEngine;
using Fusion;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
public class WaitingAreaUI : MonoBehaviour
{
    public static WaitingAreaUI Instance;
    [SerializeField] private PlayerEntryUI playerEntryPrefab;
    [SerializeField] private Transform contentsHolder;

    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;

    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonText;

    private readonly List<GameObject> enteries = new();

    public Action OnLeaveRoom;

    private NetworkPlayer localPlayer;

    private SessionCallback callbacks;
    private NetworkRunner runner;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        //NetworkManager.Instance.OnWaitingAreaOpen += Initialize;
        NetworkManager.Instance.OnPublicBrowserOpen += RegisterCallbacks;

    }
    void RegisterCallbacks()
    {
        NetworkManager.Instance.OnWaitingAreaOpen += Initialize;
        NetworkManager.Instance.Callbacks.OnPlayerListChanged += RefreshPlayerList;
    }
    public void Initialize()
    {
        Initialize(NetworkManager.Instance.Runner, NetworkManager.Instance.Callbacks);
        
    }
    public void Initialize(NetworkRunner networkRunner, SessionCallback sessionCallbacks)
    {
        this.runner = networkRunner;
        this.callbacks = sessionCallbacks;

        callbacks.OnPlayerListChanged += RefreshPlayerList;

        roomNameText.text = runner.SessionInfo.Name;

        readyButton.onClick.AddListener(ToggleReady);
        RefreshPlayerList();
    }
    public void SetupReady(NetworkPlayer localPlayer)
    {
        //if (localPlayer != null) return;
        //runner.TryGetPlayerObject(runner.LocalPlayer, out NetworkObject localObject);
        if (localPlayer != null)
        {
            //localPlayer = localObject.GetComponent<NetworkPlayer>();
            this.localPlayer = localPlayer;
            Debug.Log("Local player setup in waiting UI");
        }
    }
    private void ToggleReady()
    {
        if (localPlayer == null) return;
        localPlayer.SetReady(!localPlayer.IsReady);
        readyButtonText.text = localPlayer.IsReady ? "Not Ready" : "Ready";
    }
    public void RefreshPlayerList()
    {
        ClearList();

        if (runner == null) return;

        var activePlayers = runner.ActivePlayers.ToList();
        Debug.Log("Active Players Count: " + activePlayers.Count);
        foreach(var player in activePlayers)
        {
            if (!runner.TryGetPlayerObject(player, out NetworkObject playerObject))
            {
                Debug.Log($"Player missing player object");
                continue;
            }
            NetworkPlayer networkPlayer = playerObject.GetComponent<NetworkPlayer>();
            if (networkPlayer == null)
            {
                Debug.Log($"Player missing network player");
                continue;
            }
            PlayerEntryUI newEntry = Instantiate(playerEntryPrefab, contentsHolder);
            newEntry.SetupUI(networkPlayer.PlayerName.ToString(), networkPlayer.IsReady);
            enteries.Add(newEntry.gameObject);

            Debug.Log($"{networkPlayer.PlayerName.ToString()} Entry UI Has Been Installed");
        }
        playerCountText.text = $"{runner.SessionInfo.PlayerCount}/{runner.SessionInfo.MaxPlayers} Players";
    }

    public async void LeaveRoom()
    {
        await NetworkManager.Instance.LeaveRoom();
        OnLeaveRoom?.Invoke();
    }

    private void ClearList()
    {
        foreach(var entry in enteries)
        {
            Destroy(entry);
        }
        enteries.Clear();
    }
    private void OnDestroy()
    {
        if (callbacks != null)
            callbacks.OnPlayerListChanged -= RefreshPlayerList;
        NetworkManager.Instance.OnWaitingAreaOpen -= Initialize;
        NetworkManager.Instance.Callbacks.OnPlayerListChanged -= RefreshPlayerList;
        ClearList();
    }
}
