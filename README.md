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

## EnumHelper

The `EnumHelper` class provides comprehensive utility methods for working with enumeration types in .NET. It offers reflection-based operations for parsing, validating, converting, and analyzing enum values, including support for flags enums, attributes, and display metadata.

### Usage Example

```csharp
using GraphQLEngine.Common.Utilities;
using System.ComponentModel.DataAnnotations;

// Define an enum with display attributes
public enum UserRole
{
    [Display(Name = "Guest User")]
    Guest = 0,
    
    [Display(Name = "Registered Member")]
    Member = 1,
    
    [Display(Name = "Content Contributor")]
    Contributor = 2,
    
    [Display(Name = "Administrator")]
    Admin = 3
}

// Get all enum values
var allRoles = EnumHelper.GetEnumValues<UserRole>();
// Returns: [Guest, Member, Contributor, Admin]

// Get display names for all enum values
var roleDisplayNames = EnumHelper.GetEnumDisplayDictionary<UserRole>();
// Returns: {"Guest": "Guest User", "Member": "Registered Member", "Contributor": "Content Contributor", "Admin": "Administrator"}

// Parse a string to enum value
var role = EnumHelper.Parse<UserRole>("contributor"); // Member = 2

// Get next and previous enum values
var nextRole = EnumHelper.GetNextValue(role); // Contributor
var prevRole = EnumHelper.GetPreviousValue(role); // Member

// Check if enum has a specific attribute
var hasDisplayAttr = EnumHelper.HasAttribute<UserRole, DisplayAttribute>(UserRole.Admin); // true

// Get underlying numeric value
var underlyingValue = EnumHelper.GetUnderlyingValue(UserRole.Admin); // 3
```

// ... rest of the original README content ...
