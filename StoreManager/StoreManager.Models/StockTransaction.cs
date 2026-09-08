// -----------------------------------------------
// این فایل قالب تراکنش انباریه
// هر بار که کالایی وارد یا خارج از انبار بشه
// یه رکورد اینجا ثبت میشه
// -----------------------------------------------

using System;

namespace StoreManager.Models
{
    // نوع تراکنش انبار
    // Purchase = کالا خریدیم و اومد تو انبار (موجودی زیاد شد)
    // Sale = کالا فروختیم و رفت از انبار (موجودی کم شد)
    // Adjustment = تنظیم دستی موجودی (مثلاً بعد از انبارگردانی)
    // Return = مشتری کالا برگردوند (موجودی زیاد شد)
    public enum TransactionType { Purchase, Sale, Adjustment, Return }

    // اطلاعات هر تراکنش انبار
    public class StockTransaction
    {
        // شماره یونیک این تراکنش
        public int Id { get; set; }

        // این تراکنش برای کدوم کالاست؟
        public int ProductId { get; set; }

        // اسم کالا - از جدول کالا میاریم برای نمایش
        public string ProductName { get; set; }

        // نوع تراکنش (ورود/خروج/تنظیم/برگشت)
        public TransactionType Type { get; set; }

        // چه مقداری جابجا شد؟
        // مثلاً 10 کیلو یا 50 عدد
        public decimal Qty { get; set; }

        // قیمت هر واحد - برای محاسبه ارزش کالای جابجا شده
        public decimal UnitPrice { get; set; }

        // این تراکنش مربوط به کدوم فاکتوره؟ (اختیاری)
        // اگه فروش یا خرید باشه شماره فاکتور اینجاست
        public int? InvoiceId { get; set; }

        // تاریخ و ساعت این تراکنش
        public DateTime Date { get; set; } = DateTime.Now;

        // توضیحات - مثلاً "انبارگردانی ماهانه" یا "کسری انبار"
        public string Description { get; set; }
    }
}
