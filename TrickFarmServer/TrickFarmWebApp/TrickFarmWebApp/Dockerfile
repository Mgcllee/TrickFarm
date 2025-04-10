# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER root
RUN mkdir -p /dataprotection && \
    apt-get update && \
    apt-get install -y locales fonts-nanum fontconfig && \
    locale-gen ko_KR.UTF-8

ENV LANG=ko_KR.UTF-8 \
    LANGUAGE=ko_KR:ko \
    LC_ALL=ko_KR.UTF-8

ARG APP_UID=1000
RUN useradd -u $APP_UID appuser
USER $APP_UID

WORKDIR /app

EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TrickFarmWebApp/TrickFarmWebApp/TrickFarmWebApp.csproj", "TrickFarmWebApp/TrickFarmWebApp/"]
COPY ["TrickFarmWebApp/TrickFarmWebApp.Client/TrickFarmWebApp.Client.csproj", "TrickFarmWebApp/TrickFarmWebApp.Client/"]
RUN dotnet restore "./TrickFarmWebApp/TrickFarmWebApp/TrickFarmWebApp.csproj"
COPY . .
WORKDIR "/src/TrickFarmWebApp/TrickFarmWebApp"
RUN dotnet build "./TrickFarmWebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TrickFarmWebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8081

ENTRYPOINT ["dotnet", "TrickFarmWebApp.dll"]
