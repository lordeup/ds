CALL config.cmd
docker run --detach --rm --publish %BACKEND_API_PORT%:5000 --name backendapi backendapi
docker run --detach --rm --env BACKEND_HOST=%BACKEND_HOST% --link %BACKEND_HOST% --publish %FRONTEND_PORT%:80 --name frontend frontend