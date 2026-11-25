FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY global.json ./
COPY Vetcare/Vetcare.csproj Vetcare/
COPY Vetcare.Domain/Vetcare.Domain.csproj Vetcare.Domain/
COPY Vetcare.Application/Vetcare.Application.csproj Vetcare.Application/
COPY Vetcare.Infrastructure/Vetcare.Infrastructure.csproj Vetcare.Infrastructure/
RUN dotnet restore Vetcare/Vetcare.csproj

COPY . .
RUN dotnet publish Vetcare/Vetcare.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Vetcare.API.dll"]
