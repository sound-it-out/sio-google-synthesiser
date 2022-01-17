#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/", "src/"]
RUN dotnet restore "src/SIO.Google.Synthesizer/SIO.Google.Synthesizer.csproj"
COPY . .
WORKDIR "/src/src/SIO.Google.Synthesizer"
RUN dotnet build "SIO.Google.Synthesizer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIO.Google.Synthesizer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIO.Google.Synthesizer.dll"]