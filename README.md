# 🚀 MeetMindAI

> AI-powered Meeting Notes Assistant built with **ASP.NET Core 8**, **WPF**, **Clean Architecture**, **CQRS**, **PostgreSQL**, and **Google Gemini AI**.

MeetMindAI is a desktop-first productivity application that helps users organize meetings, store transcripts, generate AI-powered summaries, extract actionable tasks, and manage meeting attachments through a clean, scalable architecture.

---

## 🎯 Why MeetMindAI?

Meeting notes are often scattered across documents,
emails, and chat applications.

MeetMindAI centralizes the entire workflow by combining
meeting management with AI-powered summarization,
automatic task extraction, and attachment management
into a single desktop application.

## ✨ Features

- 🔐 JWT Authentication with Refresh Tokens
- 👤 User Account Management
- 📅 Meeting Management
- 📝 Transcript Management
- 🤖 AI Meeting Summaries
- ✅ AI Action Item Extraction
- 📎 Meeting Attachments
- 🧱 Clean Architecture
- ⚡ CQRS with MediatR
- 🗄 PostgreSQL + Entity Framework Core
- 🖥 WPF Desktop Client
- 📄 Swagger API Documentation

## 🚀 Roadmap

- Azure Blob Storage
- Docker Support
- CI/CD Pipeline
- Audio Recording
- Whisper Speech-to-Text
- Google Calendar Integration
- Outlook Integration
- Email Summaries
- Team Collaboration


![.NET](https://img.shields.io/badge/.NET-8.0-purple)

![WPF](https://img.shields.io/badge/WPF-Desktop-blue)

![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-blue)

![Entity Framework Core](https://img.shields.io/badge/EF_Core-8.0-green)

![CQRS](https://img.shields.io/badge/Architecture-CQRS-orange)

![Clean Architecture](https://img.shields.io/badge/Clean-Architecture-success)

![License](https://img.shields.io/badge/License-MIT-yellow)

![AI](https://img.shields.io/badge/Google-Gemini-red)

## 📖 Overview

MeetMindAI is an enterprise-grade meeting management system designed to simplify the entire meeting lifecycle.

Users can:

- Create and organize meetings
- Upload meeting transcripts
- Generate AI-powered meeting summaries
- Extract action items automatically
- Manage meeting attachments
- Securely authenticate using JWT
- Access everything through a modern WPF desktop application

The solution follows Clean Architecture principles to maintain a clear separation of concerns, making the codebase scalable, testable, and maintainable.


## 🛠 Technology Stack

| Category | Technology |
|----------|------------|
| Frontend | WPF (.NET 8) |
| Backend | ASP.NET Core Web API |
| Architecture | Clean Architecture |
| Pattern | CQRS + MediatR |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL |
| Authentication | JWT + Refresh Tokens |
| AI | Google Gemini |
| Storage | Local File Storage (Azure-ready) |
| Validation | FluentValidation |
| Logging | Serilog |
| Documentation | Swagger |
| Testing | xUnit |

## 🌟 Key Features

Core Features

Authentication
Meeting Management
Transcript Management

AI Features

Meeting Summaries
Action Item Extraction

Storage

Attachments

Architecture

CQRS
Repository Pattern
Clean Architecture

---

### 🤖 AI Meeting Summary

Generate concise summaries using Google Gemini AI.

---

### ✅ AI Action Items

Automatically extract:

- Tasks
- Priorities
- Due Dates
- Completion Status

---

### 📎 Attachments

- Upload files
- Download files
- Delete files

## 📁 Project Structure

```text
MeetMindAI
│
├── docs/
│   ├── architecture.md
│   ├── api.md
│   ├── database.md
│   ├── deployment.md
│   └── screenshots/
│
├── src/
│   ├── MeetMindAI.API
│   ├── MeetMindAI.Application
│   ├── MeetMindAI.Domain
│   ├── MeetMindAI.Infrastructure
│   ├── MeetMindAI.Persistence
│   ├── MeetMindAI.Shared
│   └── MeetMindAI.WPF
│
├── tests/
│   └── MeetMindAI.Application.Tests
│
├── README.md
├── LICENSE
└── .gitignore
```

## 🏛 Clean Architecture

MeetMindAI follows **Clean Architecture** to ensure maintainability, scalability, and testability.

```
              +----------------+
              |   WPF Client   |
              +--------+-------+
                       |
                       |
              ASP.NET Core API
                       |
          +------------+-------------+
          |                          |
     Application Layer (CQRS)
          |
     Domain Layer
          |
 Persistence + Infrastructure
          |
     PostgreSQL
          |
     Google Gemini AI
```

### Layer Responsibilities

### API / WPF

- Handles user interaction
- Receives requests
- Returns responses

### Application

- Business use cases
- Commands & Queries
- Validation
- Result Pattern
- MediatR

### Domain

- Entities
- Business Rules
- Domain Events
- Errors

### Persistence

- EF Core
- PostgreSQL
- Repository Implementations

### Infrastructure

- JWT
- Gemini AI
- File Storage
- External Services

## 🧩 Design Patterns

MeetMindAI uses several industry-standard design patterns.

| Pattern | Purpose |
|----------|----------|
| Clean Architecture | Separation of concerns |
| CQRS | Separate reads from writes |
| Repository Pattern | Abstract data access |
| Dependency Injection | Loose coupling |
| Result Pattern | Consistent error handling |
| Factory Methods | Controlled entity creation |
| Options Pattern | Strongly typed configuration |
| Fluent Validation | Request validation |
| MediatR | Decoupled request handling |

## 🗄 Database

The application uses PostgreSQL with Entity Framework Core.

### Main Tables

- Users
- RefreshTokens
- Meetings
- Transcripts
- MeetingSummaries
- ActionItems
- MeetingAttachments

### Relationships

User
│
└── Meetings
      │
      ├── Transcript (1:1)
      │
      ├── Summary (1:1)
      │
      ├── Attachments (1:N)
      │
      └── ActionItems (1:N)


## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL
- Visual Studio 2022
- Git

---

### Clone Repository

```bash
git clone https://github.com/<your-username>/MeetMindAI.git

cd MeetMindAI
```

---

### Restore Packages

```bash
dotnet restore
```

---

### Apply Database Migrations

```bash
dotnet ef database update
```

---

### Run API

```bash
dotnet run --project src/MeetMindAI.API
```

---

### Run WPF

Open the solution in Visual Studio and start the **MeetMindAI.WPF** project.

## ⚙ Configuration

Sensitive values should not be committed to source control.

Configure the following using **User Secrets** or environment variables:

- Database Connection String
- JWT Secret Key
- Gemini API Key

Example configuration:

```json
{
  "ConnectionStrings": {
    "Database": ""
  },

  "Jwt": {
    "Issuer": "",
    "Audience": "",
    "SecretKey": ""
  },

  "Gemini": {
    "ApiKey": ""
  }
}
```

## 📡 API

The REST API includes endpoints for:

- Authentication
- User Profile
- Meetings
- Transcripts
- AI Summaries
- AI Action Items
- Attachments

Interactive API documentation is available through **Swagger** when running the API locally.

## 🧪 Testing

The project includes unit tests using xUnit.

Run tests:

```bash```
dotnet test


---

# 8. Add Deployment

```md```

## 🚀 Deployment

Backend

ASP.NET Core Web API

Database

PostgreSQL

Desktop

WPF (.NET 8)

Storage

Local Storage (Azure-ready)
