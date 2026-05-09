FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Backend/FlowgorithmAnalyzer.API/FlowgorithmAnalyzer.API.csproj", "FlowgorithmAnalyzer.API/"]
COPY ["Backend/FlowgorithmAnalyzer.Core/FlowgorithmAnalyzer.Core.csproj", "FlowgorithmAnalyzer.Core/"]
COPY ["Backend/FlowgorithmAnalyzer.Infrastructure/FlowgorithmAnalyzer.Infrastructure.csproj", "FlowgorithmAnalyzer.Infrastructure/"]

# Restore
RUN dotnet restore "FlowgorithmAnalyzer.API/FlowgorithmAnalyzer.API.csproj"

# Copy all source code
COPY Backend/ .

# Build
RUN dotnet build "FlowgorithmAnalyzer.API/FlowgorithmAnalyzer.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "FlowgorithmAnalyzer.API/FlowgorithmAnalyzer.API.csproj" -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "FlowgorithmAnalyzer.API.dll"]
