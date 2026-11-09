using UnityEngine;
using TMPro;
using Fusion;
using UnityEngine.UI;
public class SessionEntryUI : MonoBehaviour
{
    public TMP_Text sessionText;
    public TMP_Text countText;
    public Button sessionButton;

    LobbyUI lobbyUI;

    private string sessionName;
    public string SessionName => sessionName;
    public void SetupUI(SessionInfo sessionInfo, LobbyUI lobbyUI)
    {
        sessionName = sessionInfo.Name;
        sessionText.text = sessionInfo.Name;
        countText.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

        sessionButton.onClick.AddListener(SelectButton);
        this.lobbyUI = lobbyUI;
    }
    private void SelectButton()
    {
        sessionButton.interactable = false;
        lobbyUI.SetSessionEntry(this);
    }
}
