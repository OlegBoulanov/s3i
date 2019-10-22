# s3i - batch Windows MSI installer 

## Build Status

  master:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/master?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/master)
  develop:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/develop?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/develop)

# Functionality

## Use case
Having a fleet of Windows computers, we need to automate software installation and configuration, depending on individual or group settings. Software amy be produced by a separate CI/CD system and uploaded to version specific folders automatically. Upgarding a group of hosts to newer version would require changing a single line in group configuration file. s3i, ran manually or automatically, would compare already installed version with requested one and take proper actions.

s3i allows to install/upgrade/downgrade software packaged for Microsoft Installer, downloading configuration files and installers from remote storage (http servers, like github releases, or AWS S3 buckets)

## Setup

```
           command line to run on host       S3/http
/------\                                  /------------\
| Host | -- s3i http://../config.ini -->  | config.ini |   host (group) configuration file
\------/                                  \------------/
                                            | |
               S3/http                      | |  links to products to be downloaded and installed
              /-------\                     | |
              | *.msi |-\  <----------------/ |
              \-------/ |  <------------------/
                 \------/  <<<<-------------------<<< CI/CD system may upload these (with version tags in URL)
```
s3i reads configuration files, specified in command line, downloads and caches product installers and properties, and performs required installations, upgrades, or downgrades by invoking Windows msiexec.exe with proper arguments.

### Configuration file format

Configuration file contains one or several product specifications:
- Product name, for example, `SomethingUseless`
- Product installer URL: `https://useless.bucket.s3.amazonaws.com/useless.product/develop/1.2.3.4-beta2+test/installer.msi`
- Set of properties (key/value pairs) to be passed to unattended MSI installation

Here is an example of `useless.ini` file:
```
; List of product name = installer URL
[$products$]
SomethingUseless = https://useless.bucket.s3.amazonaws.com/useless.product/develop/1.2.3.4-beta2+test/installer.msi

; Sections specify optional product properties
[SomethingUseless]
ImportantProperty = just an example
```
