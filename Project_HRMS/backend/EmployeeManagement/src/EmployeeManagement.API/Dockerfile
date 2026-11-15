# Build context is the solution root:  docker build -f src/EmployeeManagement.API/Dockerfile -t employee-management-api .
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY src/EmployeeManagement.Domain/EmployeeManagement.Domain.csproj src/EmployeeManagement.Domain/
COPY src/EmployeeManagement.Application/EmployeeManagement.Application.csproj src/EmployeeManagement.Application/
COPY src/EmployeeManagement.Infrastructure/EmployeeManagement.Infrastructure.csproj src/EmployeeManagement.Infrastructure/
COPY src/EmployeeManagement.API/EmployeeManagement.API.csproj src/EmployeeManagement.API/
RUN dotnet restore src/EmployeeManagement.API/EmployeeManagement.API.csproj
COPY src/ src/
RUN dotnet publish src/EmployeeManagement.API/EmployeeManagement.API.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
USER $APP_UID
ENTRYPOINT ["dotnet", "EmployeeManagement.API.dll"]
