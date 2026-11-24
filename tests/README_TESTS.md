# Isolated tests

With the .NET 8 SDK installed, run this command from the mod project directory.

```sh
dotnet run --project tests/OffWithTheirNuts.Tests.csproj
```

The test project uses the pinned Valheim 1.0.21 assemblies from the workspace. To use a different location, pass `-p:ValheimManagedDir=...` with the path to the `Managed` directory.

The tests compile the mod's source code and use Valheim's `ZPackage` to write and read packets. World state, peers, and time are simulated. Live connections, Harmony patches, and UI rendering require in-game testing.
