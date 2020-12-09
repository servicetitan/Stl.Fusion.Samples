@echo off
start cmd /C timeout 3 ^& start https://localhost:50005/README.md"
docker-compose run --service-ports tutorial
