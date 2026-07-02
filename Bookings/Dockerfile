FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /Bookings

# Копируем только csproj и делаем restore (для кэширования зависимостей)
COPY ["Bookings/Presentation/Presentation.csproj", "Presentation/"]
COPY ["Bookings/Application/Application.csproj", "Application/"]
COPY ["Bookings/Domain/Domain.csproj", "Domain/"]
COPY ["Bookings/Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["YaEventsConfigurations/YaEventsConfigurations.csproj", "YaEventsConfigurations/"]
RUN dotnet restore "Presentation/Presentation.csproj"

# Копируем весь код и собираем релиз
COPY . .
#WORKDIR "/Bookings/Presentation"
RUN dotnet publish "Bookings/Presentation/Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ (только рантайм)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV KAFKA_BOOTSTRAP_SERVERS=kafka:29092

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Presentation.dll"]