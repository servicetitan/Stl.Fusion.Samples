#!/bin/bash
(sleep 3; xdg-open https://localhost:50005/README.md) &
docker-compose run --service-ports tutorial
