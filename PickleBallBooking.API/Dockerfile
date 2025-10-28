FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PickleBallBooking.API/PickleBallBooking.API.csproj", "PickleBallBooking.API/"]
COPY ["PickleBallBooking.Services/PickleBallBooking.Services.csproj", "PickleBallBooking.Services/"]
COPY ["PickleBallBooking.Repositories/PickleBallBooking.Repositories.csproj", "PickleBallBooking.Repositories/"]
COPY ["PickleBallBooking.Domain/PickleBallBooking.Domain.csproj", "PickleBallBooking.Domain/"]
RUN dotnet restore "PickleBallBooking.API/PickleBallBooking.API.csproj"
COPY . .
WORKDIR "/src/PickleBallBooking.API"
RUN dotnet build "./PickleBallBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PickleBallBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PickleBallBooking.API.dll"]
