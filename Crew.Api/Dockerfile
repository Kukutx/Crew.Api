# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0.203 AS build
WORKDIR /app

# 先只拷贝 csproj + sln，加速 restore（大小写必须匹配）
COPY Crew.Api/Crew.Api.csproj ./Crew.Api/
COPY Crew.Services/Crew.Services.csproj ./Crew.Services/
COPY Crew.Shared/Crew.Shared.csproj ./Crew.Shared/
COPY Crew.Data/Crew.Data.csproj ./Crew.Data/
COPY Crew.Api.sln ./

RUN dotnet restore ./Crew.Api.sln

# 再拷贝全部源码
COPY . .

# 发布 Api（只需要发布 Web 项目）
WORKDIR /app/Crew.Api
RUN dotnet publish -c Release -o /app/out

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0.13
WORKDIR /app
COPY --from=build /app/out .

# Render 会注入 PORT；用 ASPNETCORE_URLS 绑定到 0.0.0.0
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080
# 某些平台喜欢显式暴露一个默认端口，但核心取决于 Render 的 PORT

ENTRYPOINT ["dotnet", "Crew.Api.dll"]
