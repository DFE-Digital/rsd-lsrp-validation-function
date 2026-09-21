resource "azurerm_storage_account" "function_app_backing" {
  name                             = "${local.environment}rsdlsrpvalfunc"
  resource_group_name              = azurerm_resource_group.rsd_lsrp_val_func.name
  location                         = azurerm_resource_group.rsd_lsrp_val_func.location
  account_tier                     = "Standard"
  account_replication_type         = "LRS"
  min_tls_version                  = "TLS1_2"
  https_traffic_only_enabled       = true
  allow_nested_items_to_be_public  = false
  public_network_access_enabled    = true
  cross_tenant_replication_enabled = false
  shared_access_key_enabled        = true

  blob_properties {
    delete_retention_policy {
      days = 7
    }
    container_delete_retention_policy {
      days = 7
    }
  }

  tags = local.tags
}

resource "azurerm_storage_container" "function_app_backing" {
  name                 = "rsdlsrpvalfunc"
  storage_account_name = azurerm_storage_account.function_app_backing.name
}

resource "azurerm_storage_account_network_rules" "function_app_backing" {
  storage_account_id = azurerm_storage_account.function_app_backing.id
  default_action     = "Allow"
  bypass             = ["AzureServices"]
  ip_rules           = local.storage_account_ipv4_allow_list
}
