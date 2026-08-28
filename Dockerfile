# ==========================================================
# ESTÁGIO 1 - Runtime
# Imagem leve que será utilizada para executar a aplicação
# ==========================================================

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080
EXPOSE 8081


# ==========================================================
# ESTÁGIO 2 - Build
# Imagem contendo o SDK completo do .NET
# ==========================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src


# ----------------------------------------------------------
# Copia apenas os .csproj primeiro
#
# Isso melhora o cache do Docker.
# Se o código mudar, mas os pacotes não mudarem,
# o Docker pode reutilizar o resultado do restore.
# ----------------------------------------------------------

COPY ["TaskManager.API/TaskManager.API.csproj", "TaskManager.API/"]
COPY ["TaskManager.Domain/TaskManager.Domain.csproj", "TaskManager.Domain/"]
COPY ["TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "TaskManager.Infrastructure/"]


# Restaurar dependências
RUN dotnet restore "TaskManager.API/TaskManager.API.csproj"


# ----------------------------------------------------------
# Agora copia todo o código fonte
# ----------------------------------------------------------

COPY . .


# Entramos no projeto Web/API
WORKDIR "/src/TaskManager.API"


# Compilar
RUN dotnet build "TaskManager.API.csproj" \
    -c Release \
    -o /app/build


# ==========================================================
# ESTÁGIO 3 - Publish
# ==========================================================

FROM build AS publish

RUN dotnet publish "TaskManager.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ==========================================================
# ESTÁGIO 4 - Imagem final
# ==========================================================

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TaskManager.API.dll"]