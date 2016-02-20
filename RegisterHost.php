<?php
require 'sql.conf.php';
$db = mysql_connect ($sqlc["server"], $sqlc["user"], $sqlc["pass"]);
if (!mysql_select_db ($sqlc["db"])) {
	echo "Could not select database " . mysql_error ();
	exit;
}
$query = "DELETE FROM MasterServer WHERE gameType='".$_REQUEST['gameType']."' AND gameName='".$_REQUEST['gameName']."';";
$res = mysql_query ($query);
$query = "INSERT INTO MasterServer ".
         "(gameType,gameName,connectedPlayers,".
         "playerLimit,internalIp,internalPort,".
         "externalIp,externalPort,comment,updated) VALUES ".
         "('".$_REQUEST['gameType']."','".$_REQUEST['gameName'].
         "',".$_REQUEST['connectedPlayers'].",".$_REQUEST['playerLimit'].
         ",'".$_REQUEST['internalIp']."',".$_REQUEST['internalPort'].
         ",'".$_SERVER['REMOTE_ADDR']."',".$_REQUEST['externalPort'].
         ",'".$_REQUEST['comment']."',NOW());";
$res = mysql_query ($query);
if (!$res) {
	echo "Could not execute query: " . mysql_error ();
	exit;
}
mysql_close ($db);
?>
