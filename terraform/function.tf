resource "azurerm_service_plan" "function_apps_flex" {
  name                = "${local.environment}-rsd-lsrp-val-function"
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name
  location            = azurerm_resource_group.rsd_lsrp_val_func.location
  os_type             = "Linux"
  sku_name            = "FC1" // Flex Consumption Plan

  tags = local.tags
}

resource "azurerm_function_app_flex_consumption" "rsd_lsrp_val_function_app" {
  name                                           = "${local.environment}-rsd-lsrp-val"
  resource_group_name                            = azurerm_resource_group.rsd_lsrp_val_func.name
  location                                       = azurerm_resource_group.rsd_lsrp_val_func.location
  storage_container_type                         = "blobContainer"
  storage_authentication_type                    = "StorageAccountConnectionString"
  storage_access_key                             = azurerm_storage_account.function_app_backing.primary_access_key
  storage_container_endpoint                     = "${azurerm_storage_account.function_app_backing.primary_blob_endpoint}${azurerm_storage_container.function_app_backing.name}"
  runtime_name                                   = "dotnet-isolated"
  runtime_version                                = "10.0"
  service_plan_id                                = azurerm_service_plan.function_apps_flex.id
  webdeploy_publish_basic_authentication_enabled = false
  https_only                                     = true
  virtual_network_subnet_id                      = azurerm_subnet.rsd_lsrp_val_function_app.id

  app_settings = merge(local.app_settings, {
    "AZURE_CLIENT_ID" = azurerm_user_assigned_identity.rsd_lsrp_val_function_app.client_id
    },
  )

  connection_string {
    name  = "ServiceBus"
    type  = "ServiceBus"
    value = local.service_bus_connection_string
  }

  site_config {
    http2_enabled                          = true
    ip_restriction_default_action          = "Allow"
    scm_ip_restriction_default_action      = "Allow"
    scm_use_main_ip_restriction            = true
    minimum_tls_version                    = "1.3"
    application_insights_connection_string = azurerm_application_insights.rsd_lsrp_val_function_app.connection_string
    application_insights_key               = azurerm_application_insights.rsd_lsrp_val_function_app.instrumentation_key
  }

  identity {
    type = "UserAssigned"
    identity_ids = [
      azurerm_user_assigned_identity.rsd_lsrp_val_function_app.id
    ]
  }

  lifecycle {
    ignore_changes = [
      app_settings
    ]
  }

  tags = local.tags
}
