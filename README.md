# StoreManager (سیستم مدیریت فروشگاه)

A Windows desktop point-of-sale / inventory management app built with WPF and a layered
architecture — built for small shops of any trade (کالا و موجودی، فروش/خرید، بدهکاران و...).

## Architecture

```
StoreManager.sln
├── StoreManager.Models    ← POCO models (Product, Category, Customer, Invoice, StockTransaction, Settings)
├── StoreManager.DAL       ← Data access (ADO.NET/SqlClient repositories, DatabaseHelper)
├── StoreManager.BLL       ← Business logic layer
└── StoreManager.UI        ← WPF desktop UI (RTL, Persian)
```

## Tech stack

| Layer | Technology |
|---|---|
| Platform | .NET, WPF (Windows desktop) |
| Database | SQL Server (Express or LocalDB), ADO.NET |
| UI | WPF, RTL Persian interface |

## Features

- **Product & category management** — products carry a code, unit, buy/sell price, current
  stock and a minimum-stock threshold for low-stock alerts.
- **Sales & purchase invoices** — `Invoice` supports three types (Sale/Purchase/Return) and
  four payment methods (Cash/Credit/Card/Mixed), with per-line discounts and automatic
  remaining-balance calculation (`FinalAmount - PaidAmount`).
- **Inventory tracking** — stock automatically moves in/out as invoices are recorded
  (`StockTransaction`).
- **Customer & debtor management** — track customers and outstanding balances from
  credit sales.
- **Reports** — sales, profit, and low-stock reports, with CSV export.
- **Statistics dashboard.**
- **Works for any type of shop**, not tied to a specific product category.

## Local setup

Prerequisites: Visual Studio 2022 (.NET Desktop Development workload), .NET 6 SDK, SQL Server
or SQL Server LocalDB.

1. Open `StoreManager.sln` in Visual Studio.
2. Set the connection string in `StoreManager.UI/App.config` to match your SQL Server instance
   (defaults to a local `.\SQLEXPRESS` instance with Windows/Integrated Security — no password
   stored in config).
3. Build (`Ctrl+Shift+B`) and run (`F5`). The database is created automatically on first run.

## Disclaimer

This is a software-engineering demo project.
