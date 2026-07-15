FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /Users

# Копируем только csproj и делаем restore (для кэширования зависимостей)
COPY ["Presentation/Presentation.csproj", "Presentation/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Presentation/Presentation.csproj"


# Копируем весь код и собираем релиз
COPY . .
WORKDIR "/Users/Presentation"
RUN dotnet publish Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ (только рантайм)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV KAFKA_BOOTSTRAP_SERVERS=kafka:29092

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Presentation.dll"]