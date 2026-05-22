<?php
error_reporting(0);
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

$sql = "SELECT username FROM users";

// Execute the SQL query
$result = $conn->query($sql);

// Process the result set
if ($result->num_rows > 0) 
{
  // Output data of each row
  while($row = $result->fetch_assoc()) 
  {
    echo $row["username"]. "/";
  }
} 

$conn->close();
?>