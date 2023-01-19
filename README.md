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

## JsonHelper

The `JsonHelper` class provides a comprehensive set of utilities for JSON serialization and deserialization in .NET applications. It offers flexible methods for converting objects to JSON strings, parsing JSON into strongly-typed objects, working with dictionaries, and performing various JSON operations like merging, validation, and path-based value extraction.

### Usage Example

```csharp
using GraphQLEngine.Common.Utilities;
using System.Text.Json;

// Define a sample data model
public class User
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public List<string>? Roles { get; set; }
}

// Create and serialize an object
var user = new User
{
    Name = "Alice Johnson",
    Age = 32,
    Roles = new List<string> { "admin", "user" }
};

// Serialize with default options (camelCase, no indentation)
var json = JsonHelper.Serialize(user);
// Output: {"name":"Alice Johnson","age":32,"roles":["admin","user"]}

// Serialize with pretty printing
var prettyJson = JsonHelper.Serialize(user, pretty: true);
// Output: {
//   "name": "Alice Johnson",
//   "age": 32,
//   "roles": [
//     "admin",
//     "user"
//   ]
// }

// Deserialize back to object
var deserializedUser = JsonHelper.Deserialize<User>(json);
Console.WriteLine(deserializedUser?.Name); // Alice Johnson

// Convert object to dictionary
var userDict = JsonHelper.ToDict(user);
Console.WriteLine(userDict?["name"]); // Alice Johnson

// Create dictionary from JSON
var jsonDict = JsonHelper.DeserializeToDictionary("{\"status\":\"active\",\"count\":5}");
Console.WriteLine(jsonDict?["status"]); // active

// Merge multiple objects
var merged = JsonHelper.Merge(
    new { id = 1, name = "First" },
    new { status = "active", priority = 10 }
);
// Output: {"id":1,"name":"First","status":"active","priority":10}

// Check if JSON is valid
var isValid = JsonHelper.IsValidJson(json); // true

// Get value by path
var nameValue = JsonHelper.GetValueByPath(json, "name"); // "Alice Johnson"

// Remove null values from JSON
var jsonWithNulls = "{\"name\":\"Bob\",\"age\":null,\"active\":true}";
var cleanedJson = JsonHelper.RemoveNulls(jsonWithNulls);
// Output: {"name":"Bob","active":true}

// Use custom serialization options
var customOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};
var customJson = JsonHelper.Serialize(user, customOptions);
```

// ... rest of the original README content ...
