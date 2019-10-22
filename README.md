# s3i - Batch Windows MSI installer 

## Build Status

  master:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/master?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/master)
  develop:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/develop?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/develop)

## Installation requirements

- Windows 7/10
- [.NET Core Runtime 3.0.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)

Latest version of `s3i.msi` can be found on the project's [Releases tab](https://github.com/OlegBoulanov/s3i/releases)

# Functionality

## Use case
Having a fleet of Windows computers, we need to automate software installation and configuration, depending on individual or group settings. Software amy be produced by a separate CI/CD system and uploaded to version specific folders and/or AWS S3 buckets automatically. Upgarding a group of hosts to newer version would require changing a single line in group configuration file. s3i, ran manually or automatically, would compare already installed version with requested one and take proper actions.

s3i allows to install/upgrade/downgrade software packaged for Microsoft Installer, downloading configuration files and installers from remote storage (http servers, like github releases, or AWS S3 buckets)

## Setup

```
           command line to run on host       S3/http
/------\                                  /------------\
| Host | -- s3i http://../config.ini -->  | config.ini |   host (group) configuration file
\------/                                  \------------/
   ^                                         | |
   |            S3/http                      | |  links to products to be downloaded and installed
   |           /-------\                     | |
   \------<<<  | *.msi |-\  <----------------/ |
               \-------/ |  <------------------/
                  \------/  <<<<-------------------<<< CI/CD system may upload these (with version tags in URL)
```
s3i reads configuration files, specified in command line, downloads and caches product installers and properties, and performs required installations, upgrades, or downgrades by invoking Windows msiexec.exe with proper arguments.

### Configuration file format

Configuration file contains one or several product specifications:
- Product name, for example, `SomethingUseless`
- Product installer URL: `https://deployment-bucket.s3.amazonaws.com/useless.product/develop/1.2.3.4-beta2+test/installer.msi`
- Set of properties (key/value pairs) to be passed to unattended MSI installation

Here is an example of `useless.ini` file:
```
; List of product name = installer URL
[$products$]
SomethingUseless = https://deployment-bucket.s3.amazonaws.com/useless.product/develop/1.2.3.4-beta2+test/installer.msi

; Sections specify optional product properties
[SomethingUseless]
ImportantProperty = just an example
```
## Use

Printing help info:
```
C:\Users\current-user>s3i
s3i: msi package batch installer v1.0.243
 Usage:
  s3i [<option> ...] <products> ...
 Options:
  -h, --help                        Print this help info [False]
  -p, --profile <profile-name>      AWS user profile name [default]
  -e, --envvar <var-name>           Environment variable name (default command line) [s3i_args]
  -s, --stage <path>                Path to staging folder [C:\Users\olegb\AppData\Local\Temp\s3i]
  -m, --msiexec <path>              MsiExec command [msiexec.exe]
  -a, --msiargs <args>              MsiExec extra args [/passive]
  -t, --timeout <timespan>          Installation timeout [00:03:00]
  -d, --dryrun                      Dry run [False]
  -v, --verbose                     Print full log info [False]
```

Installing products in configuration file in AWS S3:
```
C:\Users\current-user>s3i https://install.company.com.s3.amazonaws.com/Test/Group/products.ini --verbose
```
