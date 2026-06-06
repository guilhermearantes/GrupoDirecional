param(
	[string]$Image = 'mcr.microsoft.com/mssql/server:2019-latest',
	[string]$ContainerName = 'desafio-test-sql',
	[string]$SaPassword = $env:SA_PASSWORD ?? 'Your_password123',
	[int]$HostPort = 14333,
	[int]$TimeoutSeconds = 180
)

Write-Host "Pulling image $Image..."
docker pull $Image | Out-Null

Write-Host "Removing any existing container named $ContainerName..."
try { docker rm -f $ContainerName | Out-Null } catch { }

Write-Host "Starting container $ContainerName (host port $HostPort -> container 1433)..."
docker run -d -p ${HostPort}:1433 -e ACCEPT_EULA=Y -e SA_PASSWORD=$SaPassword --name $ContainerName $Image | Out-Null

Write-Host "Waiting for SQL Server to accept connections (timeout: ${TimeoutSeconds}s)..."
$sw = [Diagnostics.Stopwatch]::StartNew()
while ($sw.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
	try {
		$tcp = New-Object System.Net.Sockets.TcpClient
		$async = $tcp.BeginConnect('127.0.0.1', $HostPort, $null, $null)
		$success = $async.AsyncWaitHandle.WaitOne(2000)
		if ($success -and $tcp.Connected) {
			$tcp.EndConnect($async)
			$tcp.Close()
			Write-Host "TCP port $HostPort is open. SQL Server may be starting; wait a bit for readiness."
			break
		}
		$tcp.Close()
	} catch {
		# ignore
	}
	Start-Sleep -Seconds 2
}

Write-Host "Container started. Check readiness by running 'docker logs $ContainerName' if needed."
Write-Host "Use scripts\run-tests.ps1 -UseTcpDaemon -KillTestHost to run tests with Testcontainers enabled."
