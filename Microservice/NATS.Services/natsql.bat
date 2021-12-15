:: Windows Service Manager: https://nssm.cc/download  https://nssm.cc/commands
:: nssm install natsql-test C:/wax/nats/subscribe/natsql.exe -i 5 -c test.yaml
:: nssm set natsql-test AppDirectory C:/wax/nats/subscribe
:: nssm start natsql-test

natsql -i 5 -c natsql.yaml

cd C:\Users\Administrator\source\repos\ConsoleApp\ConsoleApp\bin
ConsoleApp.exe 4 1000000 natsql.test-001
