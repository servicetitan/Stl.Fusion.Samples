@echo off
docker-compose build

start "Tutorial (Docker)" docker-compose up tutorial
timeout 5
start https://localhost:50005/README.md
