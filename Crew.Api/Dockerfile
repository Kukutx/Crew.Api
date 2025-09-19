# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0.203 AS build
WORKDIR /app

# 复制解决方案和项目文件
COPY Crew.Api/*.csproj ./Crew.Api/
COPY Crew.Services/*.csproj ./Crew.Services/
COPY Crew.Shared/*.csproj ./Crew.Shared/
COPY Crew.Data/*.csproj ./Crew.Data/
COPY *.sln ./

# Restore
RUN dotnet restore

# 复制全部文件并 publish
COPY . .
WORKDIR /app/Crew.Api
RUN dotnet publish -c Release -o /app/out

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0.13
WORKDIR /app
COPY --from=build /app/out .

# Railway 环境端口
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "Crew.Api.dll"]
