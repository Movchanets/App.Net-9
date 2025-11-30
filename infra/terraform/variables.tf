variable "location" {
  type    = string
  default = "westeurope"
}

variable "resource_group_name" {
  type    = string
  default = "rg-appnet9"
}

variable "acr_name" {
  type    = string
  default = "acrappnet9"
}

variable "env_name" {
  type    = string
  default = "cae-appnet9"
}

variable "api_name" {
  type    = string
  default = "appnet9-api"
}

variable "front_name" {
  type    = string
  default = "appnet9-front"
}

variable "storage_account_name" {
  type    = string
  default = "stappnet9web"
}

variable "storage_container_name" {
  type        = string
  description = "Blob container name to create for API assets (e.g., images)"
  default     = "images"
}

variable "enable_front_container_app" {
  type        = bool
  description = "If true, create a Front Container App (use Storage Static Website instead to minimize cost)."
  default     = false
}

variable "api_image" {
  type        = string
  description = "API container image (e.g., acrappnet9.azurecr.io/app-api:latest). Defaults to a placeholder hello-world image."
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}
