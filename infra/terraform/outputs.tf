output "ingress_public_ip" {
  value = azurerm_public_ip.ingress.ip_address
}

output "ingress_fqdn" {
  value = azurerm_public_ip.ingress.fqdn
}

output "ingress_private_ip" {
  value = azurerm_network_interface.ingress.private_ip_address
}

output "control_plane_private_ip" {
  value = azurerm_network_interface.control_plane.private_ip_address
}

output "worker2_private_ip" {
  value = azurerm_network_interface.worker2.private_ip_address
}

output "ssh_ingress" {
  value = "ssh ${var.admin_username}@${azurerm_public_ip.ingress.ip_address}"
}

output "ssh_control_plane" {
  value = "ssh -J ${var.admin_username}@${azurerm_public_ip.ingress.ip_address} ${var.admin_username}@${azurerm_network_interface.control_plane.private_ip_address}"
}

output "ssh_worker2" {
  value = "ssh -J ${var.admin_username}@${azurerm_public_ip.ingress.ip_address} ${var.admin_username}@${azurerm_network_interface.worker2.private_ip_address}"
}
