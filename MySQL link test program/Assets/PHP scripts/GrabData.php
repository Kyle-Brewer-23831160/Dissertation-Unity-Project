<?php
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "userinformation";

$username1 = $_POST["username1"];
$username2 = $_POST["username2"];

// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);
// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

$sql = "SELECT Rank, username FROM users WHERE username = '$username1' OR username = '$username2' " ;

// Execute the SQL query
$result = $conn->query($sql);

// Process the result set
if ($result->num_rows > 0) 
{
  // Output data of each row
  while($row = $result->fetch_assoc()) 
  {
    echo $row["username"]. "/";
    echo $row["Rank"]. "/";
  }
} 
else { echo "0 results"; }

$conn->close();
?>