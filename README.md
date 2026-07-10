// ... rest of the original README content ...

## DateTimeExtensions

The `DateTimeExtensions` class provides a set of utility methods for working with dates and times. It includes methods for converting between different date and time formats, calculating time intervals, and checking if a date or time is within a certain range.

### Usage Example

```csharp
using GraphQLEngine.Common.Utilities;

// Get the current date and time
var now = DateTime.Now;

// Convert to Unix timestamp
var unixTimestamp = now.ToUnixTimestamp(); // 1643723400

// Parse an ISO 8601 date string
var parsedDate = DateTime.ParseISO8601("2022-02-01T12:00:00Z"); // 2022-02-01 12:00:00

// Check if the current date is today
var isToday = now.IsToday(); // true

// Check if the current date is within the past 7 days
var isWithinPastDays = now.IsWithinPastDays(7); // true
```

// ... rest of the original README content ...
