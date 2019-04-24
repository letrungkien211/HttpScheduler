FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["KL.HttpScheduler.Runner/KL.HttpScheduler.Runner.csproj", "KL.HttpScheduler.Runner/"]
RUN dotnet restore "KL.HttpScheduler.Runner/KL.HttpScheduler.Runner.csproj"
COPY . .
WORKDIR "/src/KL.HttpScheduler.Runner"
RUN dotnet build "KL.HttpScheduler.Runner.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "KL.HttpScheduler.Runner.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "KL.HttpScheduler.Runner.dll"]