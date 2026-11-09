using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int gameIndex = 1;
    public int maxPlayers = 4;
    [SerializeField] private NetworkRunner runnerPrefab;

    public Action OnPublicBrowserOpen;
    public Action OnWaitingAreaOpen;
    public Action OnConnectedToServer;
    public Action<string> OnStartConnecting;
    public Action OnCloseConnecting;

    Dictionary<PlayerRef, NetworkObject> PlayerList = new Dictionary<PlayerRef, NetworkObject>();

    private NetworkRunner runner;
    public NetworkRunner Runner => runner;
    bool isStarting = false;

    private SessionCallback callbacks;
    public SessionCallback Callbacks => callbacks;

    private const string PUBLIC_LOBBY = "Public_Lobby";
    private void Awake()
    {
        if(Instance!=null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
   
    private void OnEnable()
    {
        
    }
    private void RegisterWaitingAreaCallbacks()
    {
        if (callbacks == null) return;
        callbacks.OnPlayerJoinedEvent += HandlePlayerJoin;
        callbacks.OnPlayerLeftEvent += HandlePlayerLeft;
        Debug.Log("Player subscription");
    }

    private void HandlePlayerJoin(PlayerRef player)
    {
        //Debug.Log("HandlePlayerJoin called!");

        //Debug.Log($"Runner exists: {runner != null}");

        if (runner == null)
            return;

        //Debug.Log($"Is Server: {runner.IsServer}");

        if (!runner.IsServer)
            return;
        NetworkObject playerObj = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
        runner.SetPlayerObject(player, playerObj);
        PlayerList[player] = playerObj;
        Debug.Log("Player Instance spawned");
    }

    private void HandlePlayerLeft(PlayerRef player)
    {
        if (!runner.IsServer)
            return;
        if(PlayerList.TryGetValue(player,out NetworkObject playerObj))
        {
            runner.Despawn(playerObj);
            PlayerList.Remove(player);
        }
    }
    public void NotifyPlayerListChanged()
    {
        callbacks.OnPlayerListChanged?.Invoke();
    }
    public NetworkRunner CreateRunner()
    {
        if (runner != null) return runner;
        runner = Instantiate(runnerPrefab);
        runner.name = "Network Runner";
        runner.ProvideInput = true;

        callbacks = runner.gameObject.AddComponent<SessionCallback>();

        RegisterWaitingAreaCallbacks();

        DontDestroyOnLoad(runner.gameObject);

        return runner;

    }

    public NetworkSceneInfo GetGameScene()
    {
        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(gameIndex), LoadSceneMode.Single);
        return sceneInfo;
    }

    public NetworkSceneInfo GetCurrentScene()
    {
        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), LoadSceneMode.Single);
        return sceneInfo;
    }

    public async Task CreatePrivateRoom(string roomName)
    {
        OnStartConnecting?.Invoke("Creating A Private Room...");
        var properties = new Dictionary<string, SessionProperty>
        {
            {"mode", "private" }
        };
        await StartSession(roomName, GameMode.Host,properties, false);
        OnWaitingAreaOpen?.Invoke();
        OnCloseConnecting?.Invoke();
        //WaitingAreaUI.Instance.Initialize();
    }

    public async Task JoinRoom(string roomName)
    {
        OnStartConnecting?.Invoke("Joining Room...");
        await StartSession(roomName, GameMode.Client,null, false);
        OnCloseConnecting?.Invoke();
    }

    public async Task CreatePublicRoom(string roomName)
    {
        OnStartConnecting?.Invoke("Creating A Public Room...");
        var properties = new Dictionary<string, SessionProperty>
        {
            {"mode", "public" }
        };
        await StartSession(roomName, GameMode.Host, properties, true);
        OnCloseConnecting?.Invoke();
        OnWaitingAreaOpen?.Invoke();
        //WaitingAreaUI.Instance.Initialize();
    }
    public async Task<bool> ConnectingToServer(Action<List<SessionInfo>> onSessionUpdated)
    {
        if (runner != null) return false;

        var newRunner = CreateRunner();
        callbacks.OnSessionsUpdated += onSessionUpdated;
        newRunner.AddCallbacks(callbacks);

        OnStartConnecting?.Invoke("Connecting...");

        var result = await newRunner.JoinSessionLobby(SessionLobby.Custom, PUBLIC_LOBBY);


        if (!result.Ok)
        {
            Debug.LogError($"Lobby error: {result.ShutdownReason}");
            return false;
        }
        OnConnectedToServer?.Invoke();
        return true;
    }
    public void OpenPublicBrowser()
    {
        OnPublicBrowserOpen?.Invoke();
    }

    public async Task JoinPublicSession(string sessionName)
    {
        
        if (runner == null) return;
        var sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            Scene = GetCurrentScene(),
            SceneManager = sceneManager
        });

        
        OnWaitingAreaOpen?.Invoke();

        if (!result.Ok)
        {
            Debug.LogError($"Failed To Start Session {result.ShutdownReason}");
            await CleanupRunner();
        }
        else
        {
            Debug.Log($"Session connected with {runner.SessionInfo.Name}");
        }
    }

    public async Task StartSession(string roomName, GameMode mode, Dictionary<string, SessionProperty> sessionProp, bool isPublic)
    {
        if (isStarting) return;
        try
        {
            var runner = CreateRunner();
            var sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            isStarting = true;

            var args = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomName,
                Scene = GetCurrentScene(),
                PlayerCount = maxPlayers,
                SessionProperties = sessionProp,
                IsOpen = true,
                IsVisible = isPublic,
                CustomLobbyName = isPublic ? PUBLIC_LOBBY : null,
                SceneManager = sceneManager
            };
            var result = await runner.StartGame(args);

            if (!result.Ok)
            {
                Debug.LogError($"Failed To Start Session {result.ShutdownReason}");
                await CleanupRunner();
            }
            else
            {
                Debug.Log($"Session connected with {runner.SessionInfo.Name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            await CleanupRunner();
        }
        finally
        {
            isStarting = false;
        }
    }

    public async Task LeaveRoom()
    {
        if (runner == null) return;
        await CleanupRunner();
        PlayerList.Clear();
    }

    public void StartGame()
    {
        if (runner == null || !runner.IsServer) return;

        runner.SessionInfo.IsOpen = false;
        runner.SessionInfo.IsVisible = false;

        var gameScene = SceneRef.FromIndex(gameIndex);
        runner.LoadScene(gameScene,LoadSceneMode.Single);
    }

    
    public async Task CleanupRunner()
    {
        if (runner == null) return;
        var oldRunner = runner;
        runner = null;
        
        await oldRunner.Shutdown();
        Destroy(oldRunner.gameObject);
    }
    private void OnDestroy()
    {

        if (callbacks == null) return;
        callbacks.OnPlayerJoinedEvent += HandlePlayerJoin;
        callbacks.OnPlayerLeftEvent += HandlePlayerLeft;
    }
}
