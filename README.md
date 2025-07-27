# Sistema de Cupons de Desconto

Uma API REST desenvolvida em C# .NET 9 para gerenciar cupons de desconto com regras customizáveis e aplicação no checkout.

## 🚀 Funcionalidades

- **Criar cupons** com desconto fixo ou percentual
- **Regras configuráveis**: valor mínimo, data de expiração, uso único por cliente
- **Aplicação de cupons** com validações automáticas
- **Registro de uso** para auditoria e controle
- **Arquitetura extensível** para novas regras de negócio

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design**:

```
Landia.Api              # Camada de apresentação (Minimal APIs)
Landia.Core             # Camada de domínio (Entidades, Serviços, Interfaces)
Landia.Infrastructure   # Camada de infraestrutura (EF Core, Repositórios)
Landia.UnitTests        # Testes unitários
Landia.IntegrationTests # Testes de integração
```

## 🛠️ Tecnologias

- **.NET 9** - Framework principal
- **ASP.NET Core Minimal APIs** - Endpoints REST
- **Entity Framework Core** - ORM e persistência
- **SQL Server** - Banco de dados
- **xUnit + Moq + FluentAssertions** - Testes
- **Swagger** - Documentação da API

## 📋 Pré-requisitos

- .NET 9 SDK
- Visual Studio 2022
- SQL Server (opcional)

## 🚀 Como executar

1. **Clone o repositório**
```bash
git clone <repository-url>
cd Landia
```

2. **Restaure as dependências**
```bash
dotnet restore
```

3. **Execute a aplicação**
```bash
dotnet run --project src/Landia.Api
```

4. **Acesse o Swagger**
```
https://localhost:5285
```

## 🧪 Executar testes

**Testes unitários:**
```bash
dotnet test tests/Landia.UnitTests
```

**Testes de integração:**
```bash
dotnet test tests/Landia.IntegrationTests
```

**Todos os testes:**
```bash
dotnet test
```

## 📖 Exemplos de uso

### Criar cupom
```http
POST /api/coupons
{
    "code": "WELCOME10",
    "discountType": 2,
    "discountValue": 10.0,
    "minimumOrderValue": 50.0,
    "isUniquePerCustomer": true
}
```

### Aplicar cupom
```http
POST /api/coupons/apply
{
    "couponCode": "WELCOME10",
    "customerId": "customer123",
    "orderValue": 100.0
}
```

## 🔒 Segurança implementada

- Validação de entrada em todos os endpoints
- Prevenção contra reutilização indevida de cupons
- Logs de auditoria para todas as operações
- Middleware de tratamento de exceções
- Validação de regras de negócio

## 🎯 Padrões aplicados

- **Repository Pattern** - Abstração do acesso a dados
- **Strategy Pattern** - Validadores de regras extensíveis
- **Dependency Injection** - Inversão de controle
- **TDD** - Desenvolvimento orientado por testes
- **Clean Code** - Código limpo e legível

## 📁 Estrutura de pastas detalhada

```
src/
├── Landia.Api/
│   ├── Endpoints/           # Definição dos endpoints REST
│   ├── Middleware/          # Middlewares customizados
│   └── Program.cs           # Configuração da aplicação
│
├── Landia.Core/
│   ├── Entities/           # Entidades do domínio
│   ├── Enums/              # Enumerações
│   ├── Interfaces/         # Contratos e abstrações
│   ├── Services/           # Lógica de negócio
│   ├── Validators/         # Validadores de regras
│   └── DTOs/               # Objetos de transferência
│
└── Landia.Infrastructure/
       ├── Data/               # Contexto do EF Core
       └── Repositories/       # Implementação dos repositórios

tests/
├── Landia.IntegrationTests   # Testes de integração
│   
└── Landia.UnitTests
       ├── Services/         # Testes dos serviços
       └── Validators/       # Testes de validação
```
