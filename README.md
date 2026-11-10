# Instrucciones simples de ejecución

En la terminal Git ejecutar el comando Bash

```
git clone <url-del-repositorio-que-esta-en-el-correo>
cd BankingPlatform

dotnet restore
dotnet run --project src/Banking.Api
```
# Las pruebas se ejecutan con el comando:

```
dotnet test
```

Una ves compilado La API queda en `http://localhost:5080`. La base de datos SQLite (`banking.db`) se crea automáticamente
en el primer arranque, junto con el índice único de `AccountNumber`.
En Development el documento OpenAPI está en `http://localhost:5080/openapi/v1.json`.
