using Mirror;
using Mirror.Examples.MultipleAdditiveScenes;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;


// track state for use with UI and such
public enum NetworkState { Offline, Handshake, Office, World }

public struct OnlinePlayersData {
    public List<String> playerNames;
    public List<uint> playerIds;

    public OnlinePlayersData(List<String> names, List<uint> ids) {
        playerNames = names;
        playerIds = ids;
    }
}

/// <summary>
/// Interface with an SQLite database to upkeep information on accounts and their
/// associated characters. Should only be working on the server; clients shouldn't see the data
/// </summary>
public class CVWNetworkManager : NetworkManager
{
    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.
    public static new CVWNetworkManager singleton { get; private set; }

    private SQLiteConnection connection;
    public CVWNetAuthenticator auth;
    public Dictionary<NetworkConnection, string> lobby = new Dictionary<NetworkConnection, string>();
    
    [Header("Counselor Options")]
    public bool isCounselor = false;

    [Header("Multiscene Management")]
    
    // A Dictionary that registers a NetworkIdentity as a key and a Scene as a value.
    readonly Dictionary<NetworkIdentity, Scene> openScenesByNetworkIdentityDictionary = new Dictionary<NetworkIdentity, Scene>();
    
    // Waiting Room Variables
    [Scene]
    public string waitingRoomSceneName;
    private Scene waitingRoomScene;
    private bool waitingRoomLoaded;

    // Office Scene variables
    [Scene]
    public string officeSceneName;
    public static List<Transform> officeStartPositions = new List<Transform>();
    private List<Scene> openOfficeScenes = new List<Scene>();


    [Header("Database")]
    public int characterLimit = 4;
    public int characterNameMaxLength = 16;
    public float saveInterval = 60f; // in seconds
    public UnityEventCharacterCreateMsgPlayer onServerCharacterCreate;
    public static Dictionary<NetworkIdentity, Player> onlinePlayers = new Dictionary<NetworkIdentity, Player>();


    [Serializable] public class UnityEventCharacterCreateMsgPlayer : UnityEvent<CharacterCreateMsg, Player> { }

    #region Character Creation

    public static async void SendAccountMsg(string username, string password, bool debugging)
    {
        await IdleForClient();
        Debug.Log("Sending Account Message for " + username);
        NetworkClient.connection.Send(new AddAccountMessage { userName = username, password = password, debug = debugging });
    }

    private void HandleAccountMessage(NetworkConnectionToClient conn, AddAccountMessage msg)
    {
        bool success = true; //Database.LogOn(msg.userName, msg.password, msg.debug);
    }

    // Creates a new character by instantiating it based on the prefab
    private Player RegisterCharacter(string characterName, string account)
    {
        //Player player = Instantiate(playerPrefab).GetComponent<Player>();
        Player player = GameObject.Find(characterName).GetComponent<Player>();
        player.name = characterName;
        player.SetAccount(account);
        return player;
    }

    /// <summary>
    /// Initializes a character from the prefab and adds it to the database if it's name is allowed/not taken. 
    /// </summary>
    /// <param name="conn"></param>
    private async void OnServerCharacterCreate(NetworkConnection conn)//, CharacterCreateMsg message)
    {
        await Idle(conn);
        // only while in lobby (aka after handshake and not ingame)
        // allowed character name?
        string account = "account";
        Player player = RegisterCharacter(conn.identity.name, account);
        if (IsAllowedCharacterName(player.name))
        {
            //string account = lobby[conn]; //remnant of previous architecture. Leaving in case is needed 
            Database.GetCharacter(player);
        }
        else
        {
            Debug.LogWarning("character name not allowed: " + conn.identity.name); //<- don't show on live server
            //ServerSendError(conn, "character name not allowed", false);
            Destroy(player);
            //make the error visible to the player
            //badName.SetActive(true);
        }
    }

    
    //Authenticates that given name is permitted; not profane etc
    //always set to true for now, fix later
    private bool IsAllowedCharacterName(string name)
    {
        return true;
    }

    #endregion

    #region Waiting Room

        // Initialize the waiting room. This is called OnServerStart()
        IEnumerator ServerInitializeWaitingRoom() {
            // Load the waiting room
            yield return SceneManager.LoadSceneAsync(waitingRoomSceneName, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });

