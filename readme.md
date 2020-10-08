# VolumeSync

Synchronizes the volume across audio devices.

## Options

|Option|Type|Required|Default|Description|
|---|---|---|---|---|
|Primary|string|false|Default playback device|The identifier or full name of the device to synchronize with.|
|SecondaryDevices|[[SecondaryDevice](#secondarydevice)]|true| |Devices to synchronize to the primary.|

### SecondaryDevice

Each secondary device will be synced to the primary using `(primary + RateConstant) ^ Exponent * Multiplier + Offset`.

|Option|Type|Required|Default|Description|
|---|---|---|---|---|
|Name|string|true| |The identifier or full name of the device to synchronize.|
|Multiplier|double|false|1| |
|RateConstant|double|false|0| |
|Exponent|double|false|1| |
|Offset|double|false|0| |

### Example appsettings.json

```json5
{
    "Primary": "Speakers (Onboard)",
    "SecondaryDevices": [
        {
            "Name": "Speakers (Discrete)",
            "Multiplier": "1.5",
        },
    ],
}
```

## Run

```shell script
./VolumeSync.exe
```

## Install as a service

- Use the [Non-Sucking Service Manager](https://nssm.cc/)
    - Has the option to set environment variables for this service.
- Use [sc.exe](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/sc-create)

## Build

```shell script
dotnet build
```

## Test

```shell script
dotnet test
```
