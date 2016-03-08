<?php
    require 'sql.conf.php';
    $db = mysql_connect ($sqlc["server"], $sqlc["user"], $sqlc["pass"]);
    if (!mysql_select_db ($sqlc["db"])) {
        echo "Could not select database " . mysql_error ();
        exit;
    }
    $query = "DELETE FROM MasterServer WHERE gameType='"
        .mysql_real_escape_string($_REQUEST['gameType'])
        ."' AND gameName='".mysql_real_escape_string($_REQUEST['gameName'])
        ."' AND externalIP='".$_SERVER['REMOTE_ADDR']
        ."';";
    $res = mysql_query ($query);
    if (!$res) {
    	echo "Could not execute query: " . mysql_error ();
    	exit;
    }
    mysql_close ($db);
    print "succeeded";
?>
