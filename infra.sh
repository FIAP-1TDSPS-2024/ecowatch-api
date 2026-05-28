#!/bin/bash
set -e

# ─── Variáveis ───────────────────────────────────────────────────────────────
RESOURCE_GROUP="rg-fire-shield"
LOCATION="canadacentral"
APP_SERVICE_PLAN="asp-fire-shield"
WEB_APP_NAME="app-fire-shield"
RUNTIME="DOTNETCORE:8.0"
SKU="B1"

# ─── Login (opcional — comente se já estiver autenticado) ─────────────────────
# az login

# ─── Grupo de Recursos ───────────────────────────────────────────────────────
echo "Criando grupo de recursos: $RESOURCE_GROUP..."
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION"

# ─── App Service Plan ────────────────────────────────────────────────────────
echo "Criando App Service Plan: $APP_SERVICE_PLAN..."
az appservice plan create \
  --name "$APP_SERVICE_PLAN" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --sku "$SKU" \
  --is-linux

# ─── Web App ─────────────────────────────────────────────────────────────────
echo "Criando Web App: $WEB_APP_NAME..."
az webapp create \
  --name "$WEB_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_SERVICE_PLAN" \
  --runtime "$RUNTIME"

echo ""
echo "Infraestrutura criada com sucesso!"
echo "URL: https://$WEB_APP_NAME.azurewebsites.net"
