# Instrucciones simples de ejecución

En la terminal Git ejecutar el comando Bash

```bash
git clone <url-del-repositorio-que-esta-en-el-correo>
cd BankingPlatform

dotnet restore
dotnet run --project src/Banking.Api
```
# Las pruebas se ejecutan con el comando:

```bash
dotnet test
```

Una ves compilado La API queda en `http://localhost:5080`. La base de datos SQLite (`banking.db`) se crea automáticamente
en el primer arranque, junto con el índice único de `AccountNumber`.
En Development el documento OpenAPI está en `http://localhost:5080/openapi/v1.json`.

# Ejemplos utilizables

```bash
curl -X POST http://localhost:5080/api/customers -H "Content-Type: application/json" \
  -d '{"fullName":"María López","dateOfBirth":"1990-05-20","gender":"Female","monthlyIncome":2500.50}'

curl -X POST http://localhost:5080/api/accounts -H "Content-Type: application/json" \
  -d '{"customerId":"<id-del-cliente>","initialBalance":100}'

curl -X POST http://localhost:5080/api/accounts/ACC-20260618-1234/withdrawals \
  -H "Content-Type: application/json" -d '{"amount":30}'
```
# Puede usar los ejemplos dados para crear mas usuarios de clientes, solo hay que cambiar los datos
