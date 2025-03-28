FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AuthMicroservice.WebAPI/AuthMicroservice.WebAPI.csproj", "AuthMicroservice.WebAPI/"]
COPY ["AuthMicroservice.BusinessLogic/AuthMicroservice.BusinessLogic.csproj", "AuthMicroservice.BusinessLogic/"]
COPY ["AuthMicroservice.DataAccess/AuthMicroservice.DataAccess.csproj", "AuthMicroservice.DataAccess/"]
COPY ["AuthMicroservice.Shared/AuthMicroservice.Shared.csproj", "AuthMicroservice.Shared/"]
RUN dotnet restore "AuthMicroservice.WebAPI/AuthMicroservice.WebAPI.csproj"
COPY . .
WORKDIR "/src/AuthMicroservice.WebAPI"
RUN dotnet build "AuthMicroservice.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthMicroservice.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthMicroservice.WebAPI.dll"]
