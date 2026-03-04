FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["Server/Server.csproj", "Server/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore Server/Server.csproj
COPY . .
RUN dotnet publish Server/Server.csproj -c Release -o /app/publish -r linux-musl-x64 --self-contained true -p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine
WORKDIR /app
RUN adduser -D appuser && chown -R appuser /app
USER appuser
COPY --from=build /app/publish .
EXPOSE 5001
ENTRYPOINT ["./Server"]
