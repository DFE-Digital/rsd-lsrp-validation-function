data "azurerm_storage_account" "eat_api" {
  name                = local.eat_api_storage_account_name
  resource_group_name = local.eat_api_storage_account_resource_group
}
