# Application Use Cases

Use cases coordinate workflows using Domain types and Application abstractions. Transport hosts map incoming data to use-case requests; Infrastructure provides persistence.

## Devices

### Get devices

**Location:** `Devices/GetDevices/`

Returns all registered devices.

1. Ask `IDeviceReader` to retrieve all devices.
2. Return the devices.

### Get device by ID

**Location:** `Devices/GetDeviceById/`

Retrieves a device by its ID.

1. Ask `IDeviceReader` to retrieve the device with the supplied ID.
2. Return the device, or `null` if it does not exist.

## Sensors

### Get sensors

**Location:** `Sensors/GetSensors/`

Returns all registered sensors.

1. Ask `ISensorReader` to retrieve all sensors.
2. Return the sensors.

### Get sensor by ID

**Location:** `Sensors/GetSensorById/`

Retrieves a sensor by its ID.

1. Ask `ISensorReader` to retrieve the sensor with the supplied ID.
2. Return the sensor, or `null` if it does not exist.

## Telemetry

### Record climate measurement

**Location:** `Telemetry/RecordClimateMeasurement/`

Records a climate reading received from a telemetry source.

1. Validate the source message ID, device and sensor IDs, UTC timestamp, and relative humidity range (0–100%).
2. Verify that the sensor exists, belongs to the supplied device, both are enabled, and the sensor category is Climate.
3. Create a Domain `ClimateMeasurement`, timestamp it with the current receive time, and ask `IClimateMeasurementWriter` to store it.
4. Mark the device as seen with the same receive timestamp, including when the measurement is a duplicate.
5. Return `Recorded` with the measurement ID, `Duplicate`, or `Rejected` with a rejection reason.

### Get climate measurements for a sensor

**Location:** `Telemetry/GetSensorClimateMeasurements/`

Retrieves climate measurements for one sensor within an inclusive UTC time range.

1. Validate the UTC time range and the requested limit (default 100, maximum 500).
2. Verify that the sensor ID is valid, the sensor exists, and it measures climate data.
3. Ask `IClimateMeasurementReader` to retrieve measurements in the range, ordered by measurement time and then ID, and limited to the requested count.
4. Return the measurements or a query failure reason.

Shared telemetry checks and outcome types are in `Telemetry/Shared/Validation/` and `Telemetry/Shared/Results/`. Add future telemetry workflow descriptions under this section.
