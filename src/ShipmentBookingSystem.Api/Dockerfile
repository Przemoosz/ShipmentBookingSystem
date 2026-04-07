FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "./"]
COPY ["src/ShipmentBookingSystem.Api/ShipmentBookingSystem.Api.csproj", "src/ShipmentBookingSystem.Api/"]
COPY ["src/ShipmentBookingSystem.Presentation/ShipmentBookingSystem.Presentation.csproj", "src/ShipmentBookingSystem.Presentation/"]
COPY ["src/ShipmentBookingSystem.Application/ShipmentBookingSystem.Application.csproj", "src/ShipmentBookingSystem.Application/"]
COPY ["src/ShipmentBookingSystem.Domain/ShipmentBookingSystem.Domain.csproj", "src/ShipmentBookingSystem.Domain/"]
COPY ["src/ShipmentBookingSystem.Infrastructure/ShipmentBookingSystem.Infrastructure.csproj", "src/ShipmentBookingSystem.Infrastructure/"]
RUN dotnet restore "./src/ShipmentBookingSystem.Api/ShipmentBookingSystem.Api.csproj"
COPY . .
WORKDIR "/src/src/ShipmentBookingSystem.Api"
RUN dotnet build "./ShipmentBookingSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShipmentBookingSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShipmentBookingSystem.Api.dll"]