# CollectionExtensions

`CollectionExtensions` provides a set of static extension methods that simplify common operations on collections such as checking for emptiness, retrieving elements, modifying collections, batching, ordering, and performing aggregate computations. The methods are designed to work with `IEnumerable<T>`, `ICollection<T>`, or more specific collection types where appropriate, and they follow typical LINQ‑style semantics while avoiding unnecessary allocations where possible.

## API

### IsNullOrEmpty<T>
- **Purpose**: Determines whether the specified enumerable is `null` or contains no elements.
- **Parameters**: `source` – The `IEnumerable<T>` to test.
- **Return value**: `true` if `source` is `null` or empty; otherwise `false`.
- **Exceptions**: None (handles a `null` source gracefully).

### HasItems<T>
- **Purpose**: Determines whether the specified enumerable contains at least one element.
- **Parameters**: `source` – The `IEnumerable<T>` to test.
- **Return value**: `true` if `source` is not `null` and contains elements; otherwise `false`.
- **Exceptions**: None.

### FirstOrNull<T>
- **Purpose**: Returns the first element of the enumerable, or `null` if the enumerable is `null` or empty.
- **Parameters**: `source` – The `IEnumerable<T>` to query.
- **Return value**: The first element of type `T?` (or `default(T)` for value types) if present; otherwise `null`.
- **Exceptions**: None.

### AddIfNotNull<T>
- **Purpose**: Adds an item to the collection only if the item is not `null`.
- **Parameters**: 
  - `collection` – The `ICollection<T>` to modify.
  - `item` – The item of type `T?` to add.
- **Return value**: `void`.
- **Exceptions**: 
  - `ArgumentNullException` if `collection` is `null`.

### AddRange<T>
- **Purpose**: Appends all elements of the specified sequence to the collection.
- **Parameters**: 
  - `collection` – The `ICollection<T>` to modify.
  - `items` – The `IEnumerable<T>` whose elements should be added.
- **Return value**: `void`.
- **Exceptions**: 
  - `ArgumentNullException` if `collection` or `items` is `null`.

### RemoveRange<T>
- **Purpose**: Removes all elements that match those in the specified sequence from the collection.
- **Parameters**: 
  - `collection` – The `ICollection<T>` to modify.
  - `items` – The `IEnumerable<T>` containing elements to remove.
- **Return value**: `void`.
- **Exceptions**: 
  - `ArgumentNullException` if `collection` or `items` is `null`.

### Batch<T>
- **Purpose**: Partitions the source sequence into lists of a specified size.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to batch.
  - `size` – The maximum size of each batch (must be greater than zero).
- **Return value**: An `IEnumerable<List<T>>` yielding successive batches.
- **Exceptions**: 
  - `ArgumentNullException` if `source` is `null`.
  - `ArgumentOutOfRangeException` if `size` is less than or equal to zero.

### DistinctBy<T, TKey>
- **Purpose**: Returns distinct elements from the source according to a key selector.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to process.
  - `keySelector` – A function that extracts the key of type `TKey` from each element.
- **Return value**: An `IEnumerable<T>` containing distinct elements based on the key.
- **Exceptions**: 
  - `ArgumentNullException` if `source` or `keySelector` is `null`.

### IndexOf<T>
- **Purpose**: Returns the zero‑based index of the first occurrence of a specified value in a list.
- **Parameters**: 
  - `source` – The `IList<T>` to search.
  - `value` – The object of type `T` to locate.
- **Return value**: The index of `value` if found; otherwise `-1`.
- **Exceptions**: 
  - `ArgumentNullException` if `source` is `null`.

### ForEach<T>
- **Purpose**: Performs the specified action on each element of the enumerable.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to iterate.
  - `action` – An `Action<T>` to invoke for each element.
- **Return value**: `void`.
- **Exceptions**: 
  - `ArgumentNullException` if `source` or `action` is `null`.

### ForEachWithIndex<T>
- **Purpose**: Performs the specified action on each element of the enumerable, providing the element’s index.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to iterate.
  - `action` – An `Action<T, int>` where the second parameter is the zero‑based index.
- **Return value**: `void`.
- **Exceptions**: 
  - `ArgumentNullException` if `source` or `action` is `null`.

### All<T>
- **Purpose**: Determines whether all elements of the enumerable satisfy a condition.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to evaluate.
  - `predicate` – A function to test each element for a condition.
- **Return value**: `true` if every element satisfies the predicate or if the sequence is empty; otherwise `false`.
- **Exceptions**: 
  - `ArgumentNullException` if `source` or `predicate` is `null`.

### Combine<T>
- **Purpose**: Concatenates two sequences, yielding all elements from the first followed by all elements from the second.
- **Parameters**: 
  - `first` – The first `IEnumerable<T>`.
  - `second` – The second `IEnumerable<T>`.
