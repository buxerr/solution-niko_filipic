FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.sln .
COPY ProductCatalog.API/*.csproj ProductCatalog.API/
COPY ProductCatalog.Application/*.csproj ProductCatalog.Application/
COPY ProductCatalog.Domain/*.csproj ProductCatalog.Domain/
COPY ProductCatalog.Infrastructure/*.csproj ProductCatalog.Infrastructure/
RUN dotnet restore ProductCatalog.API/ProductCatalog.API.csproj

COPY . .
WORKDIR /src/ProductCatalog.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductCatalog.API.dll"]
