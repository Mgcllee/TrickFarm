FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["TrickFarmServer.csproj", "."]
WORKDIR /app/TrickFarmServer
RUN dotnet restore

COPY . /app
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TrickFarmServer.dll"]
