FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AthenaEcommerce website.csproj", "./"]
RUN dotnet restore "AthenaEcommerce website.csproj"
COPY . .
RUN dotnet publish "AthenaEcommerce website.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# tzdata is required — MpesaService.cs calls TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi"),
# which throws on a minimal Linux image without the IANA timezone database installed.
RUN apt-get update && apt-get install -y --no-install-recommends tzdata && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} dotnet \"AthenaEcommerce website.dll\""]