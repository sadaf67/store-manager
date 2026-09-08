// ================================================
// این فایل "دستیار دیتابیس" ماست
// هر بار که بخوایم با SQL Server حرف بزنیم
// از این کلاس استفاده میکنیم
// مثل یه واسطه بین برنامه و دیتابیس
// ================================================

using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace StoreManager.DAL
{
    // static یعنی نیازی نیست این کلاس رو new کنیم
    // مستقیم صداش میزنیم: DatabaseHelper.ExecuteQuery(...)
    public static class DatabaseHelper
    {
        // آدرس وصل شدن به دیتابیس - مثل آدرس خونه
        // مثلاً: "Server=.;Database=StoreManagerDB;Integrated Security=True"
        // این مقدار از App.config خونده میشه
        public static string ConnectionString { get; set; }

        // ---- یه اتصال جدید به دیتابیس میسازه ----
        // مثل باز کردن در برای رفتن داخل دیتابیس
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        // ---- برای خوندن اطلاعات از دیتابیس ----
        // sql = دستوری که میخوایم اجرا کنیم (مثلاً "SELECT * FROM Products")
        // parameters = مقادیری که توی دستور جای میزاریم (برای امنیت)
        // برمیگردونه: یه جدول از نتایج (مثل اکسل)
        public static DataTable ExecuteQuery(string sql, SqlParameter[] parameters = null)
        {
            var dt = new DataTable(); // جدول خالی آماده میکنیم

            using var con = GetConnection();   // در رو باز میکنیم
            using var cmd = new SqlCommand(sql, con); // دستور رو آماده میکنیم

            // اگه پارامتر داریم اضافه میکنیم
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            con.Open(); // وصل میشیم به دیتابیس

            // نتیجه رو داخل جدول میریزیم
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);

            return dt; // جدول پر شده رو برمیگردونیم
        }

        // ---- برای تغییر دادن داده (INSERT / UPDATE / DELETE) ----
        // برمیگردونه: تعداد ردیف‌هایی که تغییر کردن
        public static int ExecuteNonQuery(string sql, SqlParameter[] parameters = null)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            con.Open();
            return cmd.ExecuteNonQuery(); // اجرا کن و تعداد تغییرات رو بده
        }

        // ---- برای گرفتن یه مقدار تکی از دیتابیس ----
        // مثلاً COUNT(*) یا MAX(Id) یا SCOPE_IDENTITY()
        // برمیگردونه: فقط یه عدد یا متن
        public static object ExecuteScalar(string sql, SqlParameter[] parameters = null)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            con.Open();
            return cmd.ExecuteScalar(); // فقط اولین سلول از اولین ردیف رو برمیگردونه
        }

        // ---- اول دیتابیس رو میسازه اگه وجود نداشته باشه ----
        // این متد فقط یه بار اول اجرا میشه (توی App.xaml.cs)
        private static void EnsureDatabaseExists()
        {
            // از connection string آدرس وصل شدن رو میخونیم
            var builder = new SqlConnectionStringBuilder(ConnectionString);

            // اسم دیتابیس رو نگه میداریم (مثلاً "StoreManagerDB")
            string dbName = builder.InitialCatalog;

            // آدرس رو تغییر میدیم که به "master" وصل بشیم
            // master = دیتابیس اصلی SQL Server که همیشه هست
            builder.InitialCatalog = "master";
            string masterConnStr = builder.ConnectionString;

            // به master وصل میشیم و دیتابیس خودمون رو میسازیم
            using var con = new SqlConnection(masterConnStr);
            con.Open();

            // IF NOT EXISTS = اگه وجود نداشت بساز (اگه داشت کاری نکن)
            using var cmd = new SqlCommand(
                $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}') " +
                $"CREATE DATABASE [{dbName}];", con);
            cmd.ExecuteNonQuery();
        }

        // ---- ساخت همه جداول دیتابیس ----
        // این متد وقتی برنامه اول بار اجرا میشه صدا زده میشه
        // اگه جدول‌ها از قبل باشن کاری نمیکنه (IF NOT EXISTS)
        public static void InitializeDatabase()
        {
            // گام ۱: اول دیتابیس رو بساز اگه نبود
            EnsureDatabaseExists();

            // گام ۲: حالا جداول رو بساز
            // این یه دستور SQL بزرگه که همه جداول رو یه جا میسازه
            string script = @"
-- جدول دسته‌بندی کالاها
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Categories')
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY,       -- شماره خودکار
    Name NVARCHAR(100) NOT NULL,       -- اسم دسته‌بندی
    Icon NVARCHAR(50),                 -- آیکون
    ParentId INT DEFAULT 0,            -- زیرمجموعه کیه؟
    IsActive BIT DEFAULT 1             -- فعاله؟
);

-- جدول کالاها
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Products')
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(50) UNIQUE,          -- کد یونیک کالا
    Name NVARCHAR(200) NOT NULL,       -- اسم کالا
    CategoryId INT REFERENCES Categories(Id), -- دسته‌بندیش چیه؟
    Unit NVARCHAR(30),                 -- واحد (عدد/کیلو/متر...)
    BuyPrice DECIMAL(18,2) DEFAULT 0,  -- قیمت خرید
    SellPrice DECIMAL(18,2) DEFAULT 0, -- قیمت فروش
    Stock DECIMAL(18,3) DEFAULT 0,     -- موجودی الان
    MinStock DECIMAL(18,3) DEFAULT 0,  -- حداقل موجودی (برای هشدار)
    Description NVARCHAR(500),         -- توضیحات
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE() -- تاریخ اضافه شدن
);

-- جدول مشتریان
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Customers')
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(30),                 -- کد مشتری
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20),
    Mobile NVARCHAR(20),
    Address NVARCHAR(500),
    CreditLimit DECIMAL(18,2) DEFAULT 0,  -- سقف نسیه
    Balance DECIMAL(18,2) DEFAULT 0,      -- مانده حساب (منفی = بدهکار)
    NationalCode NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- جدول سرفاکتورها (اطلاعات کلی هر فاکتور)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Invoices')
CREATE TABLE Invoices (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceNo NVARCHAR(30) UNIQUE,     -- شماره فاکتور (مثلاً S202406001)
    Type INT NOT NULL,                 -- 0=فروش 1=خرید 2=برگشت
    CustomerId INT REFERENCES Customers(Id),
    Date DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) DEFAULT 0,  -- جمع کل کالاها
    Discount DECIMAL(18,2) DEFAULT 0,     -- تخفیف کل
    Tax DECIMAL(18,2) DEFAULT 0,          -- مالیات
    FinalAmount DECIMAL(18,2) DEFAULT 0,  -- مبلغ نهایی
    PaidAmount DECIMAL(18,2) DEFAULT 0,   -- مبلغ پرداخت شده
    PaymentType INT DEFAULT 0,            -- نوع پرداخت
    Description NVARCHAR(500)
);

-- جدول ردیف‌های فاکتور (هر کالا توی فاکتور)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='InvoiceItems')
CREATE TABLE InvoiceItems (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceId INT REFERENCES Invoices(Id) ON DELETE CASCADE, -- اگه فاکتور حذف شد ردیف‌هاش هم حذف بشن
    ProductId INT REFERENCES Products(Id),
    Qty DECIMAL(18,3) NOT NULL,        -- تعداد
    UnitPrice DECIMAL(18,2) NOT NULL,  -- قیمت واحد
    Discount DECIMAL(18,2) DEFAULT 0   -- تخفیف این ردیف
);

-- جدول تراکنش‌های انبار (هر بار کالا وارد یا خارج شد)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='StockTransactions')
CREATE TABLE StockTransactions (
    Id INT PRIMARY KEY IDENTITY,
    ProductId INT REFERENCES Products(Id),
    Type INT NOT NULL,                 -- نوع: خرید/فروش/تنظیم/برگشت
    Qty DECIMAL(18,3) NOT NULL,
    UnitPrice DECIMAL(18,2) DEFAULT 0,
    InvoiceId INT,                     -- مربوط به کدوم فاکتور؟
    Date DATETIME DEFAULT GETDATE(),
    Description NVARCHAR(500)
);

-- جدول تنظیمات برنامه (کلید=مقدار)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='AppSettings')
CREATE TABLE AppSettings (
    Id INT PRIMARY KEY IDENTITY,
    [Key] NVARCHAR(100) UNIQUE NOT NULL,   -- اسم تنظیم (مثلاً 'StoreName')
    [Value] NVARCHAR(500)                  -- مقدار (مثلاً 'فروشگاه علی')
);

-- مقادیر پیش‌فرض تنظیمات (فقط اگه جدول خالی بود)
IF NOT EXISTS (SELECT * FROM AppSettings WHERE [Key]='StoreName')
INSERT INTO AppSettings ([Key],[Value]) VALUES
    ('StoreName',N'فروشگاه من'),
    ('StoreType',N'عمومی'),
    ('OwnerName',N''),
    ('Phone',N''),
    ('Address',N''),
    ('TaxRate','0'),
    ('Currency',N'ریال'),
    ('PrintAfterSale','0'),
    ('ShowLowStockAlert','1');
";
            // اجرای اسکریپت بزرگ برای ساخت جداول
            ExecuteNonQuery(script);
        }
    }
}
