# Development Setup Guide

This project is a medical web application built with **ASP.NET Core (net8.0)** and **Entity Framework Core (SQL Server)**. 
This guide explains how to set up your local development environment and run the application using **Visual Studio 2022** and **SQL Server (LocalDB)**.

---

## 1. Prerequisites

Before getting started, make sure you have the following tools installed:

* **Visual Studio 2022** (Community Edition or higher is recommended)
  * During installation, ensure you select the following workloads:
    * **ASP.NET and web development** (Required for C# web app development)
    * **.NET desktop development** (This includes SQL Server LocalDB)
* **.NET 8.0 SDK** (Installed automatically with Visual Studio 2022)
* **Git** (For cloning and version control)

---

## 2. Setup Steps

### Step 1: Clone the Repository and Load the Solution
1. Open **Visual Studio 2022**.
2. In the start window, select **"Clone a repository"**.
3. In the **"Repository location"** field, enter the Git repository URL.
4. Specify a local **"Path"** of your choice and click **"Clone"**.
5. Once the cloning process is complete, Visual Studio will automatically load the project.
   * *Note: If the "Solution Explorer" only displays folder directories (Folder View), double-click **`medicalapp.sln`** inside the Solution Explorer to open the solution.*

### Step 2: Create the Database and Apply Migrations
Use the Package Manager Console in Visual Studio to create the database and apply the initial schema.

1. Go to the top menu and select **Tools > NuGet Package Manager > Package Manager Console**.
2. Make sure the "Default project" dropdown at the top of the console is set to `medicalapp`.
3. Run the following command:
   ```powershell
   Update-Database
   ```
   * This command reads the connection string from `appsettings.json`, automatically creates the database in your local SQL Server (LocalDB) instance, and applies the latest migrations.
   * *Alternative (CLI): If you prefer the command line, you can navigate to the `medicalapp` folder and run `dotnet ef database update` instead.*

---

## 3. Running the Application

1. Click the green Start button (labeled "IIS Express" or "medicalapp") at the top of Visual Studio, or press **[F5]** (Start Debugging) / **[Ctrl + F5]** (Start Without Debugging).
2. A web browser will automatically open and navigate to the application's homepage.
   * URL: `https://localhost:7146` (or the dynamic port configured on your machine)

---

## 4. Initial Test Accounts

Upon database initialization, the following test users are automatically registered by `DbInitializer.cs`. You can use them to test the login functionality:

| Role | Email Address | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@medicloud.com` | `Admin123!` |
| **Doctor** | `doctor@medicloud.com` | `Doctor123!` |
| **Patient** | `patient@medicloud.com` | `Patient123!` |

---

## 5. Guidelines for Collaborative Development

* **Git Commit Limits**:
  Do not commit build artifacts (`bin/`, `obj/`) or user-specific IDE settings (`.vs/`, `*.csproj.user`) to Git. These are already excluded by the `.gitignore` file.
* **Managing Connection Strings**:
  The default database settings for local development are configured to use `(localdb)\mssqllocaldb` in `appsettings.json`. If you modify this file to connect to a different database instance, be careful not to commit your local configuration changes.
  * *Tip: For production credentials or local secrets, it is highly recommended to use `dotnet user-secrets`.*
