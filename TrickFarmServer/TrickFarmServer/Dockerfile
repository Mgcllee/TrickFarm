FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 상대 경로는 Dockerfile 기준이 아니라 빌드 컨텍스트 기준
COPY TrickFarmServer.csproj ./ 
RUN dotnet restore TrickFarmServer.csproj

COPY . ./
WORKDIR /src
RUN dotnet build TrickFarmServer.csproj -c Release -o /app/build
RUN dotnet publish TrickFarmServer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TrickFarmServer.dll"]
