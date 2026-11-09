using UnityEngine;
using TMPro;
public class MenuUIControls : MonoBehaviour
{
    [Header("Panels Bar")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject waitingAreaPanel;
    [SerializeField] private GameObject createRoomPanel;

    [Header("Text Control")]
    [SerializeField] private TMP_Text connetingText;
    private void Start()
    {
    }
    private void OnEnable()
    {
        NetworkManager.Instance.OnConnectedToServer += CloseConnectionPanel;
        NetworkManager.Instance.OnCloseConnecting += CloseConnectionPanel;
        NetworkManager.Instance.OnStartConnecting += OpenConnectionPanel;
        NetworkManager.Instance.OnPublicBrowserOpen += OpenLobbyPanel;
        NetworkManager.Instance.OnWaitingAreaOpen += OpenWaitingAreaPanel;
        WaitingAreaUI.Instance.OnLeaveRoom += CloseWaitingAreaPanel;
    }
    private void OnDestroy()
    {
        NetworkManager.Instance.OnConnectedToServer = CloseConnectionPanel;
        NetworkManager.Instance.OnCloseConnecting -= CloseConnectionPanel;
        NetworkManager.Instance.OnStartConnecting -= OpenConnectionPanel;
        NetworkManager.Instance.OnPublicBrowserOpen -= OpenLobbyPanel;
        NetworkManager.Instance.OnWaitingAreaOpen -= OpenWaitingAreaPanel;
        WaitingAreaUI.Instance.OnLeaveRoom -= CloseWaitingAreaPanel;
    }
    public void OpenLobbyPanel()
    {
        lobbyPanel.SetActive(true);
    }
    public void CloseLobbyPanel()
    {
        lobbyPanel.SetActive(false);
    }
    void OpenWaitingAreaPanel()
    {
        waitingAreaPanel.SetActive(true);
    }
    void CloseWaitingAreaPanel()
    {
        waitingAreaPanel.SetActive(false);
    }
    public void OpenCreatePanel(bool active)
    {
        createRoomPanel.SetActive(active);
    }

    void OpenConnectionPanel(string txt)
    {
        connectionPanel.SetActive(true);
        connetingText.text = txt;
    }
    void CloseConnectionPanel()
    {
        connectionPanel.SetActive(false);
    }
}
