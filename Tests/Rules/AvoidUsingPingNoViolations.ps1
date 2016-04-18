$ipAddress = “127.0.0.1”

$pingMethod = new-object system.net.networkinformation.ping
$null = $pingMethod.send($ipAddress)

$null = Test-Connection $ipAddress

$null = Test-NetConnection $ipAddress 

