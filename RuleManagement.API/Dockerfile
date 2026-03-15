FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RuleManagement.API/RuleManagement.API.csproj", "RuleManagement.API/"]
RUN dotnet restore "RuleManagement.API/RuleManagement.API.csproj"
COPY . .
WORKDIR "/src/RuleManagement.API"
RUN dotnet build "RuleManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RuleManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RuleManagement.API.dll"]