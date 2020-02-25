FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base

ARG VAULT_TOKEN
ENV HTDC_VAULT_TOKEN=$VAULT_TOKEN

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ARG GITHUB_TOKEN
WORKDIR /src
COPY *.sln ./
COPY HappyTravel.CurrencyConverter/HappyTravel.CurrencyConverter.csproj HappyTravel.CurrencyConverter/
COPY HappyTravel.CurrencyConverterTests/HappyTravel.CurrencyConverterTests.csproj HappyTravel.CurrencyConverterTests/
COPY nuget.config ./
RUN dotnet restore
COPY . .
WORKDIR /src/HappyTravel.CurrencyConverter
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app

COPY --from=publish /app .

HEALTHCHECK --interval=6s --timeout=10s --retries=3 CMD curl -sS 127.0.0.1/health || exit 1

ENTRYPOINT ["dotnet", "HappyTravel.CurrencyConverter.dll"]