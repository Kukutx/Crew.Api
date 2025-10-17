# Crew.SeedDataImporter

该项目是一个简单的控制台工具，用于从 Excel 工作簿中读取种子数据并导入到 Crew 项目的 PostgreSQL 数据库中。它重用现有的 `InfrastructureModule` 来与 `AppDbContext` 建立连接，因此配置方式与主站点相同。

## Excel 模板

Excel 文件可以包含以下工作表：

| Sheet 名称 | 作用 | 需要的列（按顺序） |
| --- | --- | --- |
| `Users` | 创建/更新用户 | `FirebaseUid`, `DisplayName`, `Role`, `Bio`, `AvatarUrl`, `CreatedAt` |
| `Tags` | 创建/更新标签 | `Name`, `Category`, `CreatedAt` |
| `UserTags` | 建立用户-标签关系 | `FirebaseUid`, `TagName` |
| `Events` | 创建/更新活动 | `Title`, `OwnerFirebaseUid`, `Description`, `StartTime`, `EndTime`, `StartLatitude`, `StartLongitude`, `EndLatitude`, `EndLongitude`, `MaxParticipants`, `Visibility`, `RoutePolyline` |
| `EventSegments` | 为活动添加路段 | `EventTitle`, `Sequence`, `Latitude`, `Longitude`, `Note` |
| `EventTags` | 建立活动-标签关系 | `EventTitle`, `TagName` |
| `Registrations` | 为活动创建报名记录 | `EventTitle`, `FirebaseUid`, `Status`, `CreatedAt` |

> **提示**：所有列都支持留空（除必须字段外），工具会跳过空行，并在引用不存在的用户、标签或活动时写入警告日志。

## 配置与运行

1. 在 `Crew.SeedDataImporter/appsettings.json` 中填写数据库连接字符串，或者通过环境变量 `ConnectionStrings__Default` 覆盖。
2. 将 Excel 文件放在项目根目录，或者在配置中设置 `Seed:ExcelPath`。
3. 运行命令：

```bash
DOTNET_ENVIRONMENT=Development dotnet run --project Crew.SeedDataImporter -- Seed:ExcelPath="/path/to/seed-data.xlsx" Seed:OverwriteExisting=true
```

> 如果数据库已经存在记录，可以将 `Seed:OverwriteExisting` 设置为 `true` 来更新可覆盖的字段（不会删除不存在于 Excel 中的记录）。

导入过程中可以使用 `Ctrl+C` 取消。
