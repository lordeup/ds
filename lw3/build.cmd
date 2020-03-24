docker build -t joblogger:%1 -f Dockerfile.joblogger .
docker build -t backendapi:%1 -f Dockerfile.backendapi .
docker build -t frontend:%1 -f Dockerfile.frontend .

MKDIR app%1

COPY start.cmd app%1
COPY stop.cmd app%1
COPY config.cmd app%1