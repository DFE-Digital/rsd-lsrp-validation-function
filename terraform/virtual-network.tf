resource "azurerm_virtual_network" "default" {
  name                = "${local.environment}-rsd-lsrp-val-func"
  address_space       = [local.virtual_network_address_space]
  location            = azurerm_resource_group.rsd_lsrp_val_func.location
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name
  tags                = local.tags
}

resource "azurerm_subnet" "rsd_lsrp_val_function_app" {
  name                              = "${local.environment}rsdlsrpvalfunc"
  virtual_network_name              = azurerm_virtual_network.default.name
  resource_group_name               = azurerm_resource_group.rsd_lsrp_val_func.name
  address_prefixes                  = [local.function_app_subnet_cidr]
  private_endpoint_network_policies = "Enabled"

  delegation {
    name = "env"
    service_delegation {
      name = "Microsoft.App/environments"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }

  depends_on = [
    azurerm_virtual_network.default
  ]
}

resource "azurerm_subnet" "eat_api_storage_private_endpoint" {
  name                              = "${local.environment}eatapistorage"
  virtual_network_name              = azurerm_virtual_network.default.name
  resource_group_name               = azurerm_resource_group.rsd_lsrp_val_func.name
  address_prefixes                  = [local.eat_api_storage_private_endpoint_subnet_cidr]
  private_endpoint_network_policies = "Enabled"

  service_endpoints = [
    "Microsoft.Storage",
  ]

  depends_on = [
    azurerm_virtual_network.default
  ]
}

resource "azurerm_network_security_group" "eat_api_storage_private_endpoint" {
  name                = "${local.environment}eatapistorage"
  location            = azurerm_resource_group.rsd_lsrp_val_func.location
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name

  tags = local.tags
}

resource "azurerm_network_security_rule" "eat_api_storage_private_endpoint_allow_inbound" {
  name                         = "AllowInboundEatApiStorageFromFunctionApp"
  priority                     = 100
  direction                    = "Inbound"
  access                       = "Allow"
  protocol                     = "Tcp"
  source_port_range            = "*"
  destination_port_ranges      = ["443", "445"]
  source_address_prefixes      = azurerm_subnet.rsd_lsrp_val_function_app.address_prefixes
  destination_address_prefixes = azurerm_subnet.eat_api_storage_private_endpoint.address_prefixes
  network_security_group_name  = azurerm_network_security_group.eat_api_storage_private_endpoint.name
  resource_group_name          = azurerm_resource_group.rsd_lsrp_val_func.name
}

resource "azurerm_subnet_network_security_group_association" "storage_infra" {
  subnet_id                 = azurerm_subnet.eat_api_storage_private_endpoint.id
  network_security_group_id = azurerm_network_security_group.eat_api_storage_private_endpoint.id
}

resource "azurerm_private_dns_zone" "eat_api_storage_private_link_file" {
  name                = "${local.eat_api_storage_account_name}.file.core.windows.net"
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name
  tags                = local.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "eat_api_storage_private_link_file" {
  name                  = "${local.environment}eatapistorage"
  resource_group_name   = azurerm_resource_group.rsd_lsrp_val_func.name
  private_dns_zone_name = azurerm_private_dns_zone.eat_api_storage_private_link_file.name
  virtual_network_id    = azurerm_virtual_network.default.id
  tags                  = local.tags
}

resource "azurerm_private_endpoint" "eat_api_storage" {
  name                = "${local.environment}-eat-api-storage"
  location            = azurerm_resource_group.rsd_lsrp_val_func.location
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name
  subnet_id           = azurerm_subnet.eat_api_storage_private_endpoint.id

  private_service_connection {
    name                           = "${local.environment}eatapistorageconnection"
    private_connection_resource_id = data.azurerm_storage_account.eat_api.id
    subresource_names              = ["file"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.eat_api_storage_private_link_file.id]
  }

  tags = local.tags
}

resource "azurerm_private_dns_a_record" "storage_private_link_file" {
  name                = "@"
  zone_name           = azurerm_private_dns_zone.eat_api_storage_private_link_file.name
  resource_group_name = azurerm_resource_group.rsd_lsrp_val_func.name
  ttl                 = 300
  records             = [azurerm_private_endpoint.eat_api_storage.private_service_connection[0].private_ip_address]
  tags                = local.tags
}
