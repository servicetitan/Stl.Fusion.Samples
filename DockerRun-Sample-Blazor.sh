#!/bin/bash
(sleep 3; xdg-open http://localhost:5005/) &
docker-compose run sample_blazor
