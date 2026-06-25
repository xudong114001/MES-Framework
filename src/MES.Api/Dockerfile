FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MES.slnx", "."]
COPY ["src/MES.Domain/MES.Domain.csproj", "src/MES.Domain/"]
COPY ["src/MES.Application/MES.Application.csproj", "src/MES.Application/"]
COPY ["src/MES.Infrastructure/MES.Infrastructure.csproj", "src/MES.Infrastructure/"]
COPY ["src/MES.Integration/MES.Integration.csproj", "src/MES.Integration/"]
COPY ["src/MES.AI.Domain/MES.AI.Domain.csproj", "src/MES.AI.Domain/"]
COPY ["src/MES.AI.Application/MES.AI.Application.csproj", "src/MES.AI.Application/"]
COPY ["src/MES.Api/MES.Api.csproj", "src/MES.Api/"]
RUN dotnet restore
COPY . .
RUN dotnet build "src/MES.Api/MES.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/MES.Api/MES.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5180
EXPOSE 5180
ENTRYPOINT ["dotnet", "MES.Api.dll"]
