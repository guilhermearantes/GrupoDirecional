param(
	[string]$ContainerName = 'desafio-test-sql',
	[switch]$RemoveImage
)

Write-Host "Stopping and removing container '$ContainerName' if exists..."
try {
	$exists = docker ps -a --filter "name=$ContainerName" --format "{{.ID}}" | Select-String -Pattern '.'
	if ($exists) {
		docker rm -f $ContainerName | Out-Null
		Write-Host "Container '$ContainerName' removed."
	} else {
		Write-Host "No container named '$ContainerName' found."
	}
} catch {
	Write-Host "Failed to remove container: $($_.Exception.Message)"
}

if ($RemoveImage) {
	Write-Host "Removing image mcr.microsoft.com/mssql/server:2019-latest (if present)..."
	try {
		docker rmi mcr.microsoft.com/mssql/server:2019-latest -f | Out-Null
		Write-Host "Image removed (if it existed)."
	} catch {
		Write-Host "Failed to remove image: $($_.Exception.Message)"
	}
}

Write-Host "Cleanup complete."
