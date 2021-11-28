#!/bin/sh
mkdir -p /etc/ksol-pve-vnc-proxy
HOSTNAME=$(cat /etc/hostname)
sed -i "s/{PVE_NODE_HOSTNAME}/$HOSTNAME/g" /etc/ksol-pve-vnc-proxy/appsettings.json
systemctl daemon-reload
systemctl enable ksol-pve-vnc-proxy.service
service ksol-pve-vnc-proxy restart

