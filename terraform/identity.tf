resource "azurerm_user_assigned_identity" "rsd_lsrp_val_function_app" {
  location            = azurerm_resource_group.rsd_lsrp_val_func.location
  name                = "${local.environment}-rsd-lsrp-val-func"
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name
  tags                = local.tags
}
