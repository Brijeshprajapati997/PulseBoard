# PulseBoard - 6-Week Cloud Native Build Guide

A hands-on project to learn Cloud Native architecture using **.NET Core 10 + Angular 18**.
Each week builds on the last - by week 6 you have a fully deployed, observable, real-time GitHub metrics dashboard.

---

## Project summary

**What you are building**: PulseBoard - a real-time dashboard that listens to GitHub webhooks,
processes events through microservices, stores metrics in PostgreSQL, and displays live charts
in Angular via SignalR.

**Solution layout**

```
PulseBoard/
+-- src/
|   +-- services/
|   |   +-- PulseBoard.WebhookService/
|   |   +-- PulseBoard.ApiService/
|   |   +-- PulseBoard.AnalyticsService/
|   |   +-- PulseBoard.NotificationService/
|   +-- shared/
|   |   +-- PulseBoard.Contracts/
|   +-- frontend/
|       +-- pulseboard-ui/
+-- infra/
|   +-- docker-compose.yml
|   +-- k8s/
|   +-- terraform/
+-- PulseBoard.sln
```

---

## Week 1 - Containers (Docker)

**Goal**: Every service runs in a Docker container. All containers talk to each other via docker-compose.

### What to build

1. Create the .NET solution with 4 service projects + 1 Contracts class library using `dotnet new`
2. Create the Angular app inside `src/frontend/pulseboard-ui` using `ng new`
3. Write a `Dockerfile` for each .NET service using the multi-stage build pattern
   - Stage 1: `mcr.microsoft.com/dotnet/sdk:10.0` to restore and publish
   - Stage 2: `mcr.microsoft.com/dotnet/aspnet:10.0` as the runtime image
4. Write a `Dockerfile` for Angular
   - Stage 1: `node:20` to run `npm install` and `ng build`
   - Stage 2: `nginx:alpine` to serve the dist output
5. Write `infra/docker-compose.yml` with one entry per service plus `rabbitmq` and `postgres`
6. Add a `healthcheck` block to each service entry in docker-compose
7. Confirm all containers start with `docker compose up` and can reach each other by service name

### What you learn

- Multi-stage Docker builds to keep images small
- Environment variable injection via docker-compose
- Container networking and DNS resolution by service name
- Volume mounts for PostgreSQL data persistence

### Done when

`docker compose up` starts all containers, and `curl http://localhost:5001/health` returns 200.

---

## Week 2 - Kubernetes (local)

**Goal**: Deploy the same containers to a local Kubernetes cluster using manifests you write by hand.

### What to build

1. Install minikube and kubectl on your machine
2. Write a `Deployment` manifest for each service under `infra/k8s/`
   - Set `replicas: 1` to start
   - Set resource `requests` and `limits` for CPU and memory
3. Write a `Service` manifest for each deployment
   - Internal services: `ClusterIP`
   - WebhookService and ApiService: `NodePort` so you can reach them from your browser
4. Write a `ConfigMap` for non-secret config (RabbitMQ host, DB name)
5. Write a `Secret` for the PostgreSQL password
6. Write an `Ingress` manifest that routes `/api/` to ApiService and `/webhooks/` to WebhookService
7. Apply all manifests with `kubectl apply -f infra/k8s/` and confirm all pods reach Running state

### What you learn

- The difference between a Deployment, Service, ConfigMap and Secret
- How pods discover each other via Kubernetes DNS
- How Ingress routes external traffic to internal services
- Reading pod logs and describing pods to debug issues

### Done when

`kubectl get pods` shows all pods Running and hitting the Ingress address routes correctly.

---

## Week 3 - Microservices and event-driven messaging

**Goal**: Services communicate through RabbitMQ events, not direct HTTP calls.

### What to build

1. Add the `PulseBoard.Contracts` project and define event records there
   - `PullRequestEvent`, `CommitEvent`, `IssueEvent` - each with an `OrgName` field
   - `OrgName` is populated from `organization.login` in the webhook payload (empty for personal repos)
