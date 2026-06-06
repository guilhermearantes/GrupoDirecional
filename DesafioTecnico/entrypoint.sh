#!/bin/bash
set -e

host="${DB_HOST:-db}"
port="${DB_PORT:-1433}"

echo "Waiting for $host:$port..."
# wait for the TCP port to be open
while ! bash -c "</dev/tcp/$host/$port" 2>/dev/null; do
  sleep 1
done

echo "Database available, starting app"
exec dotnet DesafioTecnico.dll
