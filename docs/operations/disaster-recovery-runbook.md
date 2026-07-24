# Enterprise AI Platform - Disaster Recovery & Multi-Region Failover Runbook

This operational runbook provides step-by-step instructions, automation scripts, and validation procedures for executing a Disaster Recovery (DR) failover of the **Enterprise AI Platform** from the primary region (`eastus2`) to the secondary standby region (`westus2`).

---

## 1. High Availability & DR Overview

| Parameter | Target SLA | Strategy / Solution |
|---|---|---|
| **Recovery Time Objective (RTO)** | $< 5$ minutes | Automated DNS rerouting + Standby Pod scaling |
| **Recovery Point Objective (RPO)** | $< 1$ minute | Asynchronous PostgreSQL Flexible Server Geo-Replication |
| **Primary Region** | `eastus2` | Active AKS Cluster + Primary DB + Primary Redis |
| **Secondary Region** | `westus2` | Hot Standby AKS Cluster + Read Replica DB + Secondary Redis |
| **Traffic Router** | Global | Azure Front Door / Azure Traffic Manager |

---

## 2. Multi-Region Architecture Diagram

```
                       [ User / Client Requests ]
                                   |
                       [ Azure Front Door / Traffic Manager ]
                                   |
         +-------------------------+-------------------------+
         | (Active - 100% Traffic)                           | (Passive - Standby)
         v                                                   v
+-------------------------------+                 +-------------------------------+
| Primary Region: East US 2     |                 | Secondary Region: West US 2   |
|                               |                 |                               |
| [ AKS Cluster - Primary ]     |                 | [ AKS Cluster - Standby ]     |
|  - AI Gateway (Replica 3)     |                 |  - AI Gateway (Replica 1)     |
|  - Agent API (Replica 2)      |                 |  - Agent API (Replica 1)      |
|  - Portal BFF                 |                 |  - Portal BFF                 |
|                               |                 |                               |
| [ Azure DB for PostgreSQL ]   |== Geo-Sync ==>  | [ Azure DB for PostgreSQL ]   |
|  (Primary RW)                 |                 |  (Read Replica RO)            |
|                               |                 |                               |
| [ Azure Cache for Redis ]     |== Active-Pass=> | [ Azure Cache for Redis ]     |
|  (Primary Cluster)            |                 |  (Secondary Replica)          |
+-------------------------------+                 +-------------------------------+
```

---

## 3. Automated Failover Execution

In the event of a primary region outage or infrastructure impairment in `eastus2`, run the automated failover script:

```powershell
# Execute region failover from Primary (East US 2) to Secondary (West US 2)
.\deployments\dr\failover-primary-to-secondary.ps1 -TargetRegion "westus2" -ResourceGroup "rg-enterprise-ai-prod"
```

### What the Script Executes Automatically:

1. **Promotes Secondary PostgreSQL Database**:
   Promotes `westus2` PostgreSQL read-replica to standalone read-write primary.
2. **Updates Azure Front Door Routing**:
   Switches 100% of global ingress traffic from `eastus2` to `westus2`.
3. **Scales Standby Kubernetes Pods**:
   Scales AKS deployments in `westus2` from 1 standby replica to 3+ production replicas.
4. **Verifies Health Endpoints**:
   Polls `/health/live` and `/health/ready` on the secondary API endpoints until healthy.

---

## 4. Manual Failover Procedure (Step-by-Step)

If automated scripts are unavailable, perform manual failover:

### Step 1: Promote Secondary Database
```bash
az postgres flexible-server replica stop-replication \
  --resource-group rg-enterprise-ai-prod \
  --name ps-enterprise-ai-westus2
```

### Step 2: Reroute Azure Front Door Endpoint
```bash
az afd origin-group update \
  --resource-group rg-enterprise-ai-prod \
  --profile-name afd-enterprise-ai \
  --origin-group-name og-gateway \
  --selected-origin-name origin-westus2
```

### Step 3: Scale Secondary AKS Pods
```bash
az aks get-credentials --resource-group rg-enterprise-ai-prod --name aks-enterprise-ai-westus2

kubectl scale deployment aigateway-api --replicas=3 -n enterprise-ai-platform
kubectl scale deployment agents-api --replicas=3 -n enterprise-ai-platform
```

---

## 5. Failback Procedure (Restoring Primary Region)

Once `eastus2` health is restored:

1. Re-establish PostgreSQL geo-replication from `westus2` back to `eastus2`.
2. Run multi-region health check:
   ```powershell
   .\deployments\dr\health-check-regions.ps1
   ```
3. Gradually shift traffic back using Azure Front Door weighted traffic split (90/10 $\rightarrow$ 50/50 $\rightarrow$ 0/100).
