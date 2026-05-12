<?php
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "userinformation";

$userindex = $_POST["index"];


// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);
// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

$sql = "SELECT Rank, username FROM users WHERE id = '$userindex'";

// Execute the SQL query
$result = $conn->query($sql);

// Process the result set
if ($result->num_rows > 0) 
{
  // Output data of each row
  while($row = $result->fetch_assoc()) 
  {
    echo $row["username"];
  }
} 
else { echo "0 results"; }

$conn->close();
?>