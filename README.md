
# Run Tests

This document contains only the commands required to run the test suite.

Prerequisite
- .NET SDK 10.0 installed and on PATH

## Using Visual Studio

1. Open `RestfulApiTests.sln` in Visual Studio 2022 or newer.
2. Wait for NuGet restore to complete (watch the status bar).

   If restore does not start automatically, do one of the following manually:

   - Right-click the solution in Solution Explorer and choose `Restore NuGet Packages`.
   - Open "Tools → NuGet Package Manager → Package Manager Console" and run:

     ```powershell
     dotnet restore
     ```
3. Build the solution: Build → Build Solution.
4. Open "Test Explorer": Test → Test Explorer.
5. Run tests by clicking Run All or right-click individual tests. View test output in the "Output" window (select the Tests pane) or inspect logs in Test Explorer.

