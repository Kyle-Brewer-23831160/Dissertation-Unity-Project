<?php
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "userinformation";

// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);

// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

$sql = "TRUNCATE TABLE  users";

$result = $conn->query($sql);

if($result == false){
    echo("Failed to truncate ");
  }
  else
  {
    echo("Sucessfully truncated table ");
  }

$conn->close();
?>