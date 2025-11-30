resource "azurerm_storage_account" "static" {
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"
}

resource "azurerm_storage_account_static_website" "site" {
  storage_account_id = azurerm_storage_account.static.id
  index_document     = "index.html"
  error_404_document = "index.html"
}

# Blob container for API assets (e.g., images)
resource "azurerm_storage_container" "api_assets" {
  name                  = var.storage_container_name
  storage_account_id    = azurerm_storage_account.static.id
  container_access_type = "private"
}
