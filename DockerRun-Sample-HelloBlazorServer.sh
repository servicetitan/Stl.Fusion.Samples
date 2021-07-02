#!/bin/bash
(sleep 3; xdg-open http://localhost:5005/) &
docker-compose run --service-ports sample_hello_blazor_server
