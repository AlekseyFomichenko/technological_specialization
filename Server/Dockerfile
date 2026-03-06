FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /src

COPY Directory.Build.props* Directory.Build.targets* global.json* NuGet.config* ./

COPY Server/Server.csproj Server/

COPY Shared/Shared.csproj Shared/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Server/Server.csproj

COPY . .

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish Server/Server.csproj \
    -c Release \
    -o /app/publish \
    -r linux-musl-x64 \
    --self-contained true \
    -p:PublishTrimmed=false \
    -p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS runtime

WORKDIR /app

RUN addgroup -S -g 10001 appgrp \
    && adduser -S -u 10001 -G appgrp appuser

COPY --from=build --chown=10001:10001 /app/publish ./

RUN mkdir -p /app/files && chown 10001:10001 /app/files

USER 10001

ENV ASPNETCORE_URLS=http://+:5001

EXPOSE 5001

ENTRYPOINT ["./Server"]
