resource "azurerm_log_analytics_workspace" "rsd_lsrp_val_function_app" {
  name                = "${local.environment}rsdlsrpvalfunction"
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name
  location            = azurerm_resource_group.rsd_lsrp_val_func.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}

resource "azurerm_monitor_diagnostic_setting" "rsd_lsrp_val_function_app" {
  name                       = "${local.environment}-rsd-lsrp-val-diagnostics"
  target_resource_id         = azurerm_function_app_flex_consumption.rsd_lsrp_val_function_app.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.rsd_lsrp_val_function_app.id

  enabled_log {
    category = "FunctionAppLogs"
  }
}

resource "azurerm_application_insights" "rsd_lsrp_val_function_app" {
  name                = "${local.environment}-rsd-lsrp-val-insights"
  location            = azurerm_resource_group.rsd_lsrp_val_func.location
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.rsd_lsrp_val_function_app.id
  retention_in_days   = 30
  tags                = local.tags
}
