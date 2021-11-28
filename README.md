# KSol Pve VNC Proxy

This is a Proxmox Vnc Websocket proxy to allow plain vnc connections to your virtual machines on demand.
It is api compatible to the json api of Proxmox VE.

## Installation

There are three ways to install this piece of software.

### Installation via apt repo

Step 1: Install apt-transport-https
```bash
sudo apt-get install apt-transport-https
```
Step 2: Import the gpg key
```bash
curl https://apt.ksol.it/ksol.gpg.key | sudo apt-key add
```

Step 3: Add the repo to your `sources.list.d` 
```bash
echo "deb https://apt.ksol.it/deb /" | sudo tee /etc/apt/sources.list.d/apt-ksol.list
```

Step 4: Apt update
```bash
sudo apt update
```

Step 5: Install package `ksol-pve-vnc-proxy`
```bash
sudo apt install ksol-pve-vnc-proxy
```

Step 6: Check the configuration if there are any changes nescessary.
```bash
cat /etc/ksol-pve-vnc-proxy/appsettings.json
```

### Manual .deb install

Step 1: Download the latest .deb from the releases section.
```bash
wget https://github.com/mKenfenheuer/ksol-pve-vnc-proxy/releases/download/1.x/ksol-pve-vnc-proxy_1.1638105333_amd64.deb
```

Step 2: Install the package via dpkg
```bash
sudo dpkg -i ksol-pve-vnc-proxy_1.1638105333_amd64.deb
```

Step 3: Check the configuration if there are any changes nescessary.
```bash
cat /etc/ksol-pve-vnc-proxy/appsettings.json
```

### Installation from source

Step 1: Make sure you have all nescessary dependencies for the installation available.

| Dependency | Required | Description | Version | Link |
|---|---|---|---|---|
| git | optional | Used to get the source code. Can also be downloaded as .zip but this is not covered in this guide. | 2+ | [Packages](https://git-scm.com/download/linux) | 
| .Net Core SDK | required | Framework used to compile the application | 5.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/5.0) |

Step 2: Clone the repository

```bash
git clone https://github.com/mKenfenheuer/ksol-pve-vnc-proxy.git
```

Step 3: Build the source code into a self contained application for distribution

```bash
cd ksol-pve-vnc-proxy
dotnet publish --self-contained -c release -r linux-x64 *.sln
```

Step 4: Copy the compiled application to a path of your choice and make it executable:

```bash
cp ./bin/Release/net5.0/linux-x64/publish/ /usr/share/ksol-pve-vnc-proxy/
chmod +x /usr/share/ksol-pve-vnc-proxy/KSol_PVE_VNC_Proxy
```

Step 5: Copy the [config file](./appsettings.json) to the respective config directory `/etc/ksol-pve-vnc-proxy/`

```bash
cp ./bin/Release/net5.0/linux-x64/publish/appsettings.json /etc/ksol-pve-vnc-proxy/appsettings.json
```

Step 6: Edit the paths to the certificate files in the [config file](./appsettings.json), you can also use other certificates. By default we are using the same certificate as Proxmox VE.

An easy way to point to the correct certificates, assuming the software is installed directly onto the node:

```bash
HOSTNAME=$(cat /etc/hostname)
sed -i "s/{PVE_NODE_HOSTNAME}/$HOSTNAME/g" /etc/ksol-pve-vnc-proxy/appsettings.json
```

This replaces the term `{PVE_NODE_HOSTNAME}` with the node hostname in the config file.

Step 7: Copy the [start script](./Packaging/debian/ksol-pve-vnc-proxy.sh) to `/usr/bin/` and make it executable.

```bash
cp /Packaging/debian/ksol-pve-vnc-proxy.sh /usr/bin/ksol-pve-vnc-proxy
chmod +x /usr/bin/ksol-pve-vnc-proxy
```

Step 8: Copy the [systemd service file](./Packaging/debian/ksol-pve-vnc-proxy.service) for the application to `/etc/systemd/system/`

```bash
cp ./Packaging/debian/ksol-pve-vnc-proxy.service /etc/systemd/system/
```

Step 8: Reload the systemd configuration and enable the service.

```bash
sudo systemctl daemon-reload
sudo systemctl enable ksol-pve-vnc-proxy.service
```

Step 9: Start the systemd service.

```bash
sudo service ksol-pve-vnc-proxy start
sudo service ksol-pve-vnc-proxy status
```

It should output the following:

```
● ksol-pve-vnc-proxy.service - KSol PVE VNC Proxy
     Loaded: loaded (/etc/systemd/system/ksol-pve-vnc-proxy.service; enabled; vendor preset: enabled)
     Active: active (running) since Sun 2021-11-28 15:54:02 CET; 3s ago
   Main PID: 3371199 (ksol-pve-vnc-pr)
      Tasks: 32 (limit: 86670)
     Memory: 34.7M
        CPU: 396ms
     CGroup: /system.slice/ksol-pve-vnc-proxy.service
             ├─3371199 /bin/sh /usr/bin/ksol-pve-vnc-proxy
             └─3371201 /usr/share/ksol-pve-vnc-proxy/KSol_PVE_VNC_Proxy

Nov 28 15:54:02 SSTG4400 systemd[1]: Started KSol PVE VNC Proxy.
Nov 28 15:54:03 SSTG4400 ksol-pve-vnc-proxy[3371201]: info: Microsoft.Hosting.Lifetime[0]
Nov 28 15:54:03 SSTG4400 ksol-pve-vnc-proxy[3371201]:       Now listening on: https://0.0.0.0:5001
Nov 28 15:54:03 SSTG4400 ksol-pve-vnc-proxy[3371201]: info: Microsoft.Hosting.Lifetime[0]
Nov 28 15:54:03 SSTG4400 ksol-pve-vnc-proxy[3371201]:       Application started. Press Ctrl+C to shut down.
Nov 28 15:54:03 SSTG4400 ksol-pve-vnc-proxy[3371201]: info: Microsoft.Hosting.Lifetime[0]
Nov 28 15:54:03 SSTG4400 ksol-pve-vnc-proxy[3371201]:       Hosting environment: Production
Nov 28 15:54:03 SSTG4400 ksol-pve-vnc-proxy[3371201]: info: Microsoft.Hosting.Lifetime[0]
Nov 28 15:54:03 SSTG4400 ksol-pve-vnc-proxy[3371201]:       Content root path: /etc/ksol-pve-vnc-proxy
```






## How does it work?

The proxy server api listens by default on `https://0.0.0.0:5001`. This means if your ip address of your proxmox host is `192.168.1.20` you would want to access the proxy server at `https://192.168.1.20:5001/`.

There is the following api call available:

`` 
GET /api2/json/nodes/{node}/{type}/{id}/plainvncproxy
``

#### Parameters: 

| Parameter | Description |
|---|---|
| `{node}` | The node name on which the resource is running |
| `{type}` | The resource type, likely `qemu` or  `lxc` |
| `{id}` | The resource id of the virtual machine |

#### Authentication:

Authentication information to the service is pass-through, meaning there is no authentication and authorization checks performed by this proxy. This is done by the Proxmox VE host api. Therefore you will need to send the same authentication information, in the same form to this proxy as if you would do to the Proxmox api.

| HTTP-Header | Description |
|---|---|
| `Cookie: PVEAuthCookie=XXXX` | The PVE auth cookie obtained by logging in to the api. |
| `CSRFPreventionToken` | The CSRFPreventionToken obtained by logging in to the api. |´

#### Response:

The api will respond to your query with the following json response, which is the same to the Proxmox VE Api:

```json
{
    "statusCode": 200,      // Status code, 200 if succeded
    "errorMessage": {},     // Error message, null if succeded
    "responsePayload": {    // VNC Connection details, null if an error ocurred
        "upid": "",         // Proxmox Console Task ID 
        "user": "",         // User and realm for which the vnc console is started. 
        "port": ,           // VNC Port on which the proxy is listening 
        "ticket": "",       // VNC Ticket, use as password to login
    },
    "isSuccess": true       // Success indicator, false if there was an error
}
```

You can then use the information to make a vnc connection to the Proxmox VE host.
Assuming you received the same information as above (port `5900`) and the ip address of your proxmox host is `192.168.1.20` you would want to access the vnc server at `vnc://192.168.1.20:5900/`. Then on connection, use the vnc ticket as password.
