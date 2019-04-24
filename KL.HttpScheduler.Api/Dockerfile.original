FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["KL.HttpScheduler/KL.HttpScheduler.csproj", "KL.HttpScheduler/"]
RUN dotnet restore "KL.HttpScheduler/KL.HttpScheduler.csproj"
COPY . .
WORKDIR "/src/KL.HttpScheduler"
RUN dotnet build "KL.HttpScheduler.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "KL.HttpScheduler.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "KL.HttpScheduler.dll"]