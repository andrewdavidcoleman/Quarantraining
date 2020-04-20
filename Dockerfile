#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 44

# NuGet restore
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["Quarantraining/Quarantraining.csproj", "Quarantraining/"]
RUN dotnet restore "Quarantraining/Quarantraining.csproj"
COPY . .
WORKDIR "/src/Quarantraining"
RUN dotnet build "Quarantraining.csproj" -c Release -o /app

# publish
FROM build AS publish
RUN dotnet publish "Quarantraining.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
#ENTRYPOINT ["dotnet", "Quarantraining.dll"]


#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
#WORKDIR /app
#COPY --from=publish /src/publish .
# ENTRYPOINT ["dotnet", "Colors.API.dll"]
# heroku uses the following
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Quarantraining.dll