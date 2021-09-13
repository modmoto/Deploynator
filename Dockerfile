FROM mcr.microsoft.com/dotnet/sdk:5.0 AS base
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

RUN mkdir -p /root/src/app
WORKDIR /root/src/app
COPY Deploynator Deploynator
WORKDIR /root/src/app/Deploynator

RUN dotnet restore ./Deploynator.csproj
RUN dotnet publish -c release -o published -r linux-arm

FROM mcr.microsoft.com/dotnet/aspnet:5.0.9-alpine3.13-arm32v7

WORKDIR /root/
COPY --from=builder /root/src/app/Deploynator/published .

CMD ["dotnet", "./Deploynator.dll"]