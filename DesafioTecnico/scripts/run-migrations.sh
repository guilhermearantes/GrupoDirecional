#!/usr/bin/env bash
set -euo pipefail

# Wait for SQL Server TCP port to be available
host=sqlserver
port=1433
max_attempts=120
attempt=1
while ! bash -c "</dev/tcp/$host/$port" 2>/dev/null; do
  if [ $attempt -ge $max_attempts ]; then
	echo "Timed out waiting for $host:$port" >&2
	exit 1
  fi
  attempt=$((attempt+1))
  sleep 1
done

# Ensure dotnet-ef tool is installed
if ! command -v dotnet-ef >/dev/null 2>&1; then
  dotnet tool install --global dotnet-ef --version 9.0.0 || true
  export PATH="$HOME/.dotnet/tools:$PATH"
fi

# Run migrations
dotnet ef database update --no-build --project DesafioTecnico.csproj
