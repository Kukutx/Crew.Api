# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# 复制必要的 csproj 并 restore（不包含 SeedDataImporter 工具）
COPY Crew.Api/Crew.Api.csproj ./Crew.Api/
COPY Crew.Domain/Crew.Domain.csproj ./Crew.Domain/
COPY Crew.Application/Crew.Application.csproj ./Crew.Application/
COPY Crew.Infrastructure/Crew.Infrastructure.csproj ./Crew.Infrastructure/
COPY Crew.Contracts/Crew.Contracts.csproj ./Crew.Contracts/
RUN dotnet restore ./Crew.Api/Crew.Api.csproj

# 复制所有文件并发布
COPY . ./
WORKDIR /app/Crew.Api
RUN dotnet publish Crew.Api.csproj -c Release -o /app/out --no-restore

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# 使用 Railway 提供的端口
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "Crew.Api.dll"]