            // Get the waiting room scene
            waitingRoomScene = SceneManager.GetSceneAt(1);
            waitingRoomLoaded = true;
        }

        // Move the player to the waiting room
        public void ServerMovePlayerToWaitingRoom(GameObject player) {
            // Tell client to load the waiting room additively
            NetworkConnectionToClient conn = player.GetComponent<NetworkIdentity>().connectionToClient;
            conn.Send(new SceneMessage { sceneName = waitingRoomSceneName, sceneOperation = SceneOperation.LoadAdditive });

            UnloadPlayerOfficeSceneIfOpen(conn);    
            
            // Move player to waiting room
            SceneManager.MoveGameObjectToScene(player, waitingRoomScene);
            PlayerInstanceManager targetPlayerInstanceManager = player.GetComponent<PlayerInstanceManager>();
            targetPlayerInstanceManager.SetCurrentSceneName(waitingRoomSceneName);
            targetPlayerInstanceManager.TargetSetCurrentSceneName(conn, waitingRoomSceneName);
            targetPlayerInstanceManager.MoveToStartPosition();
            //SceneManager.SetActiveScene(SceneManager.GetSceneByName(waitingRoomSceneName));
    }

        // Get the start position in the waiting room
        public override Transform GetStartPosition()
        {
            // first remove any dead transforms
            startPositions.RemoveAll(t => t == null);

            if (startPositions.Count == 0)
                return null;

            if (playerSpawnMethod == PlayerSpawnMethod.Random)
            {
                return startPositions[UnityEngine.Random.Range(0, startPositions.Count)];
            }
            else
            {
                Transform startPosition = startPositions[startPositionIndex];
                startPositionIndex = (startPositionIndex + 1) % startPositions.Count;
                return startPosition;
            }
        }

    #endregion

    #region Offices

        // Create an office for the given player if an office for them does not yet exist, then move them to the office
        public void ServerCreateOfficeAndMovePlayer(GameObject player) {

            // If an office exists for the user, return
            NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
            Scene userOpenOfficeScene;
            if (openScenesByNetworkIdentityDictionary.TryGetValue(identity, out userOpenOfficeScene)) return;

            StartCoroutine(ServerCreateOfficeAndMovePlayerDelayed(player));        
        }

        // Coroutine due to LoadSceneAsync completion necessary on server.
        IEnumerator ServerCreateOfficeAndMovePlayerDelayed(GameObject player) {
            // Load new office scene on server
            yield return SceneManager.LoadSceneAsync(officeSceneName, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });


            // Add new office to openOffices list
            Scene newOfficeScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            openOfficeScenes.Add(newOfficeScene);

            // Add to office scene dictionary
            NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
            openScenesByNetworkIdentityDictionary.Add(identity, newOfficeScene);

            // Tell client to additively load the office scene
            NetworkConnectionToClient conn = identity.connectionToClient;
            conn.Send(new SceneMessage { sceneName = officeSceneName, sceneOperation = SceneOperation.LoadAdditive });
        
            // Move player to office on server
            SceneManager.MoveGameObjectToScene(player, newOfficeScene);
            PlayerInstanceManager targetPlayerInstanceManager = player.GetComponent<PlayerInstanceManager>();
            targetPlayerInstanceManager.SetCurrentSceneName(officeSceneName);
            targetPlayerInstanceManager.TargetSetCurrentSceneName(conn, officeSceneName);
            targetPlayerInstanceManager.MoveToStartPosition();
            
        }

        // Get a starting Transform for offices
        public Transform GetOfficeStartPosition()
        {
            // first remove any dead transforms
            officeStartPositions.RemoveAll(t => t == null);

            if (officeStartPositions.Count == 0)
                return null;

            return officeStartPositions[UnityEngine.Random.Range(0, officeStartPositions.Count)];
        }

        // If user was in an office scene, remove the office scene from openOfficeScenes and openScenesByNetworkIdentityDictionary
        private void UnloadPlayerOfficeSceneIfOpen(NetworkConnectionToClient conn) {
            Scene userOpenOfficeScene;
            if (openScenesByNetworkIdentityDictionary.TryGetValue(conn.identity, out userOpenOfficeScene)) {
                openScenesByNetworkIdentityDictionary.Remove(conn.identity);
                openOfficeScenes.Remove(userOpenOfficeScene);
                StartCoroutine(UnloadScene(userOpenOfficeScene));
            }
        }

        // Registers a given Transform as an office starting position
        public static void RegisterOfficeStartPosition(Transform transform) {
            officeStartPositions.Add(transform);
        }

        // Unregisters a given Transform as an office starting position
        public static void UnregisterOfficeStartPosition(Transform transform) {
            officeStartPositions.Remove(transform);
        }

    #endregion
    
    #region Counselor

    public void ServerMovePlayerToPlayerScene(NetworkIdentity targetIdentity, NetworkIdentity sourceIdentity) {
        Player sourcePlayer = sourceIdentity.GetComponent<Player>();
        Player targetPlayer = targetIdentity.GetComponent<Player>();

        Scene destinationScene = sourcePlayer.gameObject.scene;

        // Tell client to additively load the destination scene
        NetworkConnectionToClient targetConn = targetPlayer.connectionToClient;
        targetConn.Send(new SceneMessage { sceneName = destinationScene.name, sceneOperation = SceneOperation.LoadAdditive });

        // Move player to correct scene on server
        GameObject targetPlayerObject = targetPlayer.gameObject;
        SceneManager.MoveGameObjectToScene(targetPlayerObject, destinationScene);
        PlayerInstanceManager targetPlayerInstanceManager = targetPlayer.GetComponent<PlayerInstanceManager>();
        targetPlayerInstanceManager.TargetSetCurrentSceneName(targetConn, destinationScene.path);
        targetPlayerInstanceManager.TargetMoveToStartPosition(targetConn);
    }

    #endregion

    #region Scene Management

    public Transform GetStartingPositionForScene(string sceneName) {
        if (sceneName == waitingRoomSceneName) {
            return GetStartPosition();
        }

        if (sceneName == officeSceneName) {
            return GetOfficeStartPosition();
        }

        return null;
    }
    
    // Unload the subScenes and unused assets and clear the subScenes list.
    IEnumerator ServerUnloadSubScenes()
    {
        yield return SceneManager.UnloadSceneAsync(waitingRoomScene);


        foreach (var openOfficeScene in openOfficeScenes)
            yield return SceneManager.UnloadSceneAsync(openOfficeScene);

        openOfficeScenes.Clear();
        openScenesByNetworkIdentityDictionary.Clear();

        yield return Resources.UnloadUnusedAssets();

    }

    // Unload all but the active scene, which is the "container" scene
    IEnumerator ClientUnloadSubScenes()
    {
        for (int index = 1; index < SceneManager.sceneCount; index++)
        {
            if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
        }
    }


    // Unload the given scene
    IEnumerator UnloadScene(Scene scene) {
        yield return SceneManager.UnloadSceneAsync(scene);
    }

    #endregion

    #region Idling Functions

    // Waits until the connection has an identity field. Call from an asynch method. 
    private Task Idle(NetworkConnection conn)
    {
        return Task.Factory.StartNew(() =>
        {
            while (conn.identity == null) {; }
        });
    }

    private Task IdleForAccount(NetworkConnection conn)
    {
        return Task.Factory.StartNew(() =>
        {
            while (conn.identity == null) {; }
        });
    }

    private static Task IdleForClient()
    {
        return Task.Factory.StartNew(() =>
        {
            while (!NetworkClient.connection.isReady) {; }
        });
    }


    #endregion

    #region NetworkManager Script Template Functions

        #region Unity Callbacks

        public override void OnValidate()
        {
            base.OnValidate();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Start()
        {
            singleton = this;
            base.Start();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// </summary>
        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region Start & Stop

        /// <summary>
        /// Set the frame rate for a headless server.
        /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
        /// </summary>
        public override void ConfigureHeadlessFrameRate()
        {
            base.ConfigureHeadlessFrameRate();
        }

        /// <summary>
        /// called when quitting the application by closing the window / pressing stop in the editor
        /// </summary>
        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// This causes the server to switch scenes and sets the networkSceneName.
        /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
        /// </summary>
        /// <param name="newSceneName"></param>
        public override void ServerChangeScene(string newSceneName)
        {
            base.ServerChangeScene(newSceneName);
        }

        /// <summary>
        /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        public override void OnServerChangeScene(string newSceneName) { }

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        /// </summary>
        /// <param name="sceneName">The name of the new scene.</param>
        public override void OnServerSceneChanged(string sceneName) { }

        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) {
            if (NetworkClient.localPlayer == null) return;

            NetworkClient.localPlayer.GetComponent<PlayerInstanceManager>().SetCurrentSceneName(newSceneName);
        }

        /// <summary>
        /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
        /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
            if (NetworkClient.localPlayer == null) return;
            NetworkClient.localPlayer.GetComponent<PlayerInstanceManager>().MoveToStartPosition();
        }

        // support additive scene loads:
        //   NetworkScenePostProcess disables all scene objects on load, and
        //   * NetworkServer.SpawnObjects enables them again on the server when
        //     calling OnStartServer
        //   * NetworkClient.PrepareToSpawnSceneObjects enables them again on the
        //     client after the server sends ObjectSpawnStartedMessage to client
        //     in SpawnObserversForConnection. this is only called when the
        //     client joins, so we need to rebuild scene objects manually again
        // TODO merge this with FinishLoadScene()?
        public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive)
            {
                if (NetworkServer.active)
                {
                    // This line of code was modified for new function
                    NetworkServer.SpawnObjectsInScene(scene);
                    // Debug.Log($"Respawned Server objects after additive scene load: {scene.name}");
                }
                if (NetworkClient.active)
                {
                    NetworkClient.PrepareToSpawnSceneObjects();
                    // Debug.Log($"Rebuild Client spawnableObjects after additive scene load: {scene.name}");
                }
            }
        }

        #endregion

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a new client connects.
        /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerConnect(NetworkConnectionToClient conn) { }

        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
        }

        /// <summary>
        /// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            StartCoroutine(OnServerAddPlayerDelayed(conn));
        }

        // This delay is mostly for the host player that loads too fast for the
        // server to have subscenes async loaded from OnStartServer ahead of it.
        IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn)
        {
            // wait for server to async load all subscenes for game instances
            while (!waitingRoomLoaded)
                yield return null;

            // Send Scene message to client to additively load the game scene
            conn.Send(new SceneMessage { sceneName = waitingRoomSceneName, sceneOperation = SceneOperation.LoadAdditive });

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            Transform startPos = GetStartPosition();
            GameObject playerObject = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            Player player = playerObject.GetComponent<Player>();
            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            playerObject.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, playerObject);

            NetworkIdentity identity = conn.identity;

            // If user is a counselor, put them in an office instance. 
            // if (isCounselor) {
            //     player.GetComponent<PlayerInstanceManager>().CreateAndMoveToOffice();
            // }

            // // Otherwise, put the user in the waiting room
            // else {
            SceneManager.MoveGameObjectToScene(identity.gameObject, waitingRoomScene);
            PlayerInstanceManager targetPlayerInstanceManager = playerObject.GetComponent<PlayerInstanceManager>();
            targetPlayerInstanceManager.currentSceneName = waitingRoomSceneName;
            // }

        }

        /// <summary>
        /// Called on the server when a client disconnects.
        /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            UnloadPlayerOfficeSceneIfOpen(conn);
            base.OnServerDisconnect(conn);
        }

        /// <summary>
        /// Called on server when transport raises an exception.
        /// <para>NetworkConnection may be null.</para>
        /// </summary>
        /// <param name="conn">Connection of the client...may be null</param>
        /// <param name="exception">Exception thrown from the Transport.</param>
        public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message) { }

        #endregion

        #region Client System Callbacks

        /// <summary>
        /// Called on the client when connected to a server.
        /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
        /// </summary>
        public override void OnClientConnect()
        {
            base.OnClientConnect();
        }

        /// <summary>
        /// Called on clients when disconnected from a server.
        /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
        /// </summary>
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
        }

        /// <summary>
        /// Called on clients when a servers tells the client it is no longer ready.
        /// <para>This is commonly used when switching scenes.</para>
        /// </summary>
        public override void OnClientNotReady() { }

        /// <summary>
        /// Called on client when transport raises an exception.</summary>
        /// </summary>
        /// <param name="exception">Exception thrown from the Transport.</param>
        public override void OnClientError(TransportError transportError, string message) { }

        #endregion

        #region Start & Stop Callbacks

        // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
        // their functionality, users would need override all the versions. Instead these callbacks are invoked
        // from all versions, so users only need to implement this one case.

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartHost() { }

        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartServer()
        {
            StartCoroutine(ServerInitializeWaitingRoom());
            Database.manager = this;
            Database.Connect();
            NetworkServer.RegisterHandler<AddAccountMessage>(HandleAccountMessage, false);
        }   

        /// <summary>
        /// This is invoked when the client is started.
        /// </summary>
        public override void OnStartClient() { }

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        public override void OnStopHost() { }

        /// <summary>
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        public override void OnStopServer()
        {
            NetworkServer.SendToAll(new SceneMessage { sceneName = waitingRoomSceneName, sceneOperation = SceneOperation.UnloadAdditive });
            NetworkServer.SendToAll(new SceneMessage { sceneName = officeSceneName, sceneOperation = SceneOperation.UnloadAdditive });
            StartCoroutine(ServerUnloadSubScenes());
        }

        /// <summary>
        /// This is called when a client is stopped.
        /// </summary>
        public override void OnStopClient()
        {
            // make sure we're not in host mode
            if (mode == NetworkManagerMode.ClientOnly)
                StartCoroutine(ClientUnloadSubScenes());

        }

        #endregion 

    #endregion
}