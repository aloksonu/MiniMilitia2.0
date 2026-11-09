using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;

using UnityEngine.UI;
public class LobbyUI : MonoBehaviour
{
    public TMP_InputField roomInputField;
    public SessionEntryUI sessionPrefab;
    public Transform sessionParent;
    public Button joinButton;
    
    private SessionCallback callbacks;

    private List<GameObject> SessionObjList = new List<GameObject>();

    private SessionEntryUI currentSessionEntry;

    private async void Start()
    {
        bool success = await NetworkManager.Instance.ConnectingToServer(UpdateSessionList);
        joinButton.onClick.AddListener(JoinGame);
        joinButton.interactable = false;
    }
    public async void OpenPublicRooms()
    {
        bool success = await NetworkManager.Instance.ConnectingToServer(UpdateSessionList);
        NetworkManager.Instance.OpenPublicBrowser();
        //bool success = await NetworkManager.Instance.OpenPublicBrowser(UpdateSessionList);
        ////if (!success)return;
    }
    private void OnDestroy()
    {
        if (callbacks != null)
        {
            callbacks.OnSessionsUpdated -= UpdateSessionList;
        }
    }
    public void UpdateSessionList(List<SessionInfo> sessionInfo)
    {
        ClearList();
        foreach(var session in sessionInfo)
        {
            if (!session.IsOpen)
                continue;
            if(session.PlayerCount >= NetworkManager.Instance.maxPlayers)
            {
                continue;
            }
            SessionEntryUI sessionUI = Instantiate(sessionPrefab,sessionParent);
            sessionUI.SetupUI(session, this);
            SessionObjList.Add(sessionUI.gameObject);
            //Debug.Log(
            //    $"{session.Name} |" +
            //    $"{session.PlayerCount}/ {session.MaxPlayers}");
        }
    }
    //private SessionEntryUI GetSessionPrefab()
    //{
    //    SessionEntryUI sessionUI = Instantiate(sessionPrefab);
    //    return sessionUI;
    //}
    public async void JoinGame()
    {
        if (currentSessionEntry==null || string.IsNullOrEmpty(currentSessionEntry.SessionName)) return;
        await NetworkManager.Instance.JoinPublicSession(currentSessionEntry.SessionName);
        joinButton.interactable = false;
    }
    public void SetSessionEntry(SessionEntryUI value)
    {
        currentSessionEntry = value;
        joinButton.interactable = true;
    }
    private void ClearList()
    {
        foreach(var ui in SessionObjList)
        {
            DestroyImmediate(ui);
        }
        SessionObjList.Clear();
    }

    public async void CreatePrivateRoom()
    {
        string roomCode = GenerateRandomCode();
        await NetworkManager.Instance.CreatePrivateRoom(roomCode);
    }
    public async void JoinPrivateRoom(string code)
    {
        await NetworkManager.Instance.JoinRoom(code);
    }
    public async void CreatePublicRoom()
    {
        string inputRoomCode = roomInputField.text;
        if (string.IsNullOrEmpty(inputRoomCode))
            inputRoomCode = GenerateRandomCode();
        await NetworkManager.Instance.CreatePublicRoom(inputRoomCode);
    }

    public string GenerateRandomCode()
    {
        return System.Guid.NewGuid()
            .ToString("N")
            .Substring(0, 8)
            .ToUpperInvariant();
    }
}
