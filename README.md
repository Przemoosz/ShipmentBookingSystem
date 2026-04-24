# Shipment Booking System

A professional shipment management service demonstrating modern .NET practices with Wolverine, Kafka, SQL Server, and Dapper.

## 📌 Overview

Shipment Booking System implements a complete shipment booking service with:

- **REST API** for shipment creation and reporting
- **Transactional messaging** - database writes and Kafka events are atomic
- **Server-side aggregation** using SQL with JSON shaping via Dapper
- **Validation** both synchronous and asynchronous
- **Structured logging** and error handling
- **Docker Compose** infrastructure (SQL Server 2022 + Apache Kafka)

## 🏗️ Architecture

Layered architecture with clean separation of concerns:

```
Presentation (HTTP Endpoints)
     ↓
Application (Handlers, Queries, Validators)
     ↓
Domain (Entities, Events, Models)
     ↓
Infrastructure (Database, Repository, UnitOfWork)
```

## 🎯 Design Patterns

- **CQRS** - Commands (SaveShipmentRequest) and Queries (ShipmentSummaryQuery) separated
- **Mediator** - Communication via IMessageBus (Wolverine)
- **Repository** - Data access abstraction with IShipmentRepository
- **Unit of Work** - Transaction management ensuring DB + Kafka atomicity
- **Layered Architecture** - Clear separation: Presentation → Application → Domain → Infrastructure

## 🚀 Quick Start

### Prerequisites
- Docker Desktop
- .NET 10 SDK
- Windows 10/11 or WSL2

### Run Application

```bash
docker-compose up -d
```

## 📡 API Endpoints

### POST /shipments - Create Shipment

```http
POST http://localhost:8080/shipments
Content-Type: application/json

{
  "customerId": 123,
  "shipmentNumber": "SHP-2026-001",
  "items": [
    { "productCode": "TV", "quantity": 2, "unitPrice": 2500.00 },
    { "productCode": "CABLE", "quantity": 5, "unitPrice": 50.00 }
  ]
}
```

**Response:** `201 Created`

**Process:**
1. Validate request (sync + async)
2. Save shipment & items to SQL Server
3. Publish `ShipmentCreatedEvent` to Kafka (atomic with DB)
4. Return 201

**Validation:**
- `shipmentNumber` required, unique
- `customerId` required
- `items` minimum 1, `quantity` > 0, `unitPrice` > 0

### GET /shipments/summary - Report

```http
GET http://localhost:8080/shipments/summary?customerId=123&createdFrom=2026-01-01&createdTo=2026-03-31&minTotalAmount=10000&minShipments=2
```

**Response:**
```json
[
  {
    "customerId": 123,
    "shipmentsCount": 3,
    "totalAmount": 12000.00,
    "products": [
      { "productCode": "TV", "totalQuantity": 4 },
      { "productCode": "CABLE", "totalQuantity": 10 }
    ]
  }
]
```

**Aggregation:**
- SQL Server-side grouping by customer
- JSON shaping in T-SQL
- Mapped via Dapper
- Filters: customerId, createdFrom, createdTo, minTotalAmount, minShipments

## 🛠️ Tech Stack

- **.NET 10** - Framework
- **Wolverine** - CQRS & Messaging
- **Apache Kafka** - Event streaming
- **MS SQL Server 2022** - Database
- **Dapper** - Lightweight ORM for queries
- **FluentValidation** - Request validation
- **xUnit 3.X** - Integration tests

## 📁 Project Structure

```
src/
├── ShipmentBookingSystem.Api/
│   └── Program.cs                    # Wolverine, Kafka config
├── ShipmentBookingSystem.Presentation/
│   └── Endpoints/ShipmentEndpoints.cs  # HTTP endpoints
├── ShipmentBookingSystem.Application/
│   ├── Handlers/                     # CQRS handlers
│   ├── Queries/                      # Query definitions
│   ├── Requests/                     # Command definitions
│   └── Validators/                   # FluentValidation rules
├── ShipmentBookingSystem.Domain/
│   ├── Entities/                     # Shipment, ShipmentItem
│   ├── Events/                       # ShipmentCreatedEvent
│   └── Models/                       # ShipmentSummary
└── ShipmentBookingSystem.Infrastructure/
    ├── UnitOfWork.cs                 # Transaction management
    ├── Repository/                   # Data access
    └── Database/                     # Schema, migrations

tests/
└── ShipmentBookingSystem.IntegrationTests/  # xUnit + TestContainers
```