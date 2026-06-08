locals {
  ingress_cloud_init = <<-EOT
    #cloud-config
    package_update: true
    packages:
      - iptables-persistent
    runcmd:
      - echo "net.ipv4.ip_forward=1" >> /etc/sysctl.conf
      - sysctl -p
      - iptables -t nat -A POSTROUTING -o eth0 -j MASQUERADE
      - netfilter-persistent save
  EOT
}

# ingress + web: nginx ingress controller + Angular SPA
resource "azurerm_linux_virtual_machine" "ingress" {
  name                = "ingress-vm"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  size                = "Standard_B2als_v2"
  admin_username      = var.admin_username

  network_interface_ids = [azurerm_network_interface.ingress.id]

  admin_ssh_key {
    username   = var.admin_username
    public_key = file(var.ssh_public_key_path)
  }

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "StandardSSD_LRS"
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "ubuntu-24_04-lts"
    sku       = "server"
    version   = "latest"
  }

  custom_data = base64encode(local.ingress_cloud_init)
}

# control-plane: k8s 系統元件
resource "azurerm_linux_virtual_machine" "control_plane" {
  name                = "control-plane-vm"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  size                = "Standard_B2als_v2"
  admin_username      = var.admin_username

  network_interface_ids = [azurerm_network_interface.control_plane.id]

  admin_ssh_key {
    username   = var.admin_username
    public_key = file(var.ssh_public_key_path)
  }

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "StandardSSD_LRS"
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "ubuntu-24_04-lts"
    sku       = "server"
    version   = "latest"
  }
}

# worker-2: Kafka + PostgreSQL (8GB RAM)
resource "azurerm_linux_virtual_machine" "worker2" {
  name                = "worker2-vm"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  size                = "Standard_B2as_v2"
  admin_username      = var.admin_username

  network_interface_ids = [azurerm_network_interface.worker2.id]

  admin_ssh_key {
    username   = var.admin_username
    public_key = file(var.ssh_public_key_path)
  }

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "StandardSSD_LRS"
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "ubuntu-24_04-lts"
    sku       = "server"
    version   = "latest"
  }
}
