var portString = "12345";

private var showMenu = 0;
private var lastLevelPrefix = 0;
private var singlePlayerLevel = 1;
private var networkPlayerLevel = 2;

function Awake () {
    var objs = GameObject.FindGameObjectsWithTag ("Main Menu");
    if (objs.length > 1) {
        for (var obj in objs) {
            if (obj != this.gameObject) {
                obj.GetComponent(MainMenu).ResetMenu ();
            }
        }
        Destroy (this.gameObject);
    }
    DontDestroyOnLoad (this);
    networkView.group = 1;
}

function OnGUI () {
    GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
                    Vector3(Screen.width/1024.0, Screen.height/768.0, 1));
    if (showMenu == 0) {
        if (Network.peerType != NetworkPeerType.Disconnected) {
            Network.Disconnect ();
        }
        DisplayTopMenu ();
    } else if (showMenu == 1) {
        DisplayLobbyMenu ();
    } else if (showMenu == 2) {
        DisplayHostMenu ();
    }
}

function ResetMenu () {
    showMenu = 0;
    enabled = true;
}

function DisableMenu () {
    Debug.Log ("Disabling menu");
    showMenu = -1;
    enabled = false;
}

function DisplayTopMenu () {
    if (GUI.Button (Rect (10, 300, 200, 25), "Single Player")) {
        Application.LoadLevel (singlePlayerLevel);
        DisableMenu ();
    }

    if (GUI.Button (Rect (10, 350, 200, 25), "Multiplayer")) {
        showMenu = 1;
    }
}

function DisplayLobbyMenu () {
    if (GUI.Button (Rect (10, 300, 200, 25), "Start as Host")) {
        showMenu = 2;
    }
    if (GUI.Button (Rect (10, 350, 200, 25), "Back")) {
        showMenu = 0;
    }
    GUI.Window (1, Rect (300, 50, 600, 600), JoinableGames, "Join a Game");
}

function DisplayHostMenu () {
    GUI.Label (Rect (10, 300, 200, 25), "Game Name");
    var conn = GetComponent(PHPMasterServerConnect);
    conn.gameName = GUI.TextField (Rect (215, 300, 400, 25), conn.gameName);
    if (GUI.Button (Rect (110, 330, 100, 40), "Cancel")) {
        showMenu = 1;
    }
    if (GUI.Button (Rect (325, 330, 100, 40), "Accept")) {
        DisableMenu ();
        Network.InitializeServer (32, parseInt(portString),
            !Network.HavePublicAddress ());
        MasterServer.RegisterHost (conn.gameType, conn.gameName, "");
        networkView.RPC ("LoadLevel", RPCMode.AllBuffered, 
                networkPlayerLevel, lastLevelPrefix + 1);
    }
}

@RPC
function LoadLevel (level : int, levelPrefix : int) {
    lastLevelPrefix = levelPrefix;
    if (Network.isClient) {
        Network.SetSendingEnabled (0, false);
        Network.isMessageQueueRunning = false;
    }
    Network.SetLevelPrefix (levelPrefix);
    Application.LoadLevel (level);
    yield;
    yield;
    if (Network.isClient) {
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled (0, true);
    }
    Debug.Log ("load level network message sending");
    for (var go in FindObjectsOfType(GameObject)) {
        go.SendMessage ("OnNetworkLevelLoaded",
            SendMessageOptions.DontRequireReceiver);
    }
    Debug.Log ("done sending");
}

public var pollingTime : float = 5;
private var lastRequested : float = 0;
private var scrollPosition : Vector2;
function JoinableGames (id : int) {
    var conn = GetComponent (PHPMasterServerConnect);
    if (Time.realtimeSinceStartup > lastRequested + pollingTime) {
        conn.QueryPHPMasterServer (conn.gameType);
        lastRequested = Time.realtimeSinceStartup;
    }
    var hostData : HostData[] = conn.PollHostList ();
    scrollPosition = GUILayout.BeginScrollView (scrollPosition);
    if (hostData != null && hostData.length > 0) {
        for (var element in hostData) {
            GUILayout.BeginHorizontal ();
            GUILayout.Label (element.gameName, GUILayout.Width (500));
            if (GUILayout.Button ("Join")) {
                Debug.Log ("Trying to connect to " + element.gameName);
                DisableMenu ();
                Network.Connect (element.guid);
            }
            GUILayout.EndHorizontal ();
        }
    } else {
        GUILayout.Label ("No games currently available");
    }
    GUILayout.EndScrollView ();
}
