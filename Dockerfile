#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk AS build
WORKDIR /src
COPY ["jaegertracing-jaeger-issue-2638.csproj", "."]
RUN dotnet restore "jaegertracing-jaeger-issue-2638.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "jaegertracing-jaeger-issue-2638.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "jaegertracing-jaeger-issue-2638.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "jaegertracing-jaeger-issue-2638.dll"]
