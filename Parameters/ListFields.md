# List fields

A property group can have a list of ints, doubles, strings or bools as one input field. In the
battery editor it appears as an editable list; in the battery JSON and in task logs (the
`Parameters` header) it is a JSON array.

## Declaring one

Same pattern as scalar fields: a class-level display attribute plus a `[DisplayInputField]`
property. The property must be a `List<T>` of exactly the attribute's item type.

```csharp
[IntListFieldDisplay("PracticeTrials", "Practice Trials",
    initial: new[] { 0, 1, 1, 0 }, minimum: 0, maximum: 1,
    minimumCount: 1, maximumCount: 50)]
public class MyStage : GeneralBatteryStageInfo
{
    [DisplayInputField("PracticeTrials")]
    public List<int> PracticeTrials { get; set; }
}
```

| Attribute | Item type | Item options |
|---|---|---|
| `IntListFieldDisplay` | `int` | `minimum`, `maximum`, `postfix`, `newItem` |
| `DoubleListFieldDisplay` | `double` | `minimum`, `maximum`, `postfix`, `newItem` |
| `StringListFieldDisplay` | `string` | `newItem` |
| `BoolListFieldDisplay` | `bool` | `newItem` |

All of them take `initial` (the items of a new stage), `minimumCount` / `maximumCount`, and
`legacyDelimiters` (below). `newItem` is what Add appends; it is clamped into the item range.

A list field cannot have a `[DisplayInputFieldKey]`: script variables hold single values. PART's
`ListFieldDeclarationTests` checks every property group for this and for a matching attribute.

## Saved format

```json
"PracticeTrials": [0, 1, 1, 0]
```

An empty list is saved as `[]`. A key missing from a saved battery loads the attribute's
`initial` items, as for scalar fields.

## Upgrading an existing field to a list

Old batteries keep loading, with no data version bump:

- **A former delimited-string field** (e.g. `"4,4,0,5"`): give the attribute
  `legacyDelimiters: new[] { ',' }` (or several, e.g. `{ ',', ';', '\n' }`; `'\n'` also covers
  `\r\n` and `\r`). The string is split, pieces are trimmed, empty pieces are dropped, and each
  piece is parsed with the invariant culture (`.` decimal point). An empty string becomes an
  empty list. The battery is saved as an array from then on.
- **A former single-value field** (e.g. `5`): loads as a one-item list.

Match the delimiters to what the old code accepted. If the old code treated an empty string
as "not set", make the task treat an empty list the same way.

## Values that cannot be read

If a saved value cannot be read as the list (e.g. `"4,x,5"` in an int list), the field loads
as an empty list, an error is logged, and the original value is **kept and saved back
unchanged** until someone edits that list. The editor shows the value in red. A battery is
never silently given different values. Tasks that must not run in this state can check
`GetUnparsedListFields()` from `CurrentValuesValid()`.

## Bounds

As with scalar fields, item ranges and counts are enforced by the editor, not on load:

- Typed values are clamped to `minimum`..`maximum`.
- Add and Copy are disabled at `maximumCount`, Delete at `minimumCount`.
- A battery that loads with too many or too few items keeps them; a warning is logged and the
  editor shows the count in red. `ListFieldDisplayAttribute.IsListValid` reports it.

## Editor

A header row shows the title, the item count with its allowed range, and Add. Each item gets
a row: number, value, ▲ ▼ (move), Copy (duplicate below), ✕ (delete).
