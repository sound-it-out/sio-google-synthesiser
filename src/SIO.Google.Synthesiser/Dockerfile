#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/SIO.Google.Synthesiser/SIO.Google.Synthesiser.csproj", "src/SIO.Google.Synthesiser/"]
COPY ["src/SIO.Google.Credentials/SIO.Google.Credentials.csproj", "src/SIO.Google.Credentials/"]
COPY ["src/SIO.Domain/SIO.Domain.csproj", "src/SIO.Google.Synthesiser.Domain/"]
COPY ["./nuget.config", "./nuget.config"]
RUN dotnet restore "src/SIO.Google.Synthesiser/SIO.Google.Synthesiser.csproj" --configfile "./nuget.config"
COPY . .
RUN dotnet build "src/SIO.Google.Synthesiser/SIO.Google.Synthesiser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/SIO.Google.Synthesiser/SIO.Google.Synthesiser.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .