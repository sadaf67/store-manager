// -----------------------------------------------
// این فایل قالب تنظیمات فروشگاهه
// اطلاعات فروشگاه مثل اسم، آدرس، نرخ مالیات و...
// -----------------------------------------------

namespace StoreManager.Models
{
    // تنظیمات کلی برنامه که مدیر تنظیم میکنه
    public class AppSettings
    {
        // اسم فروشگاه - روی فاکتور و رسید چاپ میشه
        public string StoreName { get; set; }

        // نوع صنف - مثلاً "سوپرمارکت" یا "داروخانه"
        // برای نمایش زیر اسم فروشگاه
        public string StoreType { get; set; }

        // اسم صاحب کسب‌وکار
        public string OwnerName { get; set; }

        // شماره تلفن فروشگاه
        public string Phone { get; set; }

        // آدرس فروشگاه - روی فاکتور چاپ میشه
        public string Address { get; set; }

        // شماره مالیاتی فروشگاه (اختیاری)
        public string TaxNumber { get; set; }

        // درصد مالیات - مثلاً 9 یعنی 9 درصد مالیات
        public decimal TaxRate { get; set; }

        // واحد پول - پیش‌فرض "ریال"
        // میشه تغییر داد به "تومان" یا هر چیز دیگه‌ای
        public string Currency { get; set; } = "ریال";

        // متنی که پایین فاکتور چاپ میشه
        // مثلاً "با تشکر از خرید شما - ساعت کاری 8 تا 21"
        public string ReceiptFooter { get; set; }

        // آیا بعد از ثبت فاکتور خودکار چاپ بشه؟
        // true = بله، false = خیر
        public bool PrintAfterSale { get; set; }

        // آیا وقتی کالا کم شد هشدار نشون بده؟
        // true = بله (پیش‌فرض)
        public bool ShowLowStockAlert { get; set; } = true;
    }
}
