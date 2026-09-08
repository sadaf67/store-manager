// ================================================
// این فایل "انبار اطلاعات فاکتورهاست"
// همه کارهایی که با فاکتورها داریم از اینجا انجام میشه
// مثلاً: لیست فاکتورها، خوندن یه فاکتور کامل،
//        ذخیره فاکتور جدید، شماره فاکتور بعدی
// ================================================

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using StoreManager.Models;

namespace StoreManager.DAL.Repositories
{
    // این کلاس مسئول همه کارهای مربوط به فاکتورهاست
    public class InvoiceRepository
    {
        // ---- لیست فاکتورها رو با فیلتر میده ----
        // from = از چه تاریخی (اختیاری)
        // to   = تا چه تاریخی (اختیاری)
        // type = نوع فاکتور: فروش/خرید/برگشت (اختیاری)
        public List<Invoice> GetAll(DateTime? from = null, DateTime? to = null, InvoiceType? type = null)
        {
            var list = new List<Invoice>();

            // این SQL: فاکتورها رو بیار باهاشون اسم مشتری هم بیار
            // اگه تاریخ یا نوع انتخاب شد، فیلتر کن - جدیدترین اول
            string sql = @"SELECT i.*, c.Name AS CustomerName FROM Invoices i
                          LEFT JOIN Customers c ON i.CustomerId=c.Id
                          WHERE (@from IS NULL OR i.Date>=@from)
                          AND (@to IS NULL OR i.Date<=@to)
                          AND (@type IS NULL OR i.Type=@type)
                          ORDER BY i.Date DESC";

            var dt = DatabaseHelper.ExecuteQuery(sql, new[] {
                // اگه مقدار نداشت، NULL بفرست (یعنی این فیلتر رو نادیده بگیر)
                new SqlParameter("@from", from.HasValue ? (object)from.Value : DBNull.Value),
                new SqlParameter("@to", to.HasValue ? (object)to.Value.AddDays(1) : DBNull.Value), // تا آخر روز
                new SqlParameter("@type", type.HasValue ? (object)(int)type.Value : DBNull.Value)
            });

            foreach (System.Data.DataRow row in dt.Rows)
                list.Add(MapHeader(row));
            return list;
        }

        // ---- یه فاکتور کامل رو با همه ردیف‌هاش برمیگردونه ----
        // یعنی: اطلاعات سرفاکتور + لیست همه کالاهایی که توی اون فاکتورن
        public Invoice GetById(int id)
        {
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT i.*,c.Name AS CustomerName FROM Invoices i LEFT JOIN Customers c ON i.CustomerId=c.Id WHERE i.Id=@id",
                new[] { new SqlParameter("@id", id) });

            if (dt.Rows.Count == 0) return null; // اگه پیدا نشد null بده

            var inv = MapHeader(dt.Rows[0]); // سرفاکتور رو بساز
            inv.Items = GetItems(id);         // ردیف‌های کالا رو هم بارگذاری کن
            return inv;
        }

        // ---- ردیف‌های کالای یه فاکتور رو برمیگردونه ----
        // یعنی: هر کالایی که توی اون فاکتور بوده با تعداد و قیمتش
        public List<InvoiceItem> GetItems(int invoiceId)
        {
            var list = new List<InvoiceItem>();

            // این SQL: ردیف‌های فاکتور رو بیار باهاشون اسم کالا و واحد هم بیار
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT ii.*,p.Name AS ProductName,p.Unit FROM InvoiceItems ii JOIN Products p ON ii.ProductId=p.Id WHERE ii.InvoiceId=@id",
                new[] { new SqlParameter("@id", invoiceId) });

