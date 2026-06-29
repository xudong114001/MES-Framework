FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MES.Api/MES.Api.csproj", "MES.Api/"]
COPY ["MES.Application/MES.Application.csproj", "MES.Application/"]
COPY ["MES.Domain/MES.Domain.csproj", "MES.Domain/"]
COPY ["MES.Infrastructure/MES.Infrastructure.csproj", "MES.Infrastructure/"]
COPY ["MES.Integration/MES.Integration.csproj", "MES.Integration/"]
COPY ["MES.AI.Domain/MES.AI.Domain.csproj", "MES.AI.Domain/"]
COPY ["MES.AI.Application/MES.AI.Application.csproj", "MES.AI.Application/"]

RUN dotnet restore "MES.Api/MES.Api.csproj"

COPY . .
RUN dotnet publish "MES.Api/MES.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5180

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MES.Api.dll"]
