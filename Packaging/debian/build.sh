#!/bin/bash

#build package
fpm -s dir \
    -t deb \
    -n ksol-pve-vnc-proxy \
    -v "1.$(date +"%s")" \
    --before-install ./Packaging/debian/install-scripts/pre-install.sh \
    --after-install ./Packaging/debian/install-scripts/post-install.sh \
    --before-upgrade ./Packaging/debian/install-scripts/pre-upgrade.sh \
    --after-upgrade ./Packaging/debian/install-scripts/post-upgrade.sh \
    --vendor "KSol.IT" \
    --architecture x86_64 \
    --maintainer "KSol.IT <support@ksol.it>" \
    --url "https://www.ksol.it" \
    ./bin/Release/net5.0/linux-x64/publish/appsettings.json=/etc/ksol-pve-vnc-proxy/appsettings.json \
    ./bin/Release/net5.0/linux-x64/publish/=/usr/share/ksol-pve-vnc-proxy/ \
    ./bin/Release/net5.0/linux-x64/publish/=/usr/share/ksol-pve-vnc-proxy/ \
    ./Packaging/debian/ksol-pve-vnc-proxy.sh=/usr/bin/ksol-pve-vnc-proxy \
    ./Packaging/debian/ksol-pve-vnc-proxy.service=/etc/systemd/system/ksol-pve-vnc-proxy.service
