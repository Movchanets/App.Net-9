# Multi-stage build for .NET 9 API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY App.NET.sln ./
COPY API/API.csproj API/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
RUN dotnet restore API/API.csproj
COPY . .
RUN dotnet publish API/API.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
# Align with Azure Container Apps detected listening port (80)
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
COPY --from=build /out .
ENTRYPOINT ["dotnet", "API.dll"]
