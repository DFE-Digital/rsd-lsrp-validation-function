variable "azure_client_id" {
  description = "Service Principal Client ID"
  type        = string
}

variable "azure_client_secret" {
  description = "Service Principal Client Secret"
  type        = string
  sensitive   = true
}

variable "azure_tenant_id" {
  description = "Service Principal Tenant ID"
  type        = string
}

variable "azure_subscription_id" {
  description = "Service Principal Subscription ID"
  type        = string
}

variable "azure_location" {
  description = "Azure location in which to launch resources."
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "tfvars_filename" {
  description = "tfvars filename. This file is uploaded and stored encrypted within Key Vault, to ensure that the latest tfvars are stored in a shared place."
  type        = string
}

variable "storage_account_ipv4_allow_list" {
  description = "A list of public IPv4 address to grant access to the Storage Account"
  type        = list(string)
  default     = []
}

variable "virtual_network_address_space" {
  description = "Virtual Network address space CIDR"
  type        = string
  default     = "172.16.0.0/12"
}

variable "function_app_subnet_cidr" {
  description = "Specify a subnet prefix to use for the function app subnet"
  type        = string
  default     = "172.16.0.0/24"
}

variable "eat_api_storage_private_endpoint_subnet_cidr" {
  description = "Specify a subnet prefix to use for the eat api storage private endpoint subnet"
  type        = string
  default     = "172.16.1.0/24"
}

variable "app_settings" {
  description = "Function app settings"
  type        = map(string)
  default     = {}
}

variable "service_bus_connection_string" {
  description = "Function service bus connection string"
  type        = string
  default     = ""
}

variable "eat_api_storage_account_name" {
  description = "EAT API storage account name"
  type        = string
  default     = ""
}

variable "eat_api_storage_account_resource_group" {
  description = "EAT API storage account resource group"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to be applied to all resources"
  type        = map(string)
  default     = {}
}
