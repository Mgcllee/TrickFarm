FROM mcr.microsoft.com/devcontainers/base:ubuntu

# 기본 패키지 설치
RUN apt-get update && apt-get install -y \
    build-essential \
    cmake \
    curl \
    git \
    wget \
    lsb-release \
    gnupg \
    software-properties-common \
    redis-server

# .NET 8 설치
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-8.0

# Redis 데몬 비활성화 (백그라운드 실행 안되도록)
RUN sed -i 's/^supervised no/supervised systemd/' /etc/redis/redis.conf

# 기본 작업 디렉토리 설정
WORKDIR /workspace
