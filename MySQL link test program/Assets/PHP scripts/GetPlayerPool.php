<?php
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "userinformation";

$username1 = $_POST["username1"];

// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);
// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

$sql = "SELECT username, Level, kills, Deaths, Rank FROM users" ;

// Execute the SQL query
$result = $conn->query($sql);

// Process the result set
if ($result->num_rows > 0) 
{
  // Output data of each row
  while($row = $result->fetch_assoc()) 
  {
    echo $row["username"]. "/";
    echo $row["Level"]. "/";
    echo $row["kills"]. "/";
    echo $row["Deaths"]. "/";
    echo $row["Rank"]. "/";
  }
} 
else { echo "0 results"; }

$conn->close();
?>