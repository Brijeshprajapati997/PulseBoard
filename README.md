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

> Full step-by-step guide: [PulseBoard Build Guide](PulseBoard-6-Week-Cloud-Native-Build-Guide.md)

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

## Organization-wide Metrics

By default PulseBoard tracks your personal repos. To track an **entire GitHub organization**:

### Personal account vs Organization

| | Personal | Organization |
|--|---------|-------------|
| Webhook scope | Your repos only | All repos in the org |
| GitHub token | Personal Access Token | GitHub App installation token |
| New repos | Must add webhook manually | Covered automatically |

### How to enable org-wide tracking

**Step 1 — Register a GitHub App** (Week 6)
Go to GitHub → Settings → Developer Settings → GitHub Apps → New GitHub App. Grant it `Webhooks (read)`, `Pull requests (read)`, `Contents (read)` on the org.

**Step 2 — Install the app on your org**
Install it on the target organization. GitHub issues an installation token that the WebhookService uses to call the API on behalf of the org.

**Step 3 — Add an org-level webhook**
Go to your GitHub Org → Settings → Webhooks → Add webhook.
```
Payload URL : https://<your-domain>/webhooks/github
Content type: application/json
Events      : Pull requests, Pushes, Issues
```
One webhook covers every repo in the org, including new ones added later.

**Step 4 — Filter by org in the dashboard**
Every event contract already carries `OrgName` (populated from `organization.login` in the webhook payload). The API supports `GET /api/metrics?org=your-org-name` and the Angular UI has an org selector dropdown.

### Event contracts with OrgName

```csharp
// PulseBoard.Contracts/Events/Events.cs
public record PullRequestEvent(
    string OrgName,   // e.g. "dyad-tech" — empty string for personal repos
    string RepoName,
    string Action,
    string Author,
    int Number,
    DateTimeOffset OccurredAt
);
```

### Environment variables for org mode

| Variable | Service | Description |
|----------|---------|-------------|
| `GitHub__AppId` | WebhookService | GitHub App ID |
| `GitHub__PrivateKeyPath` | WebhookService | Path to the App's .pem key |
| `GitHub__InstallationId` | WebhookService | Installation ID for the target org |
| `GitHub__WebhookSecret` | WebhookService | Webhook secret for signature validation |

---

## Contributing

This is a personal learning project. Each week's work is committed on `main` with a `feat(weekN):` prefix.

---

## License

MIT
