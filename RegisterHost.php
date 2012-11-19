<?php
require 'sql.conf.php';
$db = mysql_connect ($sqlc["server"], $sqlc["user"], $sqlc["pass"]);
if (!mysql_select_db ("database_name")) {
	echo "Could not select database " . mysql_error ();
	exit;
}
$query = "DELETE FROM MasterServer WHERE gameType='".$_REQUEST['gameType']."' AND gameName='".$_REQUEST['gameName']."';";
$res = mysql_query ($query);
$query = "INSERT INTO MasterServer ".
	     "(useNat,gameType,gameName,connectedPlayers,".
    	 "playerLimit,internalIp,internalPort,".
		 "externalIp,externalPort,guid,passwordProtected,comment,updated) VALUES ".
         "(".$_REQUEST['useNat'].",'".$_REQUEST['gameType'].
         "','".$_REQUEST['gameName']."',".$_REQUEST['connectedPlayers'].
         ",".$_REQUEST['playerLimit'].",'".$_REQUEST['internalIp'].
         "',".$_REQUEST['internalPort'].",'".$_REQUEST['externalIp'].
		 "',".$_REQUEST['externalPort'].",'".$_REQUEST['guid']."',".
		 $_REQUEST['passwordProtected'].",'".$_REQUEST['comment']."',NOW());";
$res = mysql_query ($query);
if (!$res) {
	echo "Could not execute query: " . mysql_error ();
	exit;
}
mysql_close ($db);
?>
