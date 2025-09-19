# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0.203 AS build
WORKDIR /app

# 复制解决方案文件和所有子项目 csproj
COPY Crew.Api.sln .
COPY Crew.Api/*.csproj ./Crew.Api/

# Restore
RUN dotnet restore

# Publish
COPY Crew.Api/. ./Crew.Api/
WORKDIR /app/Crew.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0.13
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "Crew.Api.dll"]
