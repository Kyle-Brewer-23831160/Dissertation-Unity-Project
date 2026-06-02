<?php
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "userinformation";

// Create connection
$conn = new mysqli($servername, $username, $password);

// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

$sql = "CREATE DATABASE IF NOT EXISTS `$dbname` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";

$result = $conn->query($sql);

if($result == true)
  {
    echo("Sucessfully created Database ");
  }
  else
  {
    echo("Failed to create Database ");
  }

$conn->close();

// Create new connection to database
$conn = new mysqli($servername, $username, $password, $dbname);

// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

$sql = "CREATE TABLE IF NOT EXISTS `users`  (
id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
username VARCHAR(100) NOT NULL,
password VARCHAR(100) NOT NULL,
email VARCHAR(100) NOT NULL,
Level INT(10) NOT NULL,
kills INT(10) NOT NULL,
Deaths INT(10) NOT NULL,
Rank INT(10) NOT NULL) ENGINE=InnoDB";

$result = $conn->query($sql);

if($result == true)
  {
    echo("Sucessfully created Table ");
  }
  else
  {
    echo("Failed to create Table ");
  }

$conn->close();
?>