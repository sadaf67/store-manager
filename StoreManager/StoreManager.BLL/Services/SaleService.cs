// ================================================
// این فایل "مغز ثبت فاکتور" ماست
// وقتی کاربر دکمه "ثبت فاکتور" رو میزنه، این کلاس کار میکنه
// کارش اینه که همه چیز رو درست و یه‌جا ذخیره کنه:
//   1. خود فاکتور رو ذخیره کنه
//   2. ردیف‌های کالا رو ذخیره کنه
//   3. موجودی انبار رو آپدیت کنه
//   4. حساب مشتری رو آپدیت کنه
// اگه هر کدوم خراب شد، همه چیز برمیگرده (تراکنش اتمیک)
// ================================================

using System;
using Microsoft.Data.SqlClient;
using StoreManager.DAL;
using StoreManager.DAL.Repositories;
using StoreManager.Models;

namespace StoreManager.BLL.Services
{
    // این کلاس مسئول ثبت فاکتورهاست (فروش و خرید)
    public class SaleService
    {
        // ابزارهایی که بهشون نیاز داریم
        private readonly InvoiceRepository _invoiceRepo = new();    // برای کار با فاکتورها
        private readonly CustomerRepository _customerRepo = new();  // برای کار با مشتری‌ها

        // ---- ثبت فاکتور جدید ----
        // invoice = تمام اطلاعات فاکتور که کاربر وارد کرده
        // برمیگردونه: شماره (Id) فاکتوری که ثبت شد
        public int SaveSale(Invoice invoice)
        {
            // ---- گام ۱: شماره فاکتور رو بساز (مثلاً S202406001) ----
            invoice.InvoiceNo = _invoiceRepo.GetNextInvoiceNo(invoice.Type);

            // ---- گام ۲: جمع کل فاکتور رو حساب کن ----
            invoice.TotalAmount = 0;
            foreach (var item in invoice.Items)
                invoice.TotalAmount += item.TotalPrice; // TotalPrice = تعداد × قیمت - تخفیف ردیف

            // مبلغ نهایی = جمع کل - تخفیف کل + مالیات
            invoice.FinalAmount = invoice.TotalAmount - invoice.Discount + invoice.Tax;

            // ---- گام ۳: یه اتصال به دیتابیس باز کن ----
            using var con = DatabaseHelper.GetConnection();
            con.Open();

            // ---- گام ۴: یه "تراکنش" شروع کن ----
            // تراکنش = قرارداد با دیتابیس که میگه:
            // "همه این کارها باید با هم انجام بشن، اگه یکی خراب شد همه چیز رو برگردون"
            // مثل پرداخت آنلاین: پول از حساب تو برود و به حساب فروشگاه بیاد باید همزمان باشه
            using var tx = con.BeginTransaction();
            try
            {
                // ---- گام ۵: فاکتور اصلی رو ذخیره کن ----
                int invoiceId = _invoiceRepo.Save(invoice, con, tx);

                // ---- گام ۶: برای هر کالا توی فاکتور: ----
                foreach (var item in invoice.Items)
                {
                    // ۶-الف: ردیف کالا رو توی جدول InvoiceItems ذخیره کن
                    using var cmdItem = new SqlCommand(@"
                        INSERT INTO InvoiceItems(InvoiceId,ProductId,Qty,UnitPrice,Discount)
                        VALUES(@inv,@prod,@qty,@price,@disc)", con, tx);
                    cmdItem.Parameters.AddWithValue("@inv", invoiceId);
                    cmdItem.Parameters.AddWithValue("@prod", item.ProductId);
                    cmdItem.Parameters.AddWithValue("@qty", item.Qty);
                    cmdItem.Parameters.AddWithValue("@price", item.UnitPrice);
                    cmdItem.Parameters.AddWithValue("@disc", item.Discount);
                    cmdItem.ExecuteNonQuery();

                    // ۶-ب: موجودی انبار رو آپدیت کن
                    // اگه خرید بود: موجودی زیاد میشه (+)
                    // اگه فروش بود: موجودی کم میشه (-)
                    decimal stockDelta = invoice.Type == InvoiceType.Purchase ? item.Qty : -item.Qty;
                    using var cmdStock = new SqlCommand(
                        "UPDATE Products SET Stock=Stock+@d WHERE Id=@id", con, tx);
                    cmdStock.Parameters.AddWithValue("@d", stockDelta);
                    cmdStock.Parameters.AddWithValue("@id", item.ProductId);
                    cmdStock.ExecuteNonQuery();

                    // ۶-ج: یه رکورد توی تاریخچه انبار (StockTransactions) ثبت کن
                    // این جدول مثل دفتر انبار داره: هر بار کالا وارد یا خارج شد ثبت میشه
                    using var cmdTx = new SqlCommand(@"
                        INSERT INTO StockTransactions(ProductId,Type,Qty,UnitPrice,InvoiceId,Date)
                        VALUES(@prod,@type,@qty,@price,@inv,GETDATE())", con, tx);
                    cmdTx.Parameters.AddWithValue("@prod", item.ProductId);
                    cmdTx.Parameters.AddWithValue("@type", (int)(invoice.Type == InvoiceType.Purchase ? TransactionType.Purchase : TransactionType.Sale));
                    cmdTx.Parameters.AddWithValue("@qty", Math.Abs(stockDelta)); // تعداد همیشه مثبت
                    cmdTx.Parameters.AddWithValue("@price", item.UnitPrice);
                    cmdTx.Parameters.AddWithValue("@inv", invoiceId);
                    cmdTx.ExecuteNonQuery();
                }

                // ---- گام ۷: اگه مشتری داریم و مبلغی مانده، حسابش رو آپدیت کن ----
                // RemainAmount = مبلغی که مشتری هنوز نداده (بدهی)
                if (invoice.CustomerId.HasValue && invoice.RemainAmount != 0)
                {
                    // فروش نسیه = موجودی مشتری کم میشه (بدهکارتر میشه) → منفی
                    // خرید یا برگشت = موجودی مشتری زیاد میشه → مثبت
                    decimal balanceDelta = invoice.Type == InvoiceType.Sale ? -invoice.RemainAmount : invoice.RemainAmount;
                    using var cmdBal = new SqlCommand(
                        "UPDATE Customers SET Balance=Balance+@d WHERE Id=@id", con, tx);
                    cmdBal.Parameters.AddWithValue("@d", balanceDelta);
                    cmdBal.Parameters.AddWithValue("@id", invoice.CustomerId.Value);
                    cmdBal.ExecuteNonQuery();
                }

                // ---- گام ۸: همه چیز درست بود، تراکنش رو تایید کن ----
                // Commit = "باشه دیتابیس، همه این تغییرات رو قطعی کن"
                tx.Commit();
                return invoiceId; // شماره فاکتور رو به صفحه برمیگردونیم
            }
            catch
            {
                // اگه هر جایی خطا خورد، همه چیز رو برگردون به حالت قبل
                // Rollback = "دیتابیس، هیچ‌کدوم از این تغییرات رو اعمال نکن"
                tx.Rollback();
                throw; // خطا رو به صفحه منتقل کن تا کاربر ببینه
            }
        }
    }
}
