using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PHPMasterServerConnect : MonoBehaviour 
{
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

	private bool registered = false;
	
	static private PHPMasterServerConnect _instance = null;
	static public PHPMasterServerConnect instance {
		get {
			if (_instance == null) {
				_instance = (PHPMasterServerConnect) FindObjectOfType (typeof(PHPMasterServerConnect));
			}
			return _instance;
		}
	}
	
	void Awake () {
        if (_instance != null) {
            Debug.LogError ("Instance should be null");
            DestroyImmediate (gameObject);
        } else {
		    DontDestroyOnLoad (gameObject);
		}
        _instance = this;
	}

    void OnDestroy () {
        if (registered) {
			// Unregister without the CR
	        string url = masterServerURL+"UnregisterHost.php";
	        url += "?gameType="+WWW.EscapeURL (gameType);
	        url += "&gameName="+WWW.EscapeURL (gameName);
	        new WWW (url);
        }
    }
	
	public HostData[] PollHostList()
	{
		return hostData;
	}

	private bool atOnce = false;
	public void QueryPHPMasterServer ()
	{
		if (!atOnce) 
			StartCoroutine (QueryPHPMasterServerCR ());
	}
	
	private IEnumerator QueryPHPMasterServerCR ()
	{
		atOnce = true;
		string url = masterServerURL+"QueryMS.php?gameType="+WWW.EscapeURL(gameType);
    	// Debug.Log ("looking for URL " + url);
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
			yield break;
		} else if (www.text == "empty") {
			Debug.LogError ("Got an empty result I didn't expect here!");
	    	hostData = null;
	    } else {
	    	string[] hosts = new string[www.text.Split (";"[0]).Length];
			hosts = www.text.Split (";"[0]);
	    	hostData = new HostData[hosts.Length];
	    	var index = 0;
	    	foreach (string host in hosts) {
				if (host == "") continue;
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
		atOnce = false;
	}

	public void RegisterHost (string pGameName, string pComment) {
		gameName = pGameName;
		comment = pComment;
		registered = true;
		StartCoroutine (RegistrationLoop ());
	}

	private IEnumerator RegistrationLoop()
	{
		while (registered && NetworkServer.active) {
            yield return StartCoroutine (RegisterHostCR());
    		yield return new WaitForSeconds(delayBetweenUpdates);
		}

		registered = false;
	}

    private IEnumerator RegisterHostCR () {
	    string url = masterServerURL+"RegisterHost.php";
	    url += "?gameType="+WWW.EscapeURL (gameType);
	    url += "&gameName="+WWW.EscapeURL (gameName);
	    url += "&comment="+WWW.EscapeURL (comment);
		url += "&playerLimit="+NetworkManager.singleton.matchSize;
		url += "&connectedPlayers="+NetworkManager.singleton.numPlayers;
		url += "&internalIp="+NetworkManager.singleton.networkAddress;
		url += "&internalPort="+NetworkManager.singleton.networkPort;
		url += "&externalPort="+NetworkManager.singleton.networkPort;
		if (NetworkManager.singleton.serverBindToIP) {
	    	url += "&externalIp="+NetworkManager.singleton.serverBindAddress;
		} else {
	    	url += "&externalIp="+NetworkManager.singleton.networkAddress;
		}
	    Debug.Log (url);
	
	    WWW www = new WWW (url);
	    yield return www;
	
	    retries = 0;
	    while ((www.error != null || www.text != "") && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if ((www.error != null || www.text != "")) {
	        SendMessage ("OnRegisterHostFailed");
		}
    }

	public void UnregisterHost ()
	{
		StartCoroutine (UnregisterHostCR ());
	}
	
	private IEnumerator UnregisterHostCR ()
	{
		if (registered) {
	        string url = masterServerURL+"UnregisterHost.php";
	        url += "?gameType="+WWW.EscapeURL (gameType);
	        url += "&gameName="+WWW.EscapeURL (gameName);
			Debug.Log (url);
	        WWW www = new WWW (url);
	        yield return www;
	
	        retries = 0;
	        while ((www.error != null || www.text != "") && retries < maxRetries) {
	            retries ++;
	            www = new WWW (url);
	            yield return www;
	        }
	        if ((www.error != null || www.text != "")) {
	            SendMessage ("OnUnregisterHostFailed");
	        }

			registered = false;
    	}
	}
	
	// TODO update for the new networking
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
	
	// TODO update for the new networking
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
}
