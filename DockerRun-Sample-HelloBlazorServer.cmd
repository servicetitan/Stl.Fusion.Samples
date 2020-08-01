@echo off

docker-compose build sample_hello_blazor_server
start "HelloBlazorServer Sample (Docker)" docker-compose up sample_hello_blazor_server
timeout 3
start http://localhost:5000/
