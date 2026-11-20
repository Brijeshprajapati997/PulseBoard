FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/shared/PulseBoard.Contracts/ src/shared/PulseBoard.Contracts/
COPY src/services/PulseBoard.ApiService/ src/services/PulseBoard.ApiService/

WORKDIR /app/src/services/PulseBoard.ApiService
RUN dotnet publish -c Release -o /publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PulseBoard.ApiService.dll"]
