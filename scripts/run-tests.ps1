param(
	[switch]$UseTcpDaemon,
	[switch]$DisableRyuk,
	[switch]$KillTestHost,
	[switch]$UseTestcontainers
)

# Run integration tests with Testcontainers optional configuration

if ($UseTcpDaemon) {
	Write-Host "Enabling DOCKER_HOST to tcp://localhost:2375"
	$env:DOCKER_HOST = 'tcp://localhost:2375'
}

if ($DisableRyuk) {
	Write-Host "Disabling Testcontainers Ryuk (manual cleanup may be required)"
	$env:TESTCONTAINERS_RYUK_DISABLED = 'true'
}
if ($KillTestHost) {
	Write-Host "Killing lingering testhost processes..."
	try {
		$procs = Get-Process -Name 'testhost*' -ErrorAction SilentlyContinue
		if ($procs) {
			$procs | ForEach-Object {
				Write-Host "Stopping process Id=$($_.Id) Name=$($_.ProcessName)"
				Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
			}
			Start-Sleep -Seconds 1
		} else {
			Write-Host "No testhost processes found."
		}
	} catch {
		Write-Host "Failed to stop testhost processes: $($_.Exception.Message)"
	}
}

if ($UseTestcontainers) {
	Write-Host "Running tests with TESTCONTAINERS symbol via MSBuild property..."
	dotnet test Tests\Tests.csproj -p:UseTestcontainers=true
} else {
	Write-Host "Running tests without Testcontainers..."
	dotnet test Tests\Tests.csproj
}

if ($DisableRyuk) {
	Write-Host "Remember to cleanup containers named 'desafio-test-sql' if any remain."
}
