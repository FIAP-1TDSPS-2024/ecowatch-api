
# 🌍 EcoWatch API (Projeto FireShield)

> **Status do Projeto:** Em Desenvolvimento (Sprint Atual: Arquitetura e Integrações Cloud)

O **EcoWatch** é a espinha dorsal de um ecossistema de monitoramento ambiental. Esta API RESTful atua como o backend central, responsável por orquestrar denúncias de usuários via aplicativo móvel, ingerir telemetria bruta de satélites/sensores IoT e distribuir eventos críticos de forma assíncrona para outros microsserviços.

----------

## 🏗️ Arquitetura e Decisões Técnicas

O projeto foi construído focando em alta disponibilidade, observabilidade e escalabilidade, dividindo as responsabilidades de persistência e mensageria:

-   **Dados Relacionais (Oracle Cloud):** Utilizado para dados estruturados com forte necessidade de integridade referencial (Usuários, Ocorrências confirmadas).

-   **Dados Não-Estruturados (MongoDB Atlas):** Adotado para a ingestão de telemetria e dados de sensores (`AlertaSatelite`). Como payloads de hardware variam muito, a flexibilidade do NoSQL via `BsonDocument` garante que a API não quebre com mudanças de formato.

-   **Mensageria Assíncrona (RabbitMQ/CloudAMQP):** Implementa o padrão de arquitetura orientada a eventos (EDA). Em vez de a API disparar SMS/E-mails de forma síncrona (o que gera gargalos), ela apenas publica o evento na fila `alertas_incendio_queue`. O processamento pesado fica a cargo de _Workers_ externos (ex: Microsserviço Java).

-   **Observabilidade:** Tratamento global de exceções via `IExceptionHandler` (evitando vazamento de stack trace no app cliente) e endpoint centralizado de Health Checks.


----------

## 🚀 Tecnologias Utilizadas

-   **Framework:** C# / .NET 8 (Web API)

-   **ORM:** Entity Framework Core 8

-   **Bancos de Dados:**

    -   Oracle Autonomous Database (Relacional)

    -   MongoDB Atlas (NoSQL Document)

-   **Mensageria:** RabbitMQ.Client v7.0 (Async) no CloudAMQP

-   **Segurança:** Autenticação JWT (JSON Web Tokens)

-   **Monitoramento:** Xabaril HealthChecks (RabbitMQ, MongoDb, Oracle)


----------

## 📋 Pré-requisitos

-   SDK do [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.

-   Acesso às credenciais dos serviços de nuvem (Oracle ADB, MongoDB Atlas, CloudAMQP).


----------

## ⚙️ Configuração do Ambiente (Secrets)

Para garantir a segurança, este projeto não armazena credenciais em arquivos `appsettings.json` ou no controle de versão. Utilizamos o `.NET Secret Manager`.

1.  Na raiz do projeto (onde está o `.sln`), solicite ao administrador de infraestrutura o script de provisionamento de senhas (`setup-secrets.sh`).

2.  Dê permissão de execução e rode o script:


```bash
chmod +x setup-secrets.sh
./setup-secrets.sh

```

Isso injetará as Connection Strings e chaves JWT de forma segura no seu sistema operacional local.

----------

## 🏃‍♂️ Como Executar

### 1. Restaure as dependências do projeto

```bash
dotnet restore

```

### 2. Aplique as migrations no banco relacional (se houver banco novo)

```bash
dotnet ef database update --project EcoWatch.Infrastructure --startup-project EcoWatch.Api

```

### 3. Inicie a API

```bash
dotnet run --project EcoWatch.Api

```

A API estará disponível por padrão em:

```txt
http://localhost:5015

```

A documentação interativa do Swagger pode ser acessada em:

```txt
http://localhost:5015/swagger

```

----------

## 📡 Visão Geral dos Endpoints

| Método | Rota | Descrição | Requer Auth |
|---|---|---|---|
| `POST` | `/api/auth/login` | Autentica um usuário e retorna o Token JWT. | ❌ Não |
| `GET` | `/api/ocorrencias` | Lista ocorrências de incêndio reportadas via App Móvel. | ✅ Sim |
| `POST` | `/api/ocorrencias` | Registra nova ocorrência e publica evento no RabbitMQ. | ✅ Sim |
| `POST` | `/api/satelites/telemetria` | Ingestão dinâmica de dados brutos para o MongoDB. | ✅ Sim |
| `GET` | `/health` | Relatório de status da infraestrutura (Oracle, Mongo, Fila). | ❌ Não |



## 🧩 Integrações (Ecossistema)

Este backend foi desenhado para se comunicar com as seguintes interfaces:

-   **Frontend Mobile (React Native):** Consome os endpoints REST para login de usuários e submissão de ocorrências geolocalizadas.

-   **Worker de Notificação (Java/Spring Boot):** Atua como _Consumer_ da fila `alertas_incendio_queue` no RabbitMQ, mapeando o payload de bytes e disparando alertas para a Defesa Civil e usuários afetados via SMS/Push.


----------

_Documentação gerada com foco em práticas de DevOps e Clean Architecture._