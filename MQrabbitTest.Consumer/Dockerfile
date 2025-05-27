FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MQrabbitTest.Consumer/MQrabbitTest.Consumer.csproj", "MQrabbitTest.Consumer/"]
RUN dotnet restore "MQrabbitTest.Consumer/MQrabbitTest.Consumer.csproj"
COPY . .
WORKDIR "/src/MQrabbitTest.Consumer"
RUN dotnet build "MQrabbitTest.Consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MQrabbitTest.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MQrabbitTest.Consumer.dll"] 