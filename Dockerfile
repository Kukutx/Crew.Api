# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# 复制 csproj 并 restore
COPY *.sln .
COPY Crew.Api/*.csproj ./Crew.Api/
RUN dotnet restore

# 复制所有文件并发布
COPY Crew.Api/. ./Crew.Api/
WORKDIR /app/Crew.Api
RUN dotnet publish -c Release -o /app/out

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# 使用 Railway 提供的端口
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "Crew.Api.dll"]