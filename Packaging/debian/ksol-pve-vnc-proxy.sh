#!/bin/sh
mkdir -p /etc/ksol-pve-vnc-proxy/
cd /etc/ksol-pve-vnc-proxy/
/usr/share/ksol-pve-vnc-proxy/KSol_PVE_VNC_Proxy "$@"
