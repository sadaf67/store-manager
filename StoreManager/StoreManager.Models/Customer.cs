// -----------------------------------------------
// این فایل قالب اطلاعات مشتریه
// هر مشتری که توی سیستم ثبت میکنیم
// اطلاعاتش اینجا نگه داشته میشه
// -----------------------------------------------

using System;

namespace StoreManager.Models
{
    // اطلاعات کامل یه مشتری
    public class Customer
    {
        // شماره یونیک مشتری - خود سیستم میده
        public int Id { get; set; }

        // کد مشتری - مثلاً "C001" که خودمون بهش میدیم
        public string Code { get; set; }

        // اسم کامل مشتری
        public string Name { get; set; }

        // شماره تلفن ثابت
        public string Phone { get; set; }

        // شماره موبایل
        public string Mobile { get; set; }

        // آدرس کامل مشتری
        public string Address { get; set; }

        // سقف اعتبار - مشتری حداکثر تا این مبلغ میتونه نسیه بگیره
        // مثلاً 5,000,000 ریال
        public decimal CreditLimit { get; set; }

        // موجودی حساب مشتری
        // اگه منفی باشه یعنی مشتری بدهکاره (پول داره میگیره نمیده)
        // اگه مثبت باشه یعنی مشتری پیش‌پرداخت داده
        public decimal Balance { get; set; }

        // کد ملی مشتری - اختیاریه
        public string NationalCode { get; set; }

        // فعاله یا نه؟
        // مشتری رو حذف نمیکنیم، فقط غیرفعالش میکنیم
        public bool IsActive { get; set; } = true;

        // تاریخی که این مشتری رو توی سیستم ثبت کردیم
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
