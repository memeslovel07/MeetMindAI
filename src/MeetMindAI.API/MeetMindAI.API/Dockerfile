# ==========================
# Build Stage
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy solution & props
COPY ["MeetMindAI.sln", "."]
COPY ["Directory.Build.props", "."]
COPY ["global.json", "."]

# Copy project files
COPY ["src/MeetMindAI.API/MeetMindAI.API/MeetMindAI.API.csproj", "src/MeetMindAI.API/MeetMindAI.API/"]
COPY ["src/MeetMindAI.Application/MeetMindAI.Application/MeetMindAI.Application.csproj", "src/MeetMindAI.Application/MeetMindAI.Application/"]
COPY ["src/MeetMindAI.Domain/MeetMindAI.Domain/MeetMindAI.Domain.csproj", "src/MeetMindAI.Domain/MeetMindAI.Domain/"]
COPY ["src/MeetMindAI.Infrastructure/MeetMindAI.Infrastructure/MeetMindAI.Infrastructure.csproj", "src/MeetMindAI.Infrastructure/MeetMindAI.Infrastructure/"]
COPY ["src/MeetMindAI.Persistence/MeetMindAI.Persistence/MeetMindAI.Persistence.csproj", "src/MeetMindAI.Persistence/MeetMindAI.Persistence/"]
COPY ["src/MeetMindAI.Shared/MeetMindAI.Shared/MeetMindAI.Shared.csproj", "src/MeetMindAI.Shared/MeetMindAI.Shared/"]

# Restore
RUN dotnet restore "src/MeetMindAI.API/MeetMindAI.API/MeetMindAI.API.csproj"

# Copy remaining source
COPY . .

WORKDIR /src/src/MeetMindAI.API/MeetMindAI.API

# Publish
RUN dotnet publish \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ==========================
# Runtime Stage
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "MeetMindAI.API.dll"]
