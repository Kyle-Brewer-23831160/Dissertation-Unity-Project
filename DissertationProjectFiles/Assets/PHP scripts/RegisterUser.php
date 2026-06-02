<?php
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "userinformation";

//Variables set my user
$userlogin = $_POST["userlogin"];
$userpassword = $_POST["userpassword"];
$useremail = $_POST["useremail"];
$userLevel = $_POST["userlevel"];
$userKills = $_POST["userkills"];
$userDeaths = $_POST["userdeaths"];
$userRank = $_POST["userrank"];

// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);

// Check connection
if ($conn->connect_error) {
  die("Connection failed: " . $conn->connect_error);
}

$sql = "SELECT username FROM users WHERE username = '". $userlogin . "'";

$result = $conn->query($sql);

if($result->num_rows > 0){
    echo("username is already in use!");
  }
  else
  {
     $sql2 = "INSERT INTO  users (username, password, email, Level, kills, Deaths, Rank) VALUES ('$userlogin', '$userpassword', '$useremail', '$userLevel', '$userKills', '$userDeaths', '$userRank')";
     
     if($conn -> query($sql2) == TRUE)
      {
        echo("New user succesfully recorded");
      }
      else
      {
        echo("Error:" . $sql2 . "<br>" . $conn->error);
      }
  }

$conn->close();
?>