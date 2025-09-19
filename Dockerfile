# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj và restore
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ source và publish
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Copy từ stage build
COPY --from=build /app/out .

# Biến môi trường PORT do Render cung cấp
ENV PORT 5000
EXPOSE $PORT

# Chạy app
ENTRYPOINT ["dotnet", "HRMApi.dll"]