- **Return value**: An `IEnumerable<T>` that enumerates `first` and then `second`.
- **Exceptions**: 
  - `ArgumentNullException` if either `first` or `second` is `null`.

### Random<T>
- **Purpose**: Returns a randomly selected element from the enumerable.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to sample.
- **Return value**: An element of type `T?` chosen uniformly at random; returns `null` if the source is `null` or empty.
- **Exceptions**: 
  - `ArgumentNullException` if `source` is `null`.

### Shuffle<T>
- **Purpose**: Returns the elements of the source sequence in a random order.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to shuffle.
- **Return value**: An `IEnumerable<T>` containing the same elements in a shuffled order.
- **Exceptions**: 
  - `ArgumentNullException` if `source` is `null`.

### CountBy<T, TKey>
- **Purpose**: Groups elements by a key and returns a dictionary mapping each key to the number of elements with that key.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to count.
  - `keySelector` – A function that extracts the key of type `TKey` from each element.
- **Return value**: A `Dictionary<TKey, int>` where each key maps to the count of elements producing that key.
- **Exceptions**: 
  - `ArgumentNullException` if `source` or `keySelector` is `null`.

### ToDictionary<T, TKey, TValue>
- **Purpose**: Creates a dictionary from the source according to key and value selector functions.
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to convert.
  - `keySelector` – A function that extracts the key of type `TKey`.
  - `valueSelector` – A function that extracts the value of type `TValue`.
- **Return value**: A `Dictionary<TKey, TValue>` containing the mapped key‑value pairs.
- **Exceptions**: 
  - `ArgumentNullException` if `source`, `keySelector`, or `valueSelector` is `null`.
  - `ArgumentException` if the key selector produces duplicate keys.

### OrderByMany<T>
- **Purpose**: Sorts the elements of a sequence using a series of key selectors (each subsequent key is used to break ties).
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to order.
  - `selectors` – One or more `Func<T, IComparable>` key selectors, applied in the order provided.
- **Return value**: An `IEnumerable<T>` whose elements are sorted according to the provided keys.
- **Exceptions**: 
  - `ArgumentNullException` if `source` is `null` or any selector is `null`.

### Flatten<T>
- **Purpose**: Projects each element of a sequence of sequences to a single sequence.
- **Parameters**: 
  - `source` – The `IEnumerable<IEnumerable<T>>` to flatten.
- **Return value**: An `IEnumerable<T>` that contains all elements from the nested sequences.
- **Exceptions**: 
  - `ArgumentNullException` if `source` is `null`.

### Median<T>
- **Purpose**: Returns the median value of the sequence (the middle element when sorted, or the average of the two middle elements for even counts).
- **Parameters**: 
  - `source` – The `IEnumerable<T>` to evaluate. `T` must implement `IComparable<T>`.
- **Return value**: The median value as `T?`; returns `null` if the source is `null` or empty.
- **Exceptions**: 
  - `ArgumentNullException` if `source` is `null`.
  - `InvalidOperationException` if `T` does not implement `IComparable<T>`.

## Usage

```csharp
using System.Collections.Generic;
using System.Linq;

// Example 1: Safe retrieval and batching
List<int> numbers = new() { 5, 3, 9, 1, 7 };
var batches = numbers.Batch(2); // yields [5,3], [9,1], [7]
foreach (var batch in batches)
{
    Console.WriteLine(string.Join(", ", batch));
}

// Example 2: Conditional addition and distinct selection
ICollection<string> tags = new List<string> { "alpha", "beta" };
tags.AddIfNotNull(null);          // does nothing
tags.AddIfNotNull("gamma");       // adds "gamma"
var distinct = tags.DistinctBy(t => t.Length);
// distinct contains "alpha" (5), "beta" (4), "gamma" (5) – note duplicate length removed
```

## Notes

- All extension methods treat a `null` source argument as an invalid input and throw `ArgumentNullException`, except for the predicates that explicitly handle `null` (e.g., `IsNullOrEmpty`, `HasItems`, `FirstOrNull`, `Random`, `Median`).
- Methods that modify the collection (`AddIfNotNull`, `AddRange`, `RemoveRange`) are not thread‑safe; concurrent modifications require external synchronization.
- Lazy‑evaluated methods (`Batch`, `DistinctBy`, `Shuffle`, `Combine`, `OrderByMany`, `Flatten`) return enumerables that capture the source at enumeration time; changes to the source after the enumerable is obtained may affect results.
- `Median<T>` requires the element type to implement `IComparable<T>`; otherwise an `InvalidOperationException` is thrown at runtime.
- `OrderByMany` applies the supplied key selectors in sequence, similar to `ThenBy` in LINQ, and uses the default comparer for each key type.
- The randomization performed by `Random` and `Shuffle` uses the default `System.Random` instance; for cryptographic‑grade randomness, a custom implementation should be used.
