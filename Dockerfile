# Использование официального образа SDK для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файл .csproj вашего единственного проекта
# Предполагаем, что ваш основной проект называется TestTaskModulBank.csproj
COPY ["TestTaskModulBank.csproj", "./"]

# Восстанавливаем зависимости
RUN dotnet restore "./TestTaskModulBank.csproj"

# Копируем остальные файлы исходного кода
COPY . .

# Публикуем приложение в режиме Release
# -c Release: сборка в режиме Release
# -o /app/publish: выходной каталог внутри контейнера
RUN dotnet publish "./TestTaskModulBank.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Используем официальный образ ASP.NET для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Копируем опубликованное приложение из стадии build
COPY --from=build /app/publish .

# Открываем порт, на котором будет работать API
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "TestTaskModulBank.dll"]