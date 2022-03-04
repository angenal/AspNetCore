:: Windows Service Manager: https://nssm.cc/download  https://nssm.cc/commands
:: nssm install natsql-test C:/nats/services/NATS.Services.exe -i 5 -c test.yaml
:: nssm set natsql-test AppDirectory C:/nats/services
:: nssm start natsql-test

NATS.Services.exe -i 5 -c natsql.yaml

cd C:/nats/services
NATS.Services.exe 1 natsql.test-001
