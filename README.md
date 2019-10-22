# s3i
Install Windows Installer MSI from S3

## Build Status

  master:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/master?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/master)
  develop:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/develop?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/develop)

# Functionality

## Use case
Having a fleet of Windows computers, we need to automate software installation and configuration, depending on individual or group settings.

s3i allows to install/upgrade/downgrade software packaged for Microsoft Installer, downloading configuration files and installers from remote storage (http servers, like github releases, or AWS S3 buckets)

## Setup

```
           command line to run on host       S3/http
/------\                                  /------------\
| Host | -- s3i http://../config.ini -->  | config.ini |   host (group) configuration file
\------/                                  \------------/
                                            | |
               S3/http                      | |  links to products to be installed
              /-------\                     | |
              | *.msi |-\  <----------------/ |
              \-------/ |  <------------------/
                 \------/
```
s3i reads configuration files, specified in command line, downloads and caches product installers and properties, and performs required installations, upgrades, or downgrades by invoking Windows msiexec.exe with proper arguments.

### Configuration file format

Configuration file contains one or several product specifications:
- Product name, for example, `SomethingUseless`
- Product installer URL: `https://useless.bucket.s3.amazonaws.com/useless.product/develop/1.2.3.4-beta2+test/installer.msi`
- Set of properties (key/value pairs) to be passed to unattended MSI installation

Here is an example of `useless.ini` file:
```
[$products$]
SomethingUseless = https://useless.bucket.s3.amazonaws.com/useless.product/develop/1.2.3.4-beta2+test/installer.msi

[SomethingUseless]
ImportantProperty = just an example
```