            foreach (System.Data.DataRow row in dt.Rows)
                list.Add(new InvoiceItem {
                    Id = Convert.ToInt32(row["Id"]),
                    InvoiceId = invoiceId,
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"].ToString(),
                    Unit = row["Unit"].ToString(),
                    Qty = Convert.ToDecimal(row["Qty"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    Discount = Convert.ToDecimal(row["Discount"])
                });
            return list;
        }

        // ---- فاکتور رو ذخیره میکنه (اضافه کردن یا ویرایش) ----
        // con و tx = اتصال و تراکنش دیتابیس - اینا از SaleService میان
        // چرا؟ چون ذخیره فاکتور باید همزمان با ذخیره کالاها و آپدیت موجودی باشه
        // اگه یه چیزی خراب شد، همه چیز برمیگرده به حالت اول
        public int Save(Invoice inv, SqlConnection con, SqlTransaction tx)
        {
            if (inv.Id == 0)
            {
                // ---- فاکتور جدید: اضافه کن ----
                using var cmd = new SqlCommand(@"
                    INSERT INTO Invoices(InvoiceNo,Type,CustomerId,Date,TotalAmount,Discount,Tax,FinalAmount,PaidAmount,PaymentType,Description)
                    VALUES(@No,@Type,@CustId,@Date,@Total,@Disc,@Tax,@Final,@Paid,@PayType,@Desc);
                    SELECT SCOPE_IDENTITY();", con, tx);
                AddParams(cmd, inv);
                return Convert.ToInt32(cmd.ExecuteScalar()); // شماره فاکتور جدید رو برگردون
            }

            // ---- فاکتور قبلی: آپدیت مبالغ ----
            using var upd = new SqlCommand(@"
                UPDATE Invoices SET TotalAmount=@Total,Discount=@Disc,Tax=@Tax,
                FinalAmount=@Final,PaidAmount=@Paid,Description=@Desc WHERE Id=@Id", con, tx);
            AddParams(upd, inv);
            upd.ExecuteNonQuery();
            return inv.Id;
        }

        // ---- شماره فاکتور بعدی رو میسازه ----
        // مثلاً: S202406001 = S(فروش) + 202406(سال و ماه) + 0001(شماره)
        // P = خرید، R = برگشت
        public string GetNextInvoiceNo(InvoiceType type)
        {
            string prefix = type == InvoiceType.Sale ? "S" : type == InvoiceType.Purchase ? "P" : "R";
            // تعداد فاکتورهای این نوع رو بشمار و +1 کن
            var count = DatabaseHelper.ExecuteScalar($"SELECT COUNT(*)+1 FROM Invoices WHERE Type={(int)type}");
            // مثلاً: S + 202406 + 0001 = S2024060001
            return $"{prefix}{DateTime.Now:yyyyMM}{Convert.ToInt32(count):D4}";
        }

        // ---- پارامترهای SQL فاکتور رو اضافه میکنه ----
        // این متد خصوصیه، فقط داخل همین کلاس استفاده میشه
        private void AddParams(SqlCommand cmd, Invoice inv)
        {
            cmd.Parameters.AddWithValue("@Id", inv.Id);
            cmd.Parameters.AddWithValue("@No", inv.InvoiceNo ?? "");
            cmd.Parameters.AddWithValue("@Type", (int)inv.Type);          // نوع فاکتور به عدد
            cmd.Parameters.AddWithValue("@CustId", inv.CustomerId.HasValue ? (object)inv.CustomerId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Date", inv.Date);
            cmd.Parameters.AddWithValue("@Total", inv.TotalAmount);       // جمع کل
            cmd.Parameters.AddWithValue("@Disc", inv.Discount);           // تخفیف
            cmd.Parameters.AddWithValue("@Tax", inv.Tax);                 // مالیات
            cmd.Parameters.AddWithValue("@Final", inv.FinalAmount);       // مبلغ نهایی
            cmd.Parameters.AddWithValue("@Paid", inv.PaidAmount);         // پرداخت شده
            cmd.Parameters.AddWithValue("@PayType", (int)inv.PaymentType);// نوع پرداخت به عدد
            cmd.Parameters.AddWithValue("@Desc", inv.Description ?? "");
        }

        // ---- یه ردیف دیتابیس رو به سرفاکتور تبدیل میکنه ----
        // "سرفاکتور" یعنی اطلاعات کلی فاکتور (بدون ردیف‌های کالا)
        private Invoice MapHeader(System.Data.DataRow r) => new Invoice
        {
            Id = Convert.ToInt32(r["Id"]),
            InvoiceNo = r["InvoiceNo"].ToString(),
            Type = (InvoiceType)Convert.ToInt32(r["Type"]),   // عدد رو به نوع فاکتور تبدیل کن
            // اگه مشتری نداشت (نقدی بود)، null بذار
            CustomerId = r["CustomerId"] == DBNull.Value ? null : Convert.ToInt32(r["CustomerId"]),
            CustomerName = r["CustomerName"].ToString(),
            Date = Convert.ToDateTime(r["Date"]),
            TotalAmount = Convert.ToDecimal(r["TotalAmount"]),
            Discount = Convert.ToDecimal(r["Discount"]),
            Tax = Convert.ToDecimal(r["Tax"]),
            FinalAmount = Convert.ToDecimal(r["FinalAmount"]),
            PaidAmount = Convert.ToDecimal(r["PaidAmount"]),
            PaymentType = (PaymentType)Convert.ToInt32(r["PaymentType"]),
            Description = r["Description"].ToString()
        };
    }
}
