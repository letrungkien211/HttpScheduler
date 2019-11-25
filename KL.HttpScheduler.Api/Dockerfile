FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["KL.HttpScheduler.Api/KL.HttpScheduler.Api.csproj", "KL.HttpScheduler.Api/"]
COPY ["KL.HttpScheduler/KL.HttpScheduler.csproj", "KL.HttpScheduler/"]
RUN dotnet restore "KL.HttpScheduler.Api/KL.HttpScheduler.Api.csproj"
COPY . .
WORKDIR "/src/KL.HttpScheduler.Api"
RUN dotnet build "KL.HttpScheduler.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "KL.HttpScheduler.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "KL.HttpScheduler.Api.dll"]