var portString = "12345";

private var showMenu = 0;
private var lastLevelPrefix = 0;
private var singlePlayerLevel = 1;
private var networkPlayerLevel = 2;

private var connectionError : NetworkConnectionError;
private var masterserverError : String = "";

function Awake () {
    var objs = GameObject.FindObjectsOfType (Sample_MainMenu);
    if (objs.length > 1) {
        for (var obj in objs) {
            if (obj != this.gameObject) {
                obj.GetComponent(Sample_MainMenu).ResetMenu ();
            }
        }
        Destroy (this.gameObject);
        return;
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
    } else if (showMenu == 3) {
        DisplayConnectionAttempt ();
    } else if (showMenu == 4) {
        DisplayConnectionFailed ();
    } else if (showMenu == 5) {
        DisplayCrazyErrorState ();
    } else if (showMenu == 6) {
        DisplayMasterServerFailed ();
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
        Network.InitializeServer (32, parseInt(portString),
            !Network.HavePublicAddress ());
        MasterServer.RegisterHost (conn.gameType, conn.gameName, "");
        networkView.RPC ("LoadLevel", RPCMode.AllBuffered, 
                networkPlayerLevel, lastLevelPrefix + 1);
    }
}

function DisplayConnectionAttempt () {
    GUILayout.BeginArea (Rect (100, 100, 600, 400), "", "Box"); 
        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
            GUILayout.BeginVertical ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Attemping to connect...");
            GUILayout.Space (10);
            if (GUILayout.Button ("Cancel")) {
                Network.Disconnect ();
                showMenu = 0;
            }
            GUILayout.FlexibleSpace ();
            GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
    GUILayout.EndArea ();
}

function DisplayMasterServerFailed () {
    GUILayout.BeginArea (Rect (100, 100, 600, 400), "", "Box"); 
        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
            GUILayout.BeginVertical ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Error with masterserver: " + masterserverError);
            GUILayout.Space (10);
            if (GUILayout.Button ("Okay")) {
                showMenu = 0;
            }
            GUILayout.FlexibleSpace ();
            GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
    GUILayout.EndArea ();
}

function DisplayCrazyErrorState () {
    GUILayout.BeginArea (Rect (100, 100, 600, 400), "", "Box"); 
        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
            GUILayout.BeginVertical ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Mysterious Network Error...");
            GUILayout.Space (10);
            if (GUILayout.Button ("Cancel")) {
                Network.Disconnect ();
                showMenu = 0;
            }
            GUILayout.FlexibleSpace ();
            GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
    GUILayout.EndArea ();
}

function DisplayConnectionFailed () {
    GUILayout.BeginArea (Rect (100, 100, 600, 400), "", "Box"); 
        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
            GUILayout.BeginVertical ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Error connecting: " + connectionError);
            GUILayout.Space (10);
            if (GUILayout.Button ("Okay")) {
                showMenu = 0;
            }
            GUILayout.FlexibleSpace ();
            GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
    GUILayout.EndArea ();
}

@RPC
function LoadLevel (level : int, levelPrefix : int) {
    DisableMenu ();
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
                showMenu = 3;
                Network.Connect (element.guid);
            }
            GUILayout.EndHorizontal ();
        }
    } else {
        GUILayout.Label ("No games currently available");
    }
    GUILayout.EndScrollView ();
}

function OnFailedToConnect (error : NetworkConnectionError) {
    connectionError = error;
    showMenu = 4;
}

function OnFailedToConnectToMasterServer () {
    // Shouldn't happen we're not using the Unity masterserver
    showMenu = 5;
}

function OnQueryMasterServerFailed () {
    masterserverError = "Failed to query masterserver";
    showMenu = 6;
}

function OnUpdateHostFailed () {
    masterserverError = "Failed to update host on masterserver";
    showMenu = 6;
}

function OnRegisterHostFailed () {
    masterserverError = "Failed to register host on masterserver";
    showMenu = 6;
}

function OnUpdatePlayersFailed () {
    masterserverError = "Failed to update masterserver";
    showMenu = 6;
}

function OnUnregisterHostFailed () {
    masterserverError = "Failed to unregister masterserver";
    showMenu = 6;
}


