using UnityEngine;
using System.Collections;

public class PHPMasterServerConnect : MonoBehaviour {
	
	public string masterServerURL = "";
	public string gameType = "";
	public string gameName = "";
	public string comment = "";
	public float delayBetweenUpdates = 10.0f;
	private HostData[] hostData = null;
	public int maxRetries = 3;
	private int retries = 0;
	
	
	void Start () {
		DontDestroyOnLoad (this);
	}
	
	public HostData[] PollHostList()
	{
		return hostData;
	}
	
	public IEnumerator QueryPHPMasterServer(string type)
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
	
	IEnumerator RegistrationLoop()
	{
		while (Network.isServer) {
    		yield return new WaitForSeconds(delayBetweenUpdates);

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
		        SendMessage ("OnUpdateHostFailed");
			}
		}
	}
	
	IEnumerator OnServerInitialized()
	{
		while (Network.player.externalPort == 65535) {
    		yield return new WaitForSeconds(1);
  		}
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
	
	public void SetComment(string text)
	{
		comment = text; 
	}
}
