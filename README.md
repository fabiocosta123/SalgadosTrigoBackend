# Salgados Trigo API

API do sistema da fábrica. O PostgreSQL fica na Neon, na branch `develop`. As tabelas nascem por migrations do Entity Framework Core, uma etapa de cada vez.

## Nomes

O código fica em inglês: classes, tabelas e o nome da migration. O Jira continua em português.

A STRIGO-11 se chama "Criar o banco e a migration de identidade". O teste de aceite ainda diz "A migration Identidade sobe e desce." No código, o arquivo dessa migration se chama `Identity`.

## Segredo da connection string

A senha fica no User Secrets do projeto `SalgadosTrigo.Api`, na chave `ConnectionStrings:Default`. Ela não entra no `appsettings.json` nem no Git.

Para aplicar a migration, a connection string é a **direct** da branch `develop` (o host não tem `-pooler`).

## Migration de identidade

Na pasta `SalgadosTrigo.Api`:

```powershell
dotnet ef migrations add Identity
dotnet ef database update
```

Essa migration cria só `roles` e `users`. `roles` nasce com os cinco códigos: `administrador`, `gerente`, `funcionario`, `vendedor` e `financeiro`. `users` nasce vazia.

## Testes

Na pasta da solução:

```powershell
dotnet test
```

O teste atual confere o modelo do Entity Framework: tabelas, tamanhos, índices únicos, os cinco papéis e a chave estrangeira de `users` para `roles`. Ele não conecta no banco. O teste que sobe e desce a migration entra depois que o arquivo `Identity` existir.
