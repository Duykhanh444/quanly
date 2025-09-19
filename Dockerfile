# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Sao chép csproj và restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Sao chép toàn bộ source code và publish
COPY . ./
RUN dotnet publish -c Release -o out

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy từ build stage
COPY --from=build /app/out ./

# Mặc định chạy API
ENTRYPOINT ["dotnet", "HRMApi.dll"]
