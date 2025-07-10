FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TestTaskModulBank/TestTaskModulBank.csproj", "TestTaskModulBank/"]
RUN dotnet restore "TestTaskModulBank/TestTaskModulBank.csproj"
COPY . .
WORKDIR "/src/TestTaskModulBank"
RUN dotnet build "./TestTaskModulBank.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TestTaskModulBank.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestTaskModulBank.dll"]

#
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#WORKDIR /app
#
## Копируем .csproj и восстанавливаем зависимости
#COPY ["TicTacToeGame.Api/TicTacToeGame.Api.csproj", "TicTacToeGame.Api/"]
#COPY ["TicTacToeGame.Application/TicTacToeGame.Application.csproj", "TicTacToeGame.Application/"]
#COPY ["TicTacToeGame.Core/TicTacToeGame.Core.csproj", "TicTacToeGame.Core/"]
#COPY ["TicTacToeGame.Infrastructure/TicTacToeGame.Infrastructure.csproj", "TicTacToeGame.Infrastructure/"]
#
#WORKDIR /app/TicTacToeGame.Api
#RUN dotnet restore "TicTacToeGame.Api.csproj"
#
## Копируем остальной код
#WORKDIR /src
#COPY . .
#
## Публикуем приложение
#WORKDIR /src/TicTacToeGame.Api
#RUN dotnet publish "TicTacToeGame.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
#
#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
#WORKDIR /app
#COPY --from=build /app/publish .
#
#EXPOSE 8080
#
#ENV ASPNETCORE_URLS=http://+:8080
#ENV POSTGRES_CONNECTION_STRING="Host=db;Port=5432;Database=tictactoe;Username=user;Password=password"
#ENV WINNING_CONDITION=3 # Можно переопределить для игры 4-в-ряд и т.д.
#
#ENTRYPOINT ["dotnet", "TicTacToeGame.Api.dll"]
