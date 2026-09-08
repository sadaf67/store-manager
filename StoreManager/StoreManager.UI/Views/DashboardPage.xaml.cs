// ================================================
// این فایل "مغز صفحه داشبورد" ماست
// داشبورد = صفحه اول که وقتی برنامه باز میشه میبینیم
// توش آمار مهم فروشگاه نشون داده میشه:
//   - فروش امروز
//   - فروش این ماه
//   - جمع بدهی مشتریان
//   - تعداد کالاها و مشتریان
//   - هشدار کالاهای کم‌موجود
// ================================================

using System.Windows.Controls;
using StoreManager.BLL.Services;

namespace StoreManager.UI.Views
{
    // DashboardPage = صفحه داشبورد
    // : Page یعنی این یه "صفحه" WPF هست (نه پنجره مستقل)
    // صفحه‌ها توی پنجره اصلی (MainWindow) نشون داده میشن
    public partial class DashboardPage : Page
    {
        // ReportService = کلاسی که آمار و گزارش‌ها رو از دیتابیس میگیره
        private readonly ReportService _svc = new();

        // ---- وقتی صفحه باز میشه این کد اجرا میشه ----
        public DashboardPage()
        {
            InitializeComponent(); // کنترل‌های XAML رو بارگذاری کن
            LoadData();            // داده‌ها رو از دیتابیس بخون و نشون بده
        }

        // ---- آمار رو از دیتابیس میخونه و توی Label‌ها نشون میده ----
        private void LoadData()
        {
            // GetDashboardStats = یه دیکشنری از اعداد میده
            // مثل: {"TodaySales": 5000000, "ProductCount": 150, ...}
            var stats = _svc.GetDashboardStats();

            // هر عدد رو توی Label مربوطه بذار
            txtTodaySales.Text = stats["TodaySales"].ToString("N0") + " ریال";   // فروش امروز
            txtMonthSales.Text = stats["MonthSales"].ToString("N0") + " ریال";   // فروش این ماه
            txtTotalDebt.Text = stats["TotalDebt"].ToString("N0") + " ریال";     // جمع بدهی‌ها
            txtProductCount.Text = stats["ProductCount"].ToString("N0");          // تعداد کالاها
            txtCustomerCount.Text = stats["CustomerCount"].ToString("N0");        // تعداد مشتریان
            txtLowStock.Text = stats["LowStockCount"].ToString("N0");            // کالاهای کم‌موجود

            // ---- اگه کالای کم‌موجود داشتیم، جدول هشدار رو نشون بده ----
            if (stats["LowStockCount"] > 0)
            {
                // پنل هشدار رو نشون بده (پیش‌فرض مخفیه)
                pnlLowStock.Visibility = System.Windows.Visibility.Visible;

                // جدول کالاهای کم‌موجود رو پر کن
                // DefaultView = DataTable رو به فرمتی که DataGrid میفهمه تبدیل میکنه
                dgLowStock.ItemsSource = _svc.GetLowStockProducts().DefaultView;
            }
        }
    }
}
