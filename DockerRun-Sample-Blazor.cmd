@echo off
start cmd /C timeout 3 ^& start http://localhost:5005/"
docker-compose run sample_blazor
