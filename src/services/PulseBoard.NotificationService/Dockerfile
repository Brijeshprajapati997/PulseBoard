FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/shared/PulseBoard.Contracts/ src/shared/PulseBoard.Contracts/
COPY src/services/PulseBoard.NotificationService/ src/services/PulseBoard.NotificationService/

WORKDIR /app/src/services/PulseBoard.NotificationService
RUN dotnet publish -c Release -o /publish --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "PulseBoard.NotificationService.dll"]
