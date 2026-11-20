FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/shared/PulseBoard.Contracts/ src/shared/PulseBoard.Contracts/
COPY src/services/PulseBoard.WebhookService/ src/services/PulseBoard.WebhookService/

WORKDIR /app/src/services/PulseBoard.WebhookService
RUN dotnet publish -c Release -o /publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PulseBoard.WebhookService.dll"]
