FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["./TrickFarmOrleansServer/TrickFarmOrleansServer.csproj", "./TrickFarmOrleansServer/"]
COPY ["../Common", "./Common"]
RUN dotnet restore "./TrickFarmOrleansServer/TrickFarmOrleansServer.csproj"

COPY [".", "./"]
WORKDIR /src
RUN dotnet build ./TrickFarmOrleansServer/TrickFarmOrleansServer.csproj -c Release -o /app/build
RUN dotnet publish ./TrickFarmOrleansServer/TrickFarmOrleansServer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TrickFarmOrleansServer.dll"]
