resource "azurerm_virtual_network" "main" {
  name                = "knowledgehub-vnet"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
}

resource "azurerm_subnet" "ingress" {
  name                 = "ingress-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.1.0/24"]
}

# control-plane、worker-1、worker-2 共用此 subnet
resource "azurerm_subnet" "worker" {
  name                 = "worker-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.2.0/24"]
}

# Public IP for ingress only
resource "azurerm_public_ip" "ingress" {
  name                = "ingress-pip"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  allocation_method   = "Static"
  sku                 = "Standard"
  domain_name_label   = var.dns_label
}

# NSG: ingress - allow SSH + HTTP/HTTPS from internet
resource "azurerm_network_security_group" "ingress" {
  name                = "ingress-nsg"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  security_rule {
    name                       = "allow-ssh"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "22"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "allow-http-https"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_ranges    = ["80", "443"]
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
}

# NSG: private VMs - only VNet inbound, no direct internet
resource "azurerm_network_security_group" "worker" {
  name                = "worker-nsg"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  security_rule {
    name                       = "allow-vnet-inbound"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "deny-internet-inbound"
    priority                   = 200
    direction                  = "Inbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "Internet"
    destination_address_prefix = "*"
  }
}

# Route table: worker subnet 的 internet 流量走 ingress VM
resource "azurerm_route_table" "worker" {
  name                = "worker-rt"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  route {
    name                   = "internet-via-ingress"
    address_prefix         = "0.0.0.0/0"
    next_hop_type          = "VirtualAppliance"
    next_hop_in_ip_address = azurerm_network_interface.ingress.private_ip_address
  }
}

resource "azurerm_subnet_route_table_association" "worker" {
  subnet_id      = azurerm_subnet.worker.id
  route_table_id = azurerm_route_table.worker.id
}

# NIC: ingress - ip_forwarding_enabled 讓 Azure NIC 不丟轉發封包
resource "azurerm_network_interface" "ingress" {
  name                  = "ingress-nic"
  location              = azurerm_resource_group.main.location
  resource_group_name   = azurerm_resource_group.main.name
  ip_forwarding_enabled = true

  ip_configuration {
    name                          = "ingress-ipconfig"
    subnet_id                     = azurerm_subnet.ingress.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.ingress.id
  }
}

# NIC: control-plane
resource "azurerm_network_interface" "control_plane" {
  name                = "control-plane-nic"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  ip_configuration {
    name                          = "control-plane-ipconfig"
    subnet_id                     = azurerm_subnet.worker.id
    private_ip_address_allocation = "Dynamic"
  }
}

# NIC: worker-2
resource "azurerm_network_interface" "worker2" {
  name                = "worker2-nic"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  ip_configuration {
    name                          = "worker2-ipconfig"
    subnet_id                     = azurerm_subnet.worker.id
    private_ip_address_allocation = "Dynamic"
  }
}

resource "azurerm_network_interface_security_group_association" "ingress" {
  network_interface_id      = azurerm_network_interface.ingress.id
  network_security_group_id = azurerm_network_security_group.ingress.id
}

resource "azurerm_network_interface_security_group_association" "control_plane" {
  network_interface_id      = azurerm_network_interface.control_plane.id
  network_security_group_id = azurerm_network_security_group.worker.id
}

resource "azurerm_network_interface_security_group_association" "worker2" {
  network_interface_id      = azurerm_network_interface.worker2.id
  network_security_group_id = azurerm_network_security_group.worker.id
}
