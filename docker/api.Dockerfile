FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Directory.Build.props src/Directory.Packages.props ./
COPY src/lib/DBTools_SQL/DBTools/ src/lib/DBTools_SQL/DBTools/
COPY src/BuildingBlocks/ControlEasyReborn.SharedKernel/ src/BuildingBlocks/ControlEasyReborn.SharedKernel/
COPY src/BuildingBlocks/ControlEasyReborn.Infrastructure/ src/BuildingBlocks/ControlEasyReborn.Infrastructure/
COPY src/Modules/Tenants/ src/Modules/Tenants/
COPY src/Modules/Residents/ src/Modules/Residents/
COPY src/Host/ControlEasyReborn.Api/ src/Host/ControlEasyReborn.Api/
COPY global.json ./

RUN dotnet restore src/Host/ControlEasyReborn.Api/ControlEasyReborn.Api.csproj
RUN dotnet publish src/Host/ControlEasyReborn.Api/ControlEasyReborn.Api.csproj \
    -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ControlEasyReborn.Api.dll"]