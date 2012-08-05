<?php
require 'sql.conf.php';
$db = mysql_connect ($sqlc["server"], $sqlc["user"], $sqlc["pass"]);
if (!mysql_select_db ($sqlc["db"])) {
	echo "Could not select database " . mysql_error ();
	exit;
}

$query = "DELETE FROM MasterServer WHERE (NOW()-updated) > '30';";
mysql_query ($query);

$query = "SELECT externalIp,externalPort,internalIp,internalPort,useNat,guid,gameType,gameName,connectedPlayers,playerLimit,passwordProtected,comment,NOW()-updated FROM MasterServer WHERE gameType='".$_REQUEST["gameType"]."';";
// echo ($query);
$res = mysql_query ($query);
if (!$res) {
	echo "Could not execute query: " . mysql_error ();
	exit;
}
$rows = mysql_num_rows ($res);
$cols = mysql_num_fields ($res);
$show = 0;
for ($i = 0; $i < $rows; $i ++) {
	$row = mysql_fetch_row ($res);
	if ($show == 1) {
		print ";";
	} else {
		$show = 1;
	}
	if ($row[4] == "1" && $row[0] == $_SERVER['REMOTE_ADDR']) {
		print $row[2].",".$row[3].",0";
	} else {
		print $row[0].",".$row[1].",".$row[4];
	}
	for ($j = 5; $j < $cols; $j ++) {
		print ",".$row[$j];
	}
}
mysql_free_result ($res);
mysql_close ($db);
?>
