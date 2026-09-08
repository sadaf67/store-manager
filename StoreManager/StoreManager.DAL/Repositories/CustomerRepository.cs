// ================================================
// این فایل "انبار اطلاعات مشتری‌هاست"
// همه کارهایی که با مشتری‌ها داریم (لیست، ثبت، ویرایش، حذف)
// از اینجا انجام میشه
// ================================================

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using StoreManager.Models;

namespace StoreManager.DAL.Repositories
{
    // این کلاس مسئول همه کارهای مربوط به مشتری‌هاست
    public class CustomerRepository
    {
        // ---- لیست همه مشتری‌های فعال رو میده ----
        // search = اگه کاربر اسم یا موبایل رو تایپ کرد، فقط اون‌ها رو نشون بده
        public List<Customer> GetAll(string search = "")
        {
            var list = new List<Customer>(); // لیست خالی آماده میکنیم

            // این SQL: همه مشتری‌های فعال رو بیار، بر اساس نام مرتب کن
            // اگه جستجو داشتیم، فیلتر کن
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT * FROM Customers WHERE IsActive=1 AND (@s='' OR Name LIKE @s OR Mobile LIKE @s OR Code LIKE @s) ORDER BY Name",
                new[] { new SqlParameter("@s", $"%{search}%") }); // % = هر چیزی قبل و بعد

            // هر ردیف رو به مشتری تبدیل کن
            foreach (System.Data.DataRow row in dt.Rows)
                list.Add(Map(row));

            return list;
        }

        // ---- یه مشتری خاص رو با شماره‌اش پیدا میکنه ----
        public Customer GetById(int id)
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Customers WHERE Id=@id",
                new[] { new SqlParameter("@id", id) });
            return dt.Rows.Count > 0 ? Map(dt.Rows[0]) : null;
        }

        // ---- مشتری رو ذخیره میکنه (هم اضافه کردن جدید، هم ویرایش قدیمی) ----
        public int Save(Customer c)
        {
            if (c.Id == 0)
            {
                // ---- مشتری جدید: اضافه میکنیم ----
                var id = DatabaseHelper.ExecuteScalar(@"
                    INSERT INTO Customers(Code,Name,Phone,Mobile,Address,CreditLimit,Balance,NationalCode)
                    VALUES(@Code,@Name,@Phone,@Mobile,@Address,@CreditLimit,@Balance,@NationalCode);
                    SELECT SCOPE_IDENTITY();", Params(c));
                return Convert.ToInt32(id);
            }

            // ---- مشتری قدیمی: اطلاعاتش رو آپدیت میکنیم ----
            // توجه: Balance (موجودی/بدهی) اینجا آپدیت نمیشه، از طریق فاکتور تغییر میکنه
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE Customers SET Code=@Code,Name=@Name,Phone=@Phone,Mobile=@Mobile,
                Address=@Address,CreditLimit=@CreditLimit,NationalCode=@NationalCode WHERE Id=@Id", Params(c));
            return c.Id;
        }

        // ---- موجودی حساب مشتری رو آپدیت میکنه ----
        // amount = مقداری که باید به موجودیش اضافه بشه
        // اگه منفی باشه یعنی بدهکار شد (فروش نسیه)
        // اگه مثبت باشه یعنی پول داد یا کالا برگردوند
        public void UpdateBalance(int customerId, decimal amount) =>
            DatabaseHelper.ExecuteNonQuery("UPDATE Customers SET Balance=Balance+@amt WHERE Id=@id",
                new[] { new SqlParameter("@amt", amount), new SqlParameter("@id", customerId) });

        // ---- مشتری رو غیرفعال میکنه (حذف واقعی نمیکنیم) ----
        // چون این مشتری ممکنه توی فاکتورهای قدیمی باشه
        public void Delete(int id) =>
            DatabaseHelper.ExecuteNonQuery("UPDATE Customers SET IsActive=0 WHERE Id=@id",
                new[] { new SqlParameter("@id", id) });

        // ---- فقط بدهکارها رو برمیگردونه ----
        // Balance منفی = مشتری بدهکاره (نسیه گرفته و نداده)
        public List<Customer> GetDebtors() =>
            GetAll().FindAll(c => c.Balance < 0);

        // ---- آماده کردن پارامترهای SQL برای Insert و Update ----
        private SqlParameter[] Params(Customer c) => new[] {
            new SqlParameter("@Id", c.Id),
            new SqlParameter("@Code", c.Code ?? ""),
            new SqlParameter("@Name", c.Name),
            new SqlParameter("@Phone", c.Phone ?? ""),
            new SqlParameter("@Mobile", c.Mobile ?? ""),
            new SqlParameter("@Address", c.Address ?? ""),
            new SqlParameter("@CreditLimit", c.CreditLimit),  // سقف نسیه
            new SqlParameter("@Balance", c.Balance),          // موجودی/بدهی الان
            new SqlParameter("@NationalCode", c.NationalCode ?? "")
        };

        // ---- یه ردیف دیتابیس رو به کلاس Customer تبدیل میکنه ----
        private Customer Map(System.Data.DataRow r) => new Customer
        {
            Id = Convert.ToInt32(r["Id"]),
            Code = r["Code"].ToString(),
            Name = r["Name"].ToString(),
            Phone = r["Phone"].ToString(),
            Mobile = r["Mobile"].ToString(),
            Address = r["Address"].ToString(),
            CreditLimit = Convert.ToDecimal(r["CreditLimit"]),
            Balance = Convert.ToDecimal(r["Balance"]),
            NationalCode = r["NationalCode"].ToString(),
            IsActive = Convert.ToBoolean(r["IsActive"])
        };
    }
}
