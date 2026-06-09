FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EcoWatch.sln", "./"]
COPY ["EcoWatch.Api/EcoWatch.Api.csproj", "EcoWatch.Api/"]
COPY ["EcoWatch.Application/EcoWatch.Application.csproj", "EcoWatch.Application/"]
COPY ["EcoWatch.Domain/EcoWatch.Domain.csproj", "EcoWatch.Domain/"]
COPY ["EcoWatch.Infrastructure/EcoWatch.Infrastructure.csproj", "EcoWatch.Infrastructure/"]
COPY ["EcoWatch.Tests/EcoWatch.Tests.csproj", "EcoWatch.Tests/"]
RUN dotnet restore

COPY . .
WORKDIR "/src/EcoWatch.Api"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "EcoWatch.Api.dll"]