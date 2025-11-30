resource "azurerm_container_app_environment" "env" {
  name                = var.env_name
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
}
