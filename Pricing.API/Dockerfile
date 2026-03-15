FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Pricing.API/Pricing.API.csproj", "Pricing.API/"]
RUN dotnet restore "Pricing.API/Pricing.API.csproj"
COPY . .
WORKDIR "/src/Pricing.API"
RUN dotnet build "Pricing.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pricing.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pricing.API.dll"]