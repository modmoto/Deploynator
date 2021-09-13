FROM mcr.microsoft.com/dotnet/aspnet:5.0.9-alpine3.13-arm32v7 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Deploynator/Deploynator.csproj", "Deploynator/"]
RUN dotnet restore "Deploynator/Deploynator.csproj" -r linux-arm
COPY . .
WORKDIR "/src/Deploynator"
RUN dotnet build "Deploynator.csproj" -r linux-arm -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Deploynator.csproj" -r linux-arm -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Deploynator.dll"]
