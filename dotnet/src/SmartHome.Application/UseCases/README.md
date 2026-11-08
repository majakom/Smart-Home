# Application Use Cases

Use cases coordinate workflows using Domain types and Application abstractions. Transport hosts map incoming data to use-case requests; Infrastructure provides persistence.

## Telemetry

### Record climate measurement

**Location:** `Telemetry/RecordClimateMeasurement/`

Records a climate reading received from a telemetry source.

1. Validate the source message ID, device and sensor IDs, UTC timestamp, and relative humidity range (0–100%).
2. Verify that the sensor exists, belongs to the supplied device, both are enabled, and the sensor category is Climate.
3. Create a Domain `ClimateMeasurement`, timestamp it with the current receive time, and ask `IClimateMeasurementWriter` to store it.
4. Return `Recorded` with the measurement ID, `Duplicate`, or `Rejected` with a rejection reason.

Shared telemetry checks and outcome types are in `Telemetry/Shared/Validation/` and `Telemetry/Shared/Results/`. Add future telemetry workflow descriptions under this section.
