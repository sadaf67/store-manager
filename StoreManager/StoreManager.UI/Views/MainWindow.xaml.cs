// ================================================
// این فایل "مغز پنجره اصلی" ماست
// پنجره اصلی = اون پنجره‌ای که وقتی برنامه باز میشه میبینیم
// توش یه نوار کناری (Sidebar) هست با دکمه‌های مختلف
// وقتی روی هر دکمه کلیک میکنیم، محتوای وسط صفحه عوض میشه
// ================================================

using System;
using System.Windows;
using System.Windows.Threading;
using StoreManager.DAL;

namespace StoreManager.UI.Views
{
    // MainWindow = کلاس پنجره اصلی برنامه
    // : Window یعنی از کلاس Window ارث میبره (WPF)
    public partial class MainWindow : Window
    {
        // DispatcherTimer = تایمر که هر چند ثانیه یه کار انجام میده
        // ما ازش برای نشون دادن ساعت و تاریخ استفاده میکنیم
        private readonly DispatcherTimer _timer = new();

        // ---- وقتی پنجره باز میشه این کد اجرا میشه ----
        public MainWindow()
        {
            InitializeComponent(); // کنترل‌های XAML رو بارگذاری کن

            // راست به چپ برای فارسی
            FlowDirection = System.Windows.FlowDirection.RightToLeft;

            LoadSettings();  // اسم و نوع فروشگاه رو از دیتابیس بخون و نشون بده

            // ---- تایمر رو تنظیم کن برای نشون دادن ساعت ----
            _timer.Interval = TimeSpan.FromSeconds(1); // هر ۱ ثانیه
            // هر بار که تایمر میزنه، ساعت رو آپدیت کن
            _timer.Tick += (s, e) => txtDateTime.Text = DateTime.Now.ToString("dddd، yyyy/MM/dd  HH:mm:ss");
            _timer.Start(); // تایمر رو شروع کن

            // صفحه اول = داشبورد (خلاصه آمار)
            Navigate(new DashboardPage(), "داشبورد");
        }

        // ---- اسم و نوع فروشگاه رو از دیتابیس میخونه ----
        // این اطلاعات توی نوار بالای برنامه نشون داده میشه
        private void LoadSettings()
        {
            var name = DatabaseHelper.ExecuteScalar("SELECT [Value] FROM AppSettings WHERE [Key]='StoreName'");
            var type = DatabaseHelper.ExecuteScalar("SELECT [Value] FROM AppSettings WHERE [Key]='StoreType'");
            txtStoreName.Text = name?.ToString() ?? "فروشگاه من"; // اگه توی دیتابیس نبود، پیش‌فرض بذار
            txtStoreType.Text = type?.ToString() ?? "";
        }

        // ---- صفحه رو تغییر میده ----
        // page = صفحه‌ای که میخوایم نشون بدیم (مثلاً DashboardPage)
        // title = عنوانی که بالای صفحه نشون داده میشه
        private void Navigate(System.Windows.Controls.Page page, string title)
        {
            MainFrame.Navigate(page); // صفحه رو توی قاب وسط نشون بده
            txtPageTitle.Text = title; // عنوان رو بذار
        }

        // ---- دکمه‌های نوار کناری ----
        // هر دکمه یه صفحه رو باز میکنه

        // دکمه داشبورد - خلاصه آمار فروشگاه
        private void BtnDashboard_Click(object sender, RoutedEventArgs e) =>
            Navigate(new DashboardPage(), "داشبورد");

        // دکمه فروش جدید - ثبت فاکتور فروش
        private void BtnNewSale_Click(object sender, RoutedEventArgs e) =>
            Navigate(new SalePage(), "فروش جدید");

        // دکمه فاکتورها - لیست همه فاکتورها
        private void BtnInvoices_Click(object sender, RoutedEventArgs e) =>
            Navigate(new InvoicesPage(), "فاکتورها");

        // دکمه کالا و موجودی - مدیریت انبار
        private void BtnProducts_Click(object sender, RoutedEventArgs e) =>
            Navigate(new ProductsPage(), "کالا و موجودی");

        // دکمه مشتریان - لیست و مدیریت مشتریان
        private void BtnCustomers_Click(object sender, RoutedEventArgs e) =>
            Navigate(new CustomersPage(), "مشتریان");

        // دکمه گزارش‌ها - انواع گزارش‌های فروش، سود، بدهی و...
        private void BtnReports_Click(object sender, RoutedEventArgs e) =>
            Navigate(new ReportsPage(), "گزارش‌ها");

        // دکمه تنظیمات - اطلاعات فروشگاه، مالیات، QR Code و...
        private void BtnSettings_Click(object sender, RoutedEventArgs e) =>
            Navigate(new SettingsPage(), "تنظیمات");
    }
}
