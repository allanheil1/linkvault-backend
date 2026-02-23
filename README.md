# LinkVault Backend

API ASP.NET (net9) para o projeto LinkVault.

## Requisitos
- .NET SDK 9.0+
- Docker + Docker Compose (para Postgres)
- (opcional) dotnet-ef instalado globalmente: dotnet tool install -g dotnet-ef

## Subir banco
`ash
docker compose up -d postgres
# opcional: docker compose up -d adminer  # UI em http://localhost:8080
`

## Migrações
`ash
dotnet ef database update --startup-project src/LinkVault.Api --project src/LinkVault.Infrastructure
`

## Executar API
`ash
dotnet run --project src/LinkVault.Api
`
Swagger: http://localhost:5104/swagger (porta pode variar de acordo com o launchSettings.json).

## Usuário demo (seed)
- email: demo@linkvault.local
- senha: Demo123!

## Ambiente
- Connection string padrão: Host=localhost;Port=5432;Database=linkvault;Username=linkvault;Password=linkvault
- Pode ser sobrescrita por ConnectionStrings__Default.

## Estrutura
- src/LinkVault.Api — API
- src/LinkVault.Application — Application layer (MediatR, etc.)
- src/LinkVault.Domain — Entidades
- src/LinkVault.Infrastructure — Persistência/Seed
- 	ests/LinkVault.UnitTests — testes (básico)
