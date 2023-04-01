# Ports

| Name                    | Type                 | Category | Convertable                                                       |
| ----------------------- | -------------------- | -------- | ----------------------------------------------------------------- |
| StringPort              | String               | Field    | Numeric, Boolean, Rectangle, Enum                                 |
| NumericPort             | Double               | Field    | String, Boolean, Enum                                             |
| BooleanPort             | Boolean              | Field    | String, Numeric                                                   |
| EnumPort                | Enum                 | Field    | String, Numeric                                                   |
| FolderPort              | String               | Field    | String                                                            |
| RectanglePort           | Rectangle            | Shape    | String                                                            |
| ImagePort               | Image                | Image    |                                                                   |
| StringCollectionPort    | String Collection    | Field    | String (Json), Numeric (Count), Boolean (Any)                     |
| NumericCollectionPort   | Numeric Collection   | Field    | String (Json), Numeric (Count), String Collection                 |
| RectangleCollectionPort | Rectangle Collection | Field    | Rectangle (First), String (Json), Numeric (Count), Boolean (Any)  |
