# PulseBoard

A real-time GitHub metrics dashboard built with **.NET Core 10 + Angular 18**, designed as a hands-on Cloud Native learning project covering containers, Kubernetes, microservices, CI/CD, observability, and security.

---

## What it does

PulseBoard listens to GitHub webhooks, processes events through microservices, stores metrics in PostgreSQL, and displays live charts in an Angular dashboard via SignalR — all running on Kubernetes.

```
GitHub → Webhook → RabbitMQ → Analytics → PostgreSQL
                             → Notification → Slack
                   API ←────────────────── SignalR
                    ↑
               Angular UI
```

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend services | ASP.NET Core 10 (Web API + Worker) |
| Frontend | Angular 18 + Angular Material |
| Real-time | SignalR |
| Messaging | RabbitMQ via MassTransit |
| Database | PostgreSQL + Entity Framework Core |
| Auth | JWT Bearer tokens |
| Containers | Docker + docker-compose |
| Orchestration | Kubernetes (minikube) |
| CI/CD | GitHub Actions |
| IaC | Terraform |
| Metrics | Prometheus + Grafana |
| Tracing | OpenTelemetry + Jaeger |
| Logging | Serilog + Loki |

---

## Project Structure

```
PulseBoard/
├── src/
│   ├── services/
│   │   ├── PulseBoard.WebhookService/      # Receives GitHub webhooks
│   │   ├── PulseBoard.ApiService/          # REST API + SignalR hub
│   │   ├── PulseBoard.AnalyticsService/    # Processes events, writes metrics
│   │   └── PulseBoard.NotificationService/ # Sends Slack/email alerts
│   ├── shared/
│   │   └── PulseBoard.Contracts/           # Shared event contracts (DTOs)
│   └── frontend/
│       └── pulseboard-ui/                  # Angular 18 dashboard
├── infra/
│   ├── docker-compose.yml                  # Local multi-container setup
│   ├── k8s/                                # Kubernetes manifests
│   └── terraform/                          # Infrastructure as Code
└── .github/workflows/ci.yml               # GitHub Actions CI pipeline
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [minikube](https://minikube.sigs.k8s.io/docs/start/) (Week 2+)
- [kubectl](https://kubernetes.io/docs/tasks/tools/) (Week 2+)
- [Terraform](https://developer.hashicorp.com/terraform/install) (Week 4+)

### Run locally with docker-compose

```bash
git clone https://github.com/Brijeshprajapati997/PulseBoard.git
cd PulseBoard/infra

docker compose up --build
```

| Service | URL |
|---------|-----|
| Webhook Service | http://localhost:5001 |
| API Service | http://localhost:5002 |
| Angular UI | http://localhost:4200 |
| RabbitMQ Management | http://localhost:15672 |

Verify health:

```bash
curl http://localhost:5001/health
curl http://localhost:5002/health
```

### Build & test .NET solution

```bash
dotnet restore PulseBoard.sln
dotnet build PulseBoard.sln
dotnet test PulseBoard.sln
```

---

## 6-Week Build Plan

| Week | Topic | Status |
|------|-------|--------|
| 1 | Docker + docker-compose | ✅ Done |
| 2 | Kubernetes manifests (minikube) | 🔲 Upcoming |
| 3 | Microservices + RabbitMQ + SignalR | 🔲 Upcoming |
| 4 | CI/CD (GitHub Actions) + Terraform | 🔲 Upcoming |
| 5 | Observability (Prometheus, Grafana, Jaeger) | 🔲 Upcoming |
| 6 | Security (JWT, NetworkPolicy, secrets) | 🔲 Upcoming |

> Full step-by-step guide: [PulseBoard Build Guide](https://claude.ai/artifact/EnfKBNzmQJivTq8pXfLqDn)

---

## Services

### WebhookService (port 5001)
Receives `POST /webhooks/github` from GitHub. Validates the `X-Hub-Signature-256` header (Week 6) and publishes events to RabbitMQ.

### ApiService (port 5002)
Exposes `GET /api/metrics` for the Angular dashboard and hosts the SignalR hub at `/hubs/metrics` for real-time updates.

### AnalyticsService
Background Worker that consumes events from RabbitMQ and writes aggregated metrics to PostgreSQL.

### NotificationService
Background Worker that consumes events from RabbitMQ and sends Slack alerts when thresholds are crossed.

---

## Environment Variables

| Variable | Service | Description |
|----------|---------|-------------|
| `RabbitMq__Host` | All services | RabbitMQ hostname |
| `ConnectionStrings__Default` | ApiService, AnalyticsService | PostgreSQL connection string |
| `Slack__WebhookUrl` | NotificationService | Slack incoming webhook URL |

---

## Contributing

This is a personal learning project. Each week's work is committed on `main` with a `feat(weekN):` prefix.

---

## License

MIT
