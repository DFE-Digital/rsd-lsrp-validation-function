resource "azurerm_resource_group" "rsd_lsrp_val_func" {
  name     = "${local.environment}-rsd-lsrp-val-func"
  location = local.azure_location
  tags     = local.tags
}
