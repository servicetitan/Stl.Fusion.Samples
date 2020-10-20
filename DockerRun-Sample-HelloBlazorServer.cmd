@echo off
docker-compose build

start "Samples.HelloBlazorServer (Docker)" docker-compose up sample_hello_blazor_server
timeout 3
start http://localhost:5000/
