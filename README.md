# Warhammer Army Calculator

A full-stack app for building Warhammer army lists and calculating expected damage.

* **WarCalc.Api** – ASP.NET Core Web API (.NET 9) + Entity Framework Core 9 (code-first, SQLite)
* **WarCalc.Wpf** – multi-window WPF client (.NET 9, Windows only) that talks to the API over HTTP

## How to run

1. Start the API (creates and seeds `warcalc.db` automatically on first start):
   ```
   cd WarCalc.Api
   dotnet run
   ```
   Swagger UI: <http://localhost:5080/swagger>
2. Start the WPF client (the API must be running):
   ```
   cd WarCalc.Wpf
   dotnet run
   ```
   The API address is set in `WarCalc.Wpf/Api.cs` (`BaseAddress`).

Or open `WarCalc.sln` in Visual Studio 2022 and start both projects (Solution > Configure Startup Projects > Multiple).

## Database creation parameters

| Setting | Value |
|---|---|
| Engine | SQLite (`Microsoft.EntityFrameworkCore.Sqlite` 9.x) |
| Approach | Code-first, `Database.EnsureCreated()` in `Program.cs` |
| Connection string | `Data Source=warcalc.db` (`appsettings.json`) |
| File location | `WarCalc.Api/warcalc.db` (created on first run, git-ignored) |
| Seed data | `Data/WarContext.cs` (`HasData`): 3 factions, 7 units, 11 weapons, 1 army |
| Delete the file | to reset the database to the seed data |

### Tables

**Factions**: `Id` INTEGER PK autoincrement · `Name` TEXT(80) NOT NULL, UNIQUE · `Description` TEXT(300) NULL

**Units**: `Id` PK · `Name` TEXT(80) NOT NULL · `FactionId` INTEGER NOT NULL FK → Factions(Id) ON DELETE CASCADE ·
`ModelCount`, `Points`, `Move`, `Toughness`, `Save` (3 = 3+, 7 = none), `InvulnerableSave` (NULL = none), `Wounds`, `Leadership`, `ObjectiveControl` INTEGER

**Weapons**: `Id` PK · `Name` TEXT(80) NOT NULL · `UnitId` INTEGER NOT NULL FK → Units(Id) ON DELETE CASCADE ·
`RangeInches` INTEGER (0 = melee) · `Attacks` REAL (average, D6 = 3.5) · `Skill` INTEGER (BS/WS, 3 = 3+) · `Strength` INTEGER ·
`Ap` INTEGER (AP-2 stored as 2) · `Damage` REAL (average)

**Armies**: `Id` PK · `Name` TEXT(80) NOT NULL · `FactionId` INTEGER NOT NULL FK → Factions(Id) ON DELETE CASCADE · `PointsLimit` INTEGER

**ArmyUnits** (army ⇄ unit lines): `Id` PK · `ArmyId` FK → Armies(Id) CASCADE · `UnitId` FK → Units(Id) CASCADE ·
`Quantity` INTEGER · UNIQUE (`ArmyId`, `UnitId`)

```
Factions 1──* Units 1──* Weapons
Factions 1──* Armies 1──* ArmyUnits *──1 Units
```

To switch to migrations: `dotnet ef migrations add Initial`, then replace `EnsureCreated()` with `Migrate()`.
For SQL Server, swap the provider/connection string and change one cascade path
(e.g. `ArmyUnit → Unit`) to `Restrict`, since SQL Server rejects multiple cascade paths.

## API endpoints

Every controller has full CRUD (`GET all`, `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}`) plus custom endpoints:

| Controller | Custom endpoints |
|---|---|
| `/api/factions` | `GET {id}/units` – all units of a faction |
| `/api/units` | `GET search?name=&factionId=&maxPoints=` – filtered search |
| `/api/weapons` | `GET {id}/damage/{targetUnitId}?attackers=N` – expected damage (hit → wound → save → damage maths) |
| `/api/armies` | `GET {id}/summary` – total/remaining points and validity<br>`POST {id}/units` body `{ "unitId": 1, "quantity": 2 }` – add unit (faction checked)<br>`DELETE {id}/units/{unitId}` – remove unit |

### Damage maths (`Services/DamageCalculator.cs`)

* Hit / wound / save chances are "X+" on a D6, clamped to 1/6 … 5/6
* Wound roll: S ≥ 2T → 2+, S > T → 3+, S = T → 4+, S < T → 5+, S ≤ T/2 → 6+
* Save = Sv + AP, or the invulnerable save if better
* Damage per unsaved wound is capped at the target's Wounds; models slain is capped at the unit's size

Seed stats are sample values, only approximately those of the real game.

## WPF windows

| Window | Purpose |
|---|---|
| Main window | Menu to open the other windows |
| Army builder | Army CRUD, add/remove units, live points total vs. limit |
| Units | Unit CRUD, filter by faction/name (uses `search`) |
| Weapons & damage calculator | Weapon CRUD for a unit; calculate expected damage vs. any target unit |
| Factions | Faction CRUD |
