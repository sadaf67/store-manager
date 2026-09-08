// ================================================
// این فایل "انبار اطلاعات کالاها" ماست
// هر کاری که با جدول کالاها توی دیتابیس داریم
// از اینجا انجام میدیم
// مثلاً: لیست گرفتن، اضافه کردن، ویرایش، حذف
// ================================================

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using StoreManager.Models;

namespace StoreManager.DAL.Repositories
{
    // این کلاس مسئول همه کارهای مربوط به کالاهاست
    public class ProductRepository
    {
        // ---- لیست همه کالاها رو میده ----
        // search = اگه کاربر چیزی تایپ کرد، فقط اون‌ها رو نشون بده
        // categoryId = اگه یه دسته‌بندی انتخاب شد، فقط اون دسته رو بیار
        public List<Product> GetAll(string search = "", int categoryId = 0)
        {
            var list = new List<Product>(); // لیست خالی آماده میکنیم

            // این SQL میگه: همه کالاهای فعال رو بیار، باهاشون اسم دسته‌بندی‌شون رو هم بیار
            // اگه جستجو داشتیم، فقط اون‌هایی که اسم یا کدشون شبیه متن جستجوه رو بیار
            string sql = @"SELECT p.*, c.Name AS CategoryName FROM Products p
                          LEFT JOIN Categories c ON p.CategoryId=c.Id
                          WHERE p.IsActive=1
                          AND (@search='' OR p.Name LIKE @search OR p.Code LIKE @search)
                          AND (@cat=0 OR p.CategoryId=@cat)
                          ORDER BY p.Name";

            // اجرای کوئری با مقادیر جستجو و دسته‌بندی
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] {
                new SqlParameter("@search", $"%{search}%"),  // % یعنی هر چیزی قبل و بعد از متن
                new SqlParameter("@cat", categoryId)
            });

            // هر ردیف از نتیجه رو به یه کالا تبدیل میکنیم و به لیست اضافه میکنیم
            foreach (System.Data.DataRow row in dt.Rows)
                list.Add(Map(row));

            return list; // لیست پر شده رو برمیگردونیم
        }

        // ---- یه کالای خاص رو با شماره‌اش پیدا میکنه ----
        // id = شماره یونیک اون کالا توی دیتابیس
        public Product GetById(int id)
        {
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT p.*, c.Name AS CategoryName FROM Products p LEFT JOIN Categories c ON p.CategoryId=c.Id WHERE p.Id=@id",
                new[] { new SqlParameter("@id", id) });

            // اگه پیدا کرد برش گردون، اگه نه null بده
            return dt.Rows.Count > 0 ? Map(dt.Rows[0]) : null;
        }

        // ---- کالا رو ذخیره میکنه (هم اضافه کردن، هم ویرایش) ----
        // اگه Id کالا صفر بود = کالای جدیده، باید INSERT کنیم
        // اگه Id داشت = کالای قدیمیه، باید UPDATE کنیم
        public int Save(Product p)
        {
            if (p.Id == 0)
            {
                // ---- کالای جدید: توی دیتابیس اضافه میکنیم ----
                // SCOPE_IDENTITY() = شماره‌ای که سیستم به رکورد جدید داد رو میده
                var id = DatabaseHelper.ExecuteScalar(@"
                    INSERT INTO Products(Code,Name,CategoryId,Unit,BuyPrice,SellPrice,Stock,MinStock,Description,IsActive)
                    VALUES(@Code,@Name,@CategoryId,@Unit,@BuyPrice,@SellPrice,@Stock,@MinStock,@Description,1);
                    SELECT SCOPE_IDENTITY();", Params(p));
                return Convert.ToInt32(id); // شماره جدید رو برمیگردونیم
            }

            // ---- کالای قبلی: آپدیتش میکنیم ----
            // توجه: موجودی (Stock) اینجا آپدیت نمیشه، چون از طریق فاکتور تغییر میکنه
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE Products SET Code=@Code,Name=@Name,CategoryId=@CategoryId,Unit=@Unit,
                BuyPrice=@BuyPrice,SellPrice=@SellPrice,MinStock=@MinStock,Description=@Description
                WHERE Id=@Id", Params(p));
            return p.Id;
        }

        // ---- کالا رو "غیرفعال" میکنه (حذف نمیکنیم، فقط مخفی میکنیم) ----
        // چرا؟ چون اگه این کالا توی فاکتورهای قدیمی باشه نباید گم بشه
        public void Delete(int id) =>
            DatabaseHelper.ExecuteNonQuery("UPDATE Products SET IsActive=0 WHERE Id=@id",
                new[] { new SqlParameter("@id", id) });

        // ---- کالاهایی که موجودیشون کمه رو برمیگردونه (برای هشدار) ----
        // اگه Stock <= MinStock باشه یعنی باید سفارش بدیم
        public List<Product> GetLowStock() =>
            GetAll().FindAll(p => p.Stock <= p.MinStock && p.MinStock > 0);

        // ---- آماده کردن پارامترهای SQL برای Insert و Update ----
        // این متد خصوصیه، فقط داخل همین کلاس استفاده میشه
        // کار اصلیش اینه که مقادیر کالا رو به پارامتر SQL تبدیل کنه
        private SqlParameter[] Params(Product p) => new[] {
            new SqlParameter("@Id", p.Id),
            new SqlParameter("@Code", p.Code ?? ""),           // اگه کد نداشت، خالی بذار
            new SqlParameter("@Name", p.Name),
            new SqlParameter("@CategoryId", p.CategoryId == 0 ? (object)DBNull.Value : p.CategoryId), // اگه دسته نداشت، null بذار
            new SqlParameter("@Unit", p.Unit ?? "عدد"),        // اگه واحد نداشت، "عدد" بذار
            new SqlParameter("@BuyPrice", p.BuyPrice),
            new SqlParameter("@SellPrice", p.SellPrice),
            new SqlParameter("@Stock", p.Stock),
            new SqlParameter("@MinStock", p.MinStock),
            new SqlParameter("@Description", p.Description ?? "")
        };

        // ---- یه ردیف از دیتابیس رو به کلاس Product تبدیل میکنه ----
        // DataRow = یه ردیف از جدول دیتابیس (مثل یه ردیف اکسل)
        // این متد هر ستون رو به Property مربوطه نسبت میده
        private Product Map(System.Data.DataRow r) => new Product
        {
            Id = Convert.ToInt32(r["Id"]),
            Code = r["Code"].ToString(),
            Name = r["Name"].ToString(),
            // اگه دسته‌بندی نداشت (NULL بود)، صفر بذار
            CategoryId = r["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(r["CategoryId"]),
            CategoryName = r["CategoryName"].ToString(),
            Unit = r["Unit"].ToString(),
            BuyPrice = Convert.ToDecimal(r["BuyPrice"]),
            SellPrice = Convert.ToDecimal(r["SellPrice"]),
            Stock = Convert.ToDecimal(r["Stock"]),
            MinStock = Convert.ToDecimal(r["MinStock"]),
            Description = r["Description"].ToString(),
            IsActive = Convert.ToBoolean(r["IsActive"])
        };
    }
}
