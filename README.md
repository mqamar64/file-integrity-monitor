# File Integrity Monitor (ASP.NET MVC)

## Overview
This project is a File Integrity Monitoring system built using ASP.NET Core MVC and SQLite.

It allows users to:
- Create baseline scans of directories
- Run new scans
- Automatically detect file changes
- View comparison results (new, deleted, modified, unchanged files)



## Requirements

- .NET SDK (version 8.0 or later recommended)

Check installation:
dotnet --version



## How to Run

1. Extract the project zip
2. Open a terminal in the project folder

3. Restore dependencies:
dotnet restore

4. Build the project:
dotnet build

5. Run the application:
dotnet run

6. Open the browser and go to:
http://localhost:5113



## Usage

1. Click "Create Baseline"
   - Enter a name
   - Enter a folder path on your system

2. Click "Run Scan"
   - Use the same folder path

3. Click "Details"
   - Automatically shows comparison results



## Notes

- The application uses SQLite and creates a local database file automatically.
- Make sure the folder path entered exists on your machine.



## Author
Mohammad Qamar, Adam Hammoud