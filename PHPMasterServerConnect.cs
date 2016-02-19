using UnityEngine;
using System.Collections;

public class PHPMasterServerConnect : MonoBehaviour {
	
	public string masterServerURL = "";
	public string gameType = "";
    [HideInInspector]
	public string gameName = "";
    [HideInInspector]
	public string comment = "";
	public float delayBetweenUpdates = 10.0f;
	private HostData[] hostData = null;
	public int maxRetries = 3;
	private int retries = 0;
	
	static private PHPMasterServerConnect _instance = null;
	static public PHPMasterServerConnect instance {
		get {
			if (_instance == null) {
				Debug.LogError ("No PHPMasterServerConnect in the scene");
			}
			return _instance;
		}
	}
 
	
	void Awake () {
        Object[] objs = FindObjectsOfType (typeof(PHPMasterServerConnect));
        if (objs.Length > 1) {
            Destroy (gameObject);
        } else {
		    DontDestroyOnLoad (this);
        }

        if (_instance != null) {
            Debug.LogError ("Instance should be null");
        }
        _instance = this;
	}
	
	public HostData[] PollHostList()
	{
		return hostData;
	}

	public void QueryPHPMasterServer (string type)
	{
		StartCoroutine (QueryPHPMasterServerCR (type));
	}
	
	private IEnumerator QueryPHPMasterServerCR (string type)
	{
		string url = masterServerURL+"QueryMS.php?gameType="+WWW.EscapeURL(type);
    	Debug.Log ("looking for URL " + url);
    	WWW www = new WWW (url);
    	yield return www;

    	retries = 0;
	    while (www.error != null && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if (www.error != null) {
	        SendMessage ("OnQueryMasterServerFailed");
	    }
	
	    if (www.text == "") {
	    	hostData = null;
	    }
	    string[] hosts = new string[www.text.Split (";"[0]).Length];
		hosts = www.text.Split (";"[0]);
	    hostData = new HostData[hosts.Length];
	    var index = 0;
	    foreach (string host in hosts) {
	        string[] data = host.Split (","[0]);
	        hostData[index] = new HostData ();
	        hostData[index].ip = new string[1];
	        hostData[index].ip[0] = data[0];
	        hostData[index].port = int.Parse(data[1]);
	        hostData[index].useNat = (data[2] == "1");
	        hostData[index].guid = data[3];
	        hostData[index].gameType = data[4];
	        hostData[index].gameName = data[5];
	        hostData[index].connectedPlayers = int.Parse(data[6]);
	        hostData[index].playerLimit = int.Parse(data[7]);
	        hostData[index].passwordProtected = (data[8] == "1");
	        hostData[index].comment = data[9];
	        index ++;
	    }
	}
	
    IEnumerator RegisterHost () {
	    string url = masterServerURL+"RegisterHost.php";
	    url += "?gameType="+WWW.EscapeURL (gameType);
	    url += "&gameName="+WWW.EscapeURL (gameName);
	    url += "&comment="+WWW.EscapeURL (comment);
	    url += "&useNat="+!Network.HavePublicAddress();
	    url += "&connectedPlayers="+(Network.connections.Length + 1);
	    url += "&playerLimit="+Network.maxConnections;
	    url += "&internalIp="+Network.player.ipAddress;
	    url += "&internalPort="+Network.player.port;
	    url += "&externalIp="+Network.player.externalIP;
	    url += "&externalPort="+Network.player.externalPort;
	    url += "&guid="+Network.player.guid;
	    url += "&passwordProtected="+(Network.incomingPassword != "" ? 1 : 0);
	    Debug.Log (url);
	
	    WWW www = new WWW (url);
	    yield return www;
	
	    retries = 0;
	    while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if ((www.error != null || www.text != "succeeded")) {
	        SendMessage ("OnRegisterHostFailed");
		}
    }

	IEnumerator RegistrationLoop()
	{
		while (Network.isServer) {
            yield return StartCoroutine (RegisterHost());
    		yield return new WaitForSeconds(delayBetweenUpdates);
		}
	}
	
	IEnumerator OnServerInitialized()
	{
		while (Network.player.externalPort == 65535) {
    		yield return new WaitForSeconds(1);
  		}
  		StartCoroutine (RegistrationLoop());
	}
	
	IEnumerator OnPlayerConnected(NetworkPlayer player)
	{
		string url = masterServerURL+"UpdatePlayers.php";
	    url += "?gameType="+WWW.EscapeURL (gameType);
	    url += "&gameName="+WWW.EscapeURL (gameName);
	    url += "&connectedPlayers="+(Network.connections.Length + 1);
	    Debug.Log ("url " + url);
	    WWW www = new WWW (url);
	    yield return www;
	
	    retries = 0;
	    while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if ((www.error != null || www.text != "succeeded")) {
	        SendMessage ("OnUpdatePlayersFailed");
	    }
	}
	
	IEnumerator OnPlayerDisconnected(NetworkPlayer player)
	{
		string url = masterServerURL+"UpdatePlayers.php";
	    url += "?gameType="+WWW.EscapeURL (gameType);
	    url += "&gameName="+WWW.EscapeURL (gameName);
	    url += "&connectedPlayers="+Network.connections.Length;
	    Debug.Log ("url " + url);
	    WWW www = new WWW (url);
	    yield return www;
	
	    retries = 0;
	    while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if ((www.error != null || www.text != "succeeded")) {
	        SendMessage ("OnUpdatePlayersFailed");
	    }
	}
	
	IEnumerator OnDisconnectedFromServer(NetworkDisconnection info)
	{
		if (Network.isServer) {
	        string url = masterServerURL+"UnregisterHost.php";
	        url += "?gameType="+WWW.EscapeURL (gameType);
	        url += "&gameName="+WWW.EscapeURL (gameName);
	        WWW www = new WWW (url);
	        yield return www;
	
	        retries = 0;
	        while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
	            retries ++;
	            www = new WWW (url);
	            yield return www;
	        }
	        if ((www.error != null || www.text != "succeeded")) {
	            SendMessage ("OnUnregisterHostFailed");
	        }
    	}
	}

    void OnDestroy () {
        if (Network.isServer) {
	        string url = masterServerURL+"UnregisterHost.php";
	        url += "?gameType="+WWW.EscapeURL (gameType);
	        url += "&gameName="+WWW.EscapeURL (gameName);
	        new WWW (url);
        }
    }
	
	public void SetComment(string text)
	{
		comment = text; 
	}
}
