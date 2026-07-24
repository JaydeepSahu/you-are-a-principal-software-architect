# Azure Kubernetes Service (AKS) Production Deployment Guide

This guide details the production deployment process for the Enterprise AI Platform on Azure Kubernetes Service (AKS) using Workload Identity, Azure Key Vault Secrets CSI Provider, and Azure Container Registry (ACR).

---

## 1. Prerequisites & Resource Provisioning

### Create Resource Group & ACR
```bash
az group create --name rg-enterprise-ai-prod --location eastus
az acr create --resource-group rg-enterprise-ai-prod --name acrenterpriseaiprod --sku Premium
```

### Provision Production AKS Cluster
```bash
az aks create \
  --resource-group rg-enterprise-ai-prod \
  --name aks-enterprise-ai-prod \
  --node-count 3 \
  --enable-cluster-autoscaler \
  --min-count 3 \
  --max-count 20 \
  --node-vm-size Standard_D4s_v5 \
  --network-plugin azure \
  --enable-oidc-issuer \
  --enable-workload-identity \
  --enable-secret-store-csi-driver \
  --attach-acr acrenterpriseaiprod \
  --generate-ssh-keys
```

---

## 2. Secrets Management with Azure Key Vault & Workload Identity

1. **Create Key Vault**:
```bash
az keyvault create --name kv-enterprise-ai-prod --resource-group rg-enterprise-ai-prod --location eastus
```

2. **Store Provider Keys & JWT Secrets**:
```bash
az keyvault secret set --vault-name kv-enterprise-ai-prod --name "azure-openai-key" --value "SECURE_KEY"
az keyvault secret set --vault-name kv-enterprise-ai-prod --name "anthropic-key" --value "SECURE_KEY"
az keyvault secret set --vault-name kv-enterprise-ai-prod --name "jwt-secret-key" --value "SUPER_SECURE_JWT_SECRET_32_BYTES_MIN"
```

3. **Configure CSI SecretProviderClass**:
Create `SecretProviderClass` in `deployments/aks/secret-provider-class.yaml`:
```yaml
apiVersion: secrets-store.csi.k8s.io/v1
kind: SecretProviderClass
metadata:
  name: azure-kv-secrets
  namespace: enterprise-ai-platform
spec:
  provider: azure
  parameters:
    usePodIdentity: "false"
    useVMManagedIdentity: "false"
    userAssignedIdentityID: "<client-id>"
    keyvaultName: "kv-enterprise-ai-prod"
    cloudName: ""
    objects: |
      array:
        - |
          objectName: azure-openai-key
          objectType: secret
        - |
          objectName: anthropic-key
          objectType: secret
        - |
          objectName: jwt-secret-key
          objectType: secret
    tenantId: "<azure-tenant-id>"
```

---

## 3. Helm Production Release

```bash
# Get AKS credentials
az aks get-credentials --resource-group rg-enterprise-ai-prod --name aks-enterprise-ai-prod

# Create production namespace
kubectl create namespace enterprise-ai-platform

# Install Helm release
helm upgrade --install enterprise-ai ./deployments/helm/enterprise-ai-platform \
  --namespace enterprise-ai-platform \
  --values ./deployments/helm/enterprise-ai-platform/values.yaml \
  --set image.repository=acrenterpriseaiprod.azurecr.io/enterprise-ai/aigateway-api \
  --set image.tag=v1.0.0
```
