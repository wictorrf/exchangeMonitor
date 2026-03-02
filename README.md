# 💹 ExchangeMonitor (BRL/ARS)

> **Resilient .NET 8 Worker Service** designed to monitor exchange rate fluctuations between Brazilian Real (BRL) and Argentine Peso (ARS). Built with a focus on high availability, clean code, and enterprise-grade patterns.

---

## 🛠️ Tech Stack & Patterns

- **Framework:** .NET 8 (Worker Services)
- **Language:** C#
- **Patterns:** Clean Architecture, CQRS, Outbox Pattern
- **Libraries:** MediatR, Polly (Resilience), xUnit, Moq
- **Cloud Ready:** Infrastructure prepared for Azure (Key Vault & SQL Database)

---

## 🚀 Key Features

### 🛡️ Resilience with Polly
The system implements advanced transient-fault-handling using **Polly**. This ensures that even if the external Exchange API is temporarily unstable, the service will perform retries with exponential backoff before failing.

### 📦 Outbox Pattern
To ensure data consistency, we use the **Outbox Pattern**. This guarantees that exchange rate alerts are reliably captured and processed, preventing data loss during messaging or database fluctuations.

### 🏛️ Clean Architecture & CQRS
The project is decoupled into four main layers to ensure maintainability:
- **Domain**: Pure business logic and entities.
- **Application**: CQRS Handlers (Commands/Queries) powered by **MediatR**.
- **Infrastructure**: Implementation of external services and resilience policies.
- **Worker**: The background execution engine.

---

## 📂 Project Structure

```bash
ExchangeMonitor/
├── ExchangeMonitor.Domain/         # Core business rules & entities
├── ExchangeMonitor.Application/    # CQRS Handlers, DTOs & Logic
├── ExchangeMonitor.Infrastructure/ # External APIs & Polly Policies
└── ExchangeMonitor.Worker/         # Background Service execution
