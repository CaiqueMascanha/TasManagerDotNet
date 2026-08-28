# Estágio 1: Runtime (Imagem leve para rodar o app em produção)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Estágio 2: Build (Imagem robusta com SDK do .NET 10 para restaurar e compilar)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar arquivos de projeto (.csproj) individualmente para cachear a restauração de pacotes
COPY ["TaskManager.API/TaskManager.API.csproj", "TaskManager.API/"]
COPY ["TaskManager.Domain/TaskManager.Domain.csproj", "TaskManager.Domain/"]
COPY ["TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "TaskManager.Infrastructure/"]

# Restaurar as dependências via NuGet
RUN dotnet restore "TaskManager.API/TaskManager.API.csproj"

# Copiar o restante de todo o código fonte e realizar o build
COPY . .
WORKDIR "/src/TaskManager.API"
RUN dotnet build "TaskManager.API.csproj" -c Release -o /app/build

# Estágio 3: Publicação (Gera os arquivos finais compilados prontos para execução)
FROM build AS publish
RUN dotnet publish "TaskManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 4: Final (Copia apenas os arquivos gerados no Estágio 3 para a imagem leve do Estágio 1)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]