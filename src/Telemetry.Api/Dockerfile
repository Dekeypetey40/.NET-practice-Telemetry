FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.sln ./
COPY src/Telemetry.Domain/Telemetry.Domain.csproj src/Telemetry.Domain/
COPY src/Telemetry.Application/Telemetry.Application.csproj src/Telemetry.Application/
COPY src/Telemetry.Infrastructure/Telemetry.Infrastructure.csproj src/Telemetry.Infrastructure/
COPY src/Telemetry.Api/Telemetry.Api.csproj src/Telemetry.Api/
RUN dotnet restore src/Telemetry.Api/Telemetry.Api.csproj
COPY src/ src/
RUN dotnet publish src/Telemetry.Api/Telemetry.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Telemetry.Api.dll"]
