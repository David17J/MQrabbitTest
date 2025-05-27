FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MQrabbitTest.Consumer.New/MQrabbitTest.Consumer.New.csproj", "MQrabbitTest.Consumer.New/"]
RUN dotnet restore "MQrabbitTest.Consumer.New/MQrabbitTest.Consumer.New.csproj"
COPY . .
WORKDIR "/src/MQrabbitTest.Consumer.New"
RUN dotnet build "MQrabbitTest.Consumer.New.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MQrabbitTest.Consumer.New.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MQrabbitTest.Consumer.New.dll"] 