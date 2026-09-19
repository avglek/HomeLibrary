# ───────────────────────────────────────────────────────────────
# Этап 1: сборка
# ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Сначала копируем только .csproj — чтобы кэшировать restore
COPY ["HomeLibrary/HomeLibrary.csproj", "HomeLibrary/"]
RUN dotnet restore "HomeLibrary/HomeLibrary.csproj"

# Затем — весь остальной код
COPY . .
WORKDIR "/src/HomeLibrary"
RUN dotnet publish "HomeLibrary.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ───────────────────────────────────────────────────────────────
# Этап 2: рантайм
# ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=build /app/publish .

# Порт, который слушает Kestrel внутри контейнера
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "HomeLibrary.dll"]