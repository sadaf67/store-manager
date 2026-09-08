// ================================================
// این فایل "مغز گزارش‌ها" ماست
// همه گزارش‌هایی که کاربر میتونه ببینه از اینجا میان:
//   - فروش روزانه
//   - فروش به تفکیک کالا
//   - بدهی مشتری‌ها
//   - کالاهای کم‌موجود
//   - سود و زیان
//   - آمار کلی داشبورد
// ================================================

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using StoreManager.DAL;

namespace StoreManager.BLL.Services
{
    // این کلاس مسئول تهیه همه گزارش‌هاست
    public class ReportService
    {
        // ---- گزارش فروش یه روز خاص ----
        // date = تاریخ مورد نظر
        // برمیگردونه: یه جدول با ستون‌های: تاریخ، تعداد فاکتور، جمع فروش، دریافتی، مانده
        public DataTable GetDailySales(DateTime date)
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT CONVERT(NVARCHAR,i.Date,111) AS تاریخ,
                       COUNT(*) AS تعداد_فاکتور,
                       SUM(i.FinalAmount) AS جمع_فروش,
                       SUM(i.PaidAmount) AS دریافتی,
                       SUM(i.FinalAmount-i.PaidAmount) AS مانده
                FROM Invoices i WHERE i.Type=0
                AND CAST(i.Date AS DATE)=@d
                GROUP BY CONVERT(NVARCHAR,i.Date,111)",
                new[] { new SqlParameter("@d", date.Date) });
        }

        // ---- گزارش فروش به تفکیک هر کالا در یه بازه زمانی ----
        // from/to = از چه تاریخی تا چه تاریخی
        // برمیگردونه: جدول با ستون‌های: کالا، واحد، مقدار فروش، مبلغ فروش، موجودی فعلی
        public DataTable GetSalesByProduct(DateTime from, DateTime to)
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT p.Name AS کالا, p.Unit AS واحد,
                       SUM(ii.Qty) AS مقدار_فروش,
                       SUM(ii.Qty * ii.UnitPrice) AS مبلغ_فروش,
                       p.Stock AS موجودی
                FROM InvoiceItems ii
                JOIN Products p ON ii.ProductId=p.Id
                JOIN Invoices i ON ii.InvoiceId=i.Id
                WHERE i.Type=0 AND i.Date BETWEEN @from AND @to
                GROUP BY p.Name,p.Unit,p.Stock
                ORDER BY مبلغ_فروش DESC",  // بیشترین فروش اول
                new[] { new SqlParameter("@from", from), new SqlParameter("@to", to.AddDays(1)) });
        }

        // ---- لیست مشتری‌های بدهکار ----
        // برمیگردونه: جدول با ستون‌های: مشتری، موبایل، مبلغ بدهی
        // Balance منفی یعنی بدهکار - ABS مقدار مطلق (بدون علامت منفی) رو میده
        public DataTable GetCustomerDebts()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT Name AS مشتری, Mobile AS موبایل,
                       ABS(Balance) AS بدهی
                FROM Customers WHERE Balance<0 AND IsActive=1
                ORDER BY Balance");  // بیشترین بدهی اول (Balance بیشتر منفی = بدهی بیشتر)
        }

        // ---- لیست کالاهایی که موجودیشون زیر حداقل افتاده ----
        // این گزارش برای هشدار کم‌موجودی استفاده میشه
        public DataTable GetLowStockProducts()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT p.Name AS کالا, c.Name AS دسته_بندی,
                       p.Stock AS موجودی, p.MinStock AS حداقل_موجودی,
                       p.Unit AS واحد
                FROM Products p LEFT JOIN Categories c ON p.CategoryId=c.Id
                WHERE p.Stock<=p.MinStock AND p.MinStock>0 AND p.IsActive=1
                ORDER BY p.Stock");  // کم‌موجودترین اول
        }

        // ---- گزارش سود و زیان در یه بازه زمانی ----
        // سود = درآمد فروش - بهای تمام شده (قیمت خریدمون)
        // برمیگردونه: جدول با ستون‌های: کالا، مقدار، بهای تمام شده، درآمد، سود
        public DataTable GetProfitReport(DateTime from, DateTime to)
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT p.Name AS کالا,
                       SUM(ii.Qty) AS مقدار,
                       SUM(ii.Qty * p.BuyPrice) AS بهای_تمام_شده,
                       SUM(ii.Qty * ii.UnitPrice - ii.Discount) AS درآمد,
                       SUM(ii.Qty * ii.UnitPrice - ii.Discount) - SUM(ii.Qty * p.BuyPrice) AS سود
                FROM InvoiceItems ii
                JOIN Products p ON ii.ProductId=p.Id
                JOIN Invoices i ON ii.InvoiceId=i.Id
                WHERE i.Type=0 AND i.Date BETWEEN @from AND @to
                GROUP BY p.Name
                ORDER BY سود DESC",  // سودآورترین کالا اول
                new[] { new SqlParameter("@from", from), new SqlParameter("@to", to.AddDays(1)) });
        }

        // ---- آمار کلی برای صفحه داشبورد ----
        // این متد با یه کوئری SQL، همه اعداد مهم رو یه‌جا میگیره
        // برمیگردونه: یه دیکشنری از اعداد (مثل: {"TodaySales": 5000000})
        public Dictionary<string, decimal> GetDashboardStats()
        {
            var today = DateTime.Today;
            var result = new Dictionary<string, decimal>();

            // این یه کوئری بزرگه که ۶ تا عدد مهم رو همزمان حساب میکنه:
            // TodaySales   = فروش امروز
            // MonthSales   = فروش این ماه
            // ProductCount = تعداد کالاهای فعال
            // CustomerCount= تعداد مشتریان فعال
            // LowStockCount= تعداد کالاهای کم‌موجود (برای نشان‌دهنده هشدار)
            // TotalDebt    = جمع بدهی همه مشتریان
            var row = DatabaseHelper.ExecuteQuery(@"
                SELECT
                    (SELECT ISNULL(SUM(FinalAmount),0) FROM Invoices WHERE Type=0 AND CAST(Date AS DATE)=CAST(GETDATE() AS DATE)) AS TodaySales,
                    (SELECT ISNULL(SUM(FinalAmount),0) FROM Invoices WHERE Type=0 AND MONTH(Date)=MONTH(GETDATE()) AND YEAR(Date)=YEAR(GETDATE())) AS MonthSales,
                    (SELECT COUNT(*) FROM Products WHERE IsActive=1) AS ProductCount,
                    (SELECT COUNT(*) FROM Customers WHERE IsActive=1) AS CustomerCount,
                    (SELECT COUNT(*) FROM Products WHERE Stock<=MinStock AND MinStock>0 AND IsActive=1) AS LowStockCount,
                    (SELECT ABS(ISNULL(SUM(Balance),0)) FROM Customers WHERE Balance<0 AND IsActive=1) AS TotalDebt").Rows[0];

            // نتایج رو توی دیکشنری بریز
            result["TodaySales"] = Convert.ToDecimal(row["TodaySales"]);
            result["MonthSales"] = Convert.ToDecimal(row["MonthSales"]);
            result["ProductCount"] = Convert.ToDecimal(row["ProductCount"]);
            result["CustomerCount"] = Convert.ToDecimal(row["CustomerCount"]);
            result["LowStockCount"] = Convert.ToDecimal(row["LowStockCount"]);
            result["TotalDebt"] = Convert.ToDecimal(row["TotalDebt"]);
            return result;
        }
    }
}
