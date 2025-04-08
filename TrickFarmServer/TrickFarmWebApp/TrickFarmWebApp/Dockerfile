FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER root
RUN mkdir dataprotection && \
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
EXPOSE 53502

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TrickFarmWebApp/TrickFarmWebApp/TrickFarmWebApp.csproj", "TrickFarmWebApp/TrickFarmWebApp/"]
COPY ["TrickFarmWebApp/TrickFarmWebApp.Client/TrickFarmWebApp.Client.csproj", "TrickFarmWebApp/TrickFarmWebApp.Client/"]
RUN dotnet restore "./TrickFarmWebApp/TrickFarmWebApp/TrickFarmWebApp.csproj"
COPY . . 
WORKDIR "/src/TrickFarmWebApp/TrickFarmWebApp"
RUN dotnet build "./TrickFarmWebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TrickFarmWebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Use environment variable to bind to all network interfaces
ENV ASPNETCORE_URLS=http://0.0.0.0:53502

# Entry point to start the application
ENTRYPOINT ["dotnet", "TrickFarmWebApp.dll"]
