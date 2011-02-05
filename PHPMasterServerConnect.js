public var masterServerURL : String;
public var gameType : String;
@HideInInspector
public var gameName : String;
@HideInInspector
public var comment : String = "";
public var delayBetweenUpdates : float = 10.0;
private var hostData : HostData[] = null;
public var maxRetries : int = 3;
private var retries :int = 0;


function Awake () {
	var gos = FindObjectsOfType (PHPMasterServerConnect);
  	if (gos.Length > 1) {
		Destroy (gameObject);
	} else {
		DontDestroyOnLoad (this);
	}
}

public function PollHostList () : HostData[] {
	return hostData;
}

public function QueryPHPMasterServer (type : String) {
    url = masterServerURL+"QueryMS.php?gameType="+WWW.EscapeURL(type);
    Debug.Log ("looking for URL " + url);
    www = new WWW (url);
    yield www;

    retries = 0;
    while (www.error != null && retries < maxRetries) {
        retries ++;
        www = new WWW (url);
        yield www;
    }
    if (www.error != null) {
        SendMessage ("OnQueryMasterServerFailed");
        return;
    }

    if (www.text == "") {
    	hostData = null;
        return;
    }
    hosts = www.text.Split (";"[0]);
    hostData = new HostData[hosts.length];
    var index = 0;
    for (var host in hosts) {
        data = host.Split (","[0]);
        hostData[index] = new HostData ();
        hostData[index].ip = new String[1];
        hostData[index].ip[0] = data[0];
        hostData[index].port = parseInt(data[1]);
        hostData[index].useNat = (data[2] == "1");
        hostData[index].guid = data[3];
        hostData[index].gameType = data[4];
        hostData[index].gameName = data[5];
        hostData[index].connectedPlayers = parseInt(data[6]);
        hostData[index].playerLimit = parseInt(data[7]);
        hostData[index].passwordProtected = (data[8] == "1");
        hostData[index].comment = data[9];
        index ++;
    }
}

function RegistrationLoop() {
  while (Network.isServer) {
    yield WaitForSeconds(delayBetweenUpdates);

    var url = masterServerURL+"RegisterHost.php";
    url += "?gameType="+WWW.EscapeURL (gameType);
    url += "&gameName="+WWW.EscapeURL (gameName);
    url += "&comment="+WWW.EscapeURL (comment);
    url += "&useNat="+!Network.HavePublicAddress();
    url += "&connectedPlayers="+(Network.connections.length + 1);
    url += "&playerLimit="+Network.maxConnections;
    url += "&internalIp="+Network.player.ipAddress;
    url += "&internalPort="+Network.player.port;
    url += "&externalIp="+Network.player.externalIP;
    url += "&externalPort="+Network.player.externalPort;
    url += "&guid="+Network.player.guid;
    url += "&passwordProtected="+(Network.incomingPassword != "" ? 1 : 0);
    Debug.Log (url);

    var www = new WWW (url);
    yield www;

    retries = 0;
    while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
        retries ++;
        www = new WWW (url);
        yield www;
    }
    if ((www.error != null || www.text != "succeeded")) {
        SendMessage ("OnUpdateHostFailed");
        return;
    }
  }
}

function OnServerInitialized () {
  while (Network.player.externalPort == 65535) {
    yield WaitForSeconds(1);
  }
  var url = masterServerURL+"RegisterHost.php";
  url += "?gameType="+WWW.EscapeURL (gameType);
  url += "&gameName="+WWW.EscapeURL (gameName);
  url += "&comment="+WWW.EscapeURL (comment);
  url += "&useNat="+!Network.HavePublicAddress();
  url += "&connectedPlayers="+(Network.connections.length + 1);
  url += "&playerLimit="+Network.maxConnections;
  url += "&internalIp="+Network.player.ipAddress;
  url += "&internalPort="+Network.player.port;
  url += "&externalIp="+Network.player.externalIP;
  url += "&externalPort="+Network.player.externalPort;
  url += "&guid="+Network.player.guid;
  url += "&passwordProtected="+(Network.incomingPassword != "" ? 1 : 0);
  Debug.Log (url);
  var www = new WWW (url);
  yield www;

  retries = 0;
  while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
      retries ++;
      www = new WWW (url);
      yield www;
  }
  if ((www.error != null || www.text != "succeeded")) {
      SendMessage ("OnRegisterHostFailed");
      return;
  }

  StartCoroutine (RegistrationLoop());
}

function OnPlayerConnected (player : NetworkPlayer) {
    var url = masterServerURL+"UpdatePlayers.php";
    url += "?gameType="+WWW.EscapeURL (gameType);
    url += "&gameName="+WWW.EscapeURL (gameName);
    url += "&connectedPlayers="+(Network.connections.length + 1);
    Debug.Log ("url " + url);
    var www = new WWW (url);
    yield www;

    retries = 0;
    while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
        retries ++;
        www = new WWW (url);
        yield www;
    }
    if ((www.error != null || www.text != "succeeded")) {
        SendMessage ("OnUpdatePlayersFailed");
        return;
    }
}

function OnPlayerDisconnected (player : NetworkPlayer) {
    var url = masterServerURL+"UpdatePlayers.php";
    url += "?gameType="+WWW.EscapeURL (gameType);
    url += "&gameName="+WWW.EscapeURL (gameName);
    url += "&connectedPlayers="+Network.connections.length;
    Debug.Log ("url " + url);
    var www = new WWW (url);
    yield www;

    retries = 0;
    while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
        retries ++;
        www = new WWW (url);
        yield www;
    }
    if ((www.error != null || www.text != "succeeded")) {
        SendMessage ("OnUpdatePlayersFailed");
        return;
    }
}

function OnDisconnectedFromServer(info : NetworkDisconnection) {
    if (Network.isServer) {
        var url = masterServerURL+"UnregisterHost.php";
        url += "?gameType="+WWW.EscapeURL (gameType);
        url += "&gameName="+WWW.EscapeURL (gameName);
        var www = new WWW (url);
        yield www;

        retries = 0;
        while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
            retries ++;
            www = new WWW (url);
            yield www;
        }
        if ((www.error != null || www.text != "succeeded")) {
            SendMessage ("OnUnregisterHostFailed");
            return;
        }
    }
}

function SetComment (text : String) {
    comment = text; 
}