2. Add MassTransit + RabbitMQ transport to WebhookService - publish the event when a webhook POST arrives
3. Add MassTransit to AnalyticsService as a hosted Worker
   - Consume events and write rows to PostgreSQL via EF Core
   - Store `org_name` column alongside `repo_name` to support org filtering later
4. Add MassTransit to NotificationService - consume events and send a Slack message via Incoming Webhooks
5. Add a SignalR Hub to ApiService - broadcast `MetricSavedEvent` to connected Angular clients
6. Add `GET /api/metrics?org=<orgName>` filter to ApiService
7. Update Angular to connect to the SignalR hub and update chart data in real time

### What you learn

- Async, decoupled communication using a message broker
- Consumer groups and competing consumers in RabbitMQ
- SignalR for server-to-browser push over WebSockets
- Each service owns its own schema

### Done when

Sending a test webhook POST causes the Angular dashboard to update live without a page refresh.

---

## Week 4 - CI/CD and Infrastructure as Code

**Goal**: Every push to main builds images, runs tests, and deploys to Kubernetes automatically.

### What to build

1. Write `.github/workflows/ci.yml` with three jobs: `build` (dotnet test), `docker` (build + push to ghcr.io), `deploy` (kubectl apply)
2. Write `infra/terraform/main.tf` to provision the cluster using the Kubernetes Terraform provider
3. Add a `GHCR_TOKEN` secret to your GitHub repository settings
4. Tag each image with the Git SHA so you can roll back to any commit
5. Add a `rollout restart` step in the deploy job so pods pick up the new image

### What you learn

- How a CI pipeline moves code from commit to running container
- Declarative infrastructure with Terraform
- GitOps: cluster state derived from files in the repo
- Rolling back a deployment using a previous image tag

### Done when

A `git push` to main triggers the pipeline and the new image is running in the cluster within 5 minutes.

---

## Week 5 - Observability

**Goal**: You can see what every service is doing without reading log files manually.

### What to build

1. Add `prometheus-net.AspNetCore` to each service and expose a `/metrics` endpoint with at least one custom counter
2. Deploy Prometheus to the cluster with a scrape config for each service
3. Create a Grafana dashboard with panels for request rate, event count per repo, error rate, memory usage
4. Add Serilog with the Loki sink to each service - include `repoName`, `orgName`, `eventType`, `traceId` as log properties
5. Deploy Grafana Loki and Promtail to the cluster
6. Add OpenTelemetry tracing using `OpenTelemetry.Extensions.Hosting` - instrument HttpClient and EF Core - export to Jaeger
7. Deploy Jaeger and confirm you can trace a webhook event end-to-end

### What you learn

- The three pillars of observability: metrics, logs, traces
- How Prometheus scrapes and stores time-series data
- How distributed tracing connects a request across multiple services
- Structured logging and log correlation using trace IDs

### Done when

You can see live metrics in Grafana, trace a webhook event in Jaeger, and search logs in Loki by `orgName` or `repoName`.

---

## Week 6 - Security and wrap-up

**Goal**: The system is hardened with auth, secrets management and network isolation.

### What to build

1. Add JWT authentication to ApiService - protect `/api/metrics` - add `/api/auth/token` for API key exchange
2. Update Angular to store the JWT and attach it as a Bearer header on every API call
3. Replace plain-text secrets in ConfigMap with Kubernetes Secrets (RabbitMQ password, PostgreSQL password, Slack URL)
4. Add a Kubernetes `NetworkPolicy` - allow only ApiService to PostgreSQL and WebhookService to RabbitMQ - deny all other pod-to-pod traffic
5. Validate the GitHub webhook signature (`X-Hub-Signature-256`) in WebhookService before publishing events
6. Register a **GitHub App** for org-wide access (see Organization Support section below)
7. Run `trivy` against your images and fix any critical CVEs

### What you learn

- JWT-based API authentication
- Kubernetes NetworkPolicy for zero-trust pod networking
- Secrets management as a first step toward Vault
- Webhook signature validation
- GitHub App authentication for org-level access
- Container image scanning

### Done when

