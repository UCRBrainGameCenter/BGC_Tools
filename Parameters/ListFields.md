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

## Tuple lists

For items with 2 to 4 values that each mean something, too small to deserve a property group
(e.g. one row per difficulty level), use a list of value tuples. Each column is declared with
its own class-level attribute, so a subclass can redeclare one to change its title, bounds or
initial values.

```csharp
[TupleListFieldDisplay("DifficultyLevels", "Difficulty Levels", minimumCount: 1)]
[IntTupleColumn("DifficultyLevels", 0, "MaxWallWidth", "Max Wall Width",
    initial: new[] { 4, 3, 2, 1 })]
[DoubleTupleColumn("DifficultyLevels", 1, "ObstacleChance", "Obstacle Chance",
    initial: new[] { 0, 0.2, 0.4, 0.6 }, minimum: 0, maximum: 1)]
public class MyConfig : CommonPropertyGroup
{
    [DisplayInputField("DifficultyLevels")]
    public List<(int MaxWallWidth, double ObstacleChance)> DifficultyLevels { get; set; }
}
```

- Columns: `IntTupleColumn`, `DoubleTupleColumn` (`minimum`, `maximum`, `postfix`, `newItem`),
  `StringTupleColumn`, `BoolTupleColumn` (`newItem`). Arguments: field name, index (0 =
  `Item1`), column name, title. Every column's `initial` must have the same length.
- The tuple's element names (`MaxWallWidth` above) are for code only; titles and saved names
  come from the column attributes.
- **Saved format:** one object per row, keyed by column name:
  `[{"MaxWallWidth":4,"ObstacleChance":0}, ...]`. A saved row missing a column, or with a key
  that matches no column, makes the value unreadable (preserved as above): adding, renaming or
  removing a column never silently fills in, changes or drops data. When you change the
  columns, upgrade the saved rows explicitly (see below).
- **Legacy string:** set both `legacyDelimiters` (between rows) and `legacyColumnDelimiters`
  (between columns), e.g. `"4,0; 3,0.2"`. Every row must have every column.
- **Several old fields merged into one tuple list, or a column added,** need code: override the
  group's `Deserialize`, build the new field's value from the old data on a copy of the
  `JsonObject`, then call `base.Deserialize` with the copy.
  GoNoGo's `DiscriminabilityDifficultyConfig` is the example. Stage migrations
  (`[StageMigration]`) only see top-level stages, so this also works for nested groups.
- Not supported: mirrored tuple lists, script-variable keys.
- The editor shows the column titles above the rows, and each row gets one value per column.
  The info panel shows rows as `(4, 0), (3, 0.2)`.
