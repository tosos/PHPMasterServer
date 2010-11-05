<?php
    $db = mysql_connect ("server_name", "user_name", "password");
    if (!mysql_select_db ("database_name")) {
	echo "Could not select database " . mysql_error ();
	exit;
    }
    $query = "UPDATE MasterServer SET updated = NOW() ".
             "WHERE gameType='".$_REQUEST['gameType']."' ".
	     "AND gameName='".$_REQUEST['gameName']."';";
    $res = mysql_query ($query);
    if (!$res) {
	echo "Could not execute query: " . mysql_error ();
	exit;
    }
    mysql_close ($db);
?>
