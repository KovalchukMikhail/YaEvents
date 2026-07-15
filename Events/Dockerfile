FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /Events

# Копируем только csproj и делаем restore (для кэширования зависимостей)
COPY ["Events/Presentation/Presentation.csproj", "Presentation/"]
COPY ["Events/Application/Application.csproj", "Application/"]
COPY ["Events/Domain/Domain.csproj", "Domain/"]
COPY ["Events/Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["YaEventsConfigurations/YaEventsConfigurations.csproj", "YaEventsConfigurations/"]
RUN dotnet restore "Presentation/Presentation.csproj"

# Копируем весь код и собираем релиз
COPY . .
#WORKDIR "/Events/Presentation"
RUN dotnet publish "Events/Presentation/Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ (только рантайм)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV KAFKA_BOOTSTRAP_SERVERS=kafka:29092

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Presentation.dll"]