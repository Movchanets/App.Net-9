output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "api_fqdn" {
  value = azurerm_container_app.api.latest_revision_fqdn
}

output "front_fqdn" {
  value       = length(azurerm_container_app.front) > 0 ? azurerm_container_app.front[0].latest_revision_fqdn : null
  description = "Front Container App FQDN, null when front app is disabled."
}

output "static_website_primary_endpoint" {
  value = azurerm_storage_account.static.primary_web_endpoint
}

# Secrets to use manually in GitHub Actions
output "storage_account_name" {
  value       = azurerm_storage_account.static.name
  description = "Storage account name to set as AZURE_STORAGE_ACCOUNT_NAME."
}

output "storage_container_name" {
  value       = azurerm_storage_container.api_assets.name
  description = "Blob container name to set as AZURE_STORAGE_CONTAINER_NAME."
}

output "storage_connection_string" {
  value       = format("DefaultEndpointsProtocol=https;AccountName=%s;AccountKey=%s;EndpointSuffix=core.windows.net", azurerm_storage_account.static.name, azurerm_storage_account.static.primary_access_key)
  sensitive   = true
  description = "Connection string to set as AZURE_STORAGE_CONNECTION_STRING."
}

# Additional outputs to map directly to GitHub Secrets
output "resource_group_name" {
  value       = azurerm_resource_group.rg.name
  description = "Set as AZURE_RESOURCE_GROUP."
}

output "acr_name" {
  value       = azurerm_container_registry.acr.name
  description = "Set as AZURE_ACR_NAME (registry name only)."
}

output "containerapp_api_name" {
  value       = azurerm_container_app.api.name
  description = "Set as AZURE_CONTAINERAPP_API_NAME."
}

output "containerapps_environment_name" {
  value       = azurerm_container_app_environment.env.name
  description = "Set as AZURE_CONTAINERAPPS_ENVIRONMENT_NAME."
}

output "location" {
  value       = azurerm_resource_group.rg.location
  description = "Set as AZURE_LOCATION (optional)."
}

output "vite_api_url" {
  value       = "https://${azurerm_container_app.api.latest_revision_fqdn}"
  description = "Use as VITE_API_URL for the frontend."
}
