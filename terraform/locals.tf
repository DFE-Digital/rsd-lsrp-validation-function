locals {
  environment     = var.environment
  azure_location  = var.azure_location
  tags            = var.tags
  tfvars_filename = var.tfvars_filename

  storage_account_ipv4_allow_list = var.storage_account_ipv4_allow_list

  virtual_network_address_space                = var.virtual_network_address_space
  function_app_subnet_cidr                     = var.function_app_subnet_cidr
  eat_api_storage_private_endpoint_subnet_cidr = var.eat_api_storage_private_endpoint_subnet_cidr

  app_settings                  = var.app_settings
  service_bus_connection_string = var.service_bus_connection_string

  eat_api_storage_account_name           = var.eat_api_storage_account_name
  eat_api_storage_account_resource_group = var.eat_api_storage_account_resource_group

  is_windows = can(regex("^[A-Za-z]:", abspath(path.root)))
  bash       = local.is_windows ? "C:/Program Files/Git/bin/bash.exe" : "/bin/bash"
}
