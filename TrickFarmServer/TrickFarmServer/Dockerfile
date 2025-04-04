# 1. 빌드 환경
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 프로젝트 파일 복사 및 복원
COPY ["TrickFarmServer/TrickFarmServer.csproj", "TrickFarmServer/"]
WORKDIR /app/TrickFarmServer
RUN dotnet restore

# 전체 소스 코드 복사 및 빌드
COPY . /app
RUN dotnet publish -c Release -o /app/publish

# 2. 런타임 환경
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 빌드된 파일 복사
COPY --from=build /app/publish .

# 실행 명령
ENTRYPOINT ["dotnet", "TrickFarmServer.dll"]
