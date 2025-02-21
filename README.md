# 🚀 ABC School

Uma aplicação desenvolvida em **.NET 8** com arquitetura robusta e escalável, seguindo os princípios da **Clean Architecture**. O projeto utiliza **Entity Framework Core** para persistência de dados, **MediatR** para mediadores de comandos e eventos, autenticação via **JWT Tokens** e suporte a **Multi-Tenant**.

---

## 🛠️ Tecnologias Utilizadas

- [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR)
- [JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt)
- Multi-Tenant Architecture

---

## 📂 Estrutura do Projeto

```
├── src
│   ├── Application           # Casos de uso, comandos e queries (MediatR)
│   ├── Domain                # Entidades e interfaces
│   ├── Infrastructure        # Persistência de dados (Entity Framework)
│   ├── WebAPI                # Controllers e configuração da API
│   ├── TenantManagement      # Lógica de Multi-Tenant
│
├── tests                     # Testes unitários e de integração
│
├── README.md                 # Documentação do projeto
└── ...
```

---
