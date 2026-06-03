# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Dependiendo del sistema operativo de las máquinas host que vayan a compilar o ejecutar los contenedores, puede que sea necesario cambiar la imagen especificada en la instrucción FROM.
# Para más información, consulte https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AnotadorGymApp.Api/AnotadorGymApp.Api.csproj", "src/AnotadorGymApp.Api/"]

RUN dotnet restore "src/AnotadorGymApp.Api/AnotadorGymApp.Api.csproj"

COPY . .
WORKDIR "/src/src/AnotadorGymApp.Api"
RUN dotnet build "AnotadorGymApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AnotadorGymApp.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false
    
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnotadorGymApp.Api.dll"]