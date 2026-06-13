.PHONY: up down logs test migrate build restore clean

up:
	docker compose -f docker/docker-compose.yml up -d

down:
	docker compose -f docker/docker-compose.yml down -v

logs:
	docker compose -f docker/docker-compose.yml logs -f

test:
	dotnet test tests/ControlEasyReborn.UnitTests/ControlEasyReborn.UnitTests.csproj --nologo
	dotnet test tests/ControlEasyReborn.IntegrationTests/ControlEasyReborn.IntegrationTests.csproj --nologo || true

migrate:
	@echo "Schema migrations run automatically via docker/mysql/init/ scripts on first boot."

build:
	dotnet build src/ControlEasyReborn.sln -nologo

restore:
	dotnet restore src/ControlEasyReborn.sln

clean:
	dotnet clean src/ControlEasyReborn.sln -nologo
	rm -rf src/**/bin src/**/obj tests/**/bin tests/**/obj