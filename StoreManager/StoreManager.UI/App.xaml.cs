// ================================================
// این فایل "نقطه شروع برنامه" ماست
// وقتی کاربر برنامه رو باز میکنه، اول این کد اجرا میشه
// کارش اینه که:
//   ۱. آدرس دیتابیس رو از فایل تنظیمات بخونه
//   ۲. دیتابیس رو بسازه اگه وجود نداشت
//   ۳. جداول رو بسازه اگه وجود نداشتن
// بعد از این‌ها، برنامه به حالت عادی باز میشه
// ================================================

using System.Configuration;
using System.Windows;
using StoreManager.DAL;

namespace StoreManager.UI
{
    // این کلاس "مدیر کل برنامه" ماست - از Application ارث میبره
    // Application = کلاس اصلی WPF که کل برنامه رو مدیریت میکنه
    public partial class App : Application
    {
        // ---- وقتی برنامه داره استارت میشه، این متد صدا زده میشه ----
        // قبل از اینکه پنجره اصلی باز بشه، این کدها اجرا میشن
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e); // اول کار پیش‌فرض WPF رو انجام بده

            // ---- گام ۱: آدرس دیتابیس رو بخون ----
            // این آدرس توی App.config ذخیره شده (مثل آدرس خونه برای SQL Server)
            // "StoreDb" اسمیه که توی App.config بهش دادیم
            DatabaseHelper.ConnectionString = ConfigurationManager.ConnectionStrings["StoreDb"].ConnectionString;

            // ---- گام ۲: دیتابیس و جداول رو آماده کن ----
            // این متد اول دیتابیس رو میسازه اگه نبود،
            // بعد همه جداول رو میسازه اگه نبودن
            // اگه همه چیز از قبل بود، کاری نمیکنه (IF NOT EXISTS)
            DatabaseHelper.InitializeDatabase();
        }
    }
}