An unauthenticated request to `/api/metrics` returns 401, all secrets are in K8s Secrets, and `trivy` reports no critical CVEs.

---

## Organization Support

By default PulseBoard tracks your personal repos. This section explains how to expand it to cover an entire GitHub organization.

### Personal account vs Organization

| | Personal | Organization |
|--|---------|-------------|
| Webhook scope | Your repos only | All repos in the org |
| GitHub token | Personal Access Token | GitHub App installation token |
| New repos | Must add webhook manually | Covered automatically |
| Access control | Just you | Org admins control it |

### What is already in the codebase

Every event record in `PulseBoard.Contracts` carries an `OrgName` field:

```csharp
// src/shared/PulseBoard.Contracts/Events.cs
public record PullRequestEvent(
    string OrgName,   // e.g. "dyad-tech" — empty string for personal repos
    string RepoName,
    string Action,
    string Author,
    int Number,
    DateTimeOffset OccurredAt
);
```

The same `OrgName` field is on `CommitEvent` and `IssueEvent`.

The WebhookService populates it from `organization.login` in the GitHub webhook payload.
The AnalyticsService stores it as `org_name` in PostgreSQL.
The ApiService supports `GET /api/metrics?org=dyad-tech` to filter by org.

### How to set up an org-level webhook (Step 3 above, done in Week 6)

**Step 1 - Register a GitHub App**
GitHub -> Settings -> Developer Settings -> GitHub Apps -> New GitHub App.
Grant it `Webhooks (read)`, `Pull requests (read)`, `Contents (read)` on the organization.

**Step 2 - Install the app on your org**
Install it on the target organization. GitHub issues an installation token that
WebhookService uses to authenticate API calls on behalf of the org.

**Step 3 - Add a single org-level webhook**
Go to your GitHub Org -> Settings -> Webhooks -> Add webhook.

```
Payload URL : https://<your-domain>/webhooks/github
Content type: application/json
Secret      : <your GITHUB__WebhookSecret value>
Events      : Pull requests, Pushes, Issues
```

One webhook covers every repo in the org, including repos added in the future.

**Step 4 - Test locally with ngrok**
Use ngrok to expose your local WebhookService while testing (no deployment needed).

```bash
ngrok http 5001
# Copy the https://xxxx.ngrok.io URL and use it as the webhook Payload URL on GitHub
```

### Environment variables for org mode

| Variable | Service | Description |
|----------|---------|-------------|
| `GitHub__AppId` | WebhookService | GitHub App ID |
| `GitHub__PrivateKeyPath` | WebhookService | Path to the App .pem key |
| `GitHub__InstallationId` | WebhookService | Installation ID for the target org |
| `GitHub__WebhookSecret` | WebhookService | Used to validate X-Hub-Signature-256 |

---

## Progress tracker

| Week | Topic | Status |
|------|-------|--------|
| 1 | Docker + docker-compose | Done |
| 2 | Kubernetes manifests | Not started |
| 3 | Microservices + RabbitMQ + SignalR | Not started |
| 4 | CI/CD + Terraform | Not started |
| 5 | Observability | Not started |
| 6 | Security + GitHub App (org support) | Not started |

---

## Useful commands reference

```bash
# Scaffold solution
dotnet new sln -n PulseBoard
dotnet new webapi -n PulseBoard.WebhookService -o src/services/PulseBoard.WebhookService
dotnet new worker  -n PulseBoard.AnalyticsService -o src/services/PulseBoard.AnalyticsService
dotnet new classlib -n PulseBoard.Contracts -o src/shared/PulseBoard.Contracts
ng new pulseboard-ui --directory src/frontend/pulseboard-ui --routing --style scss

# Docker
docker compose up --build
docker compose logs -f webhook-service

# Kubernetes
kubectl apply -f infra/k8s/
kubectl get pods -n pulseboard
kubectl logs deploy/webhook-service -n pulseboard
kubectl describe pod <name> -n pulseboard

# Terraform
cd infra/terraform && terraform init && terraform apply

# Test org webhook locally
ngrok http 5001
```
