// ================================================
// این فایل "مغز صفحه گزارش‌ها" ماست
// کاربر از این صفحه میتونه انواع گزارش ببینه:
//   - فروش به تفکیک کالا
//   - سود و زیان
//   - بدهی مشتریان
//   - کالاهای کم‌موجود
// همچنین میتونه نتیجه رو به فایل Excel (CSV) ذخیره کنه
// ================================================

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using StoreManager.BLL.Services;

namespace StoreManager.UI.Views
{
    // صفحه گزارش‌ها
    public partial class ReportsPage : Page
    {
        // ابزار گرفتن گزارش‌ها از دیتابیس
        private readonly ReportService _svc = new();

        // آخرین گزارشی که نشون داده شده (برای خروجی CSV)
        private DataTable _currentData;

        // ---- وقتی صفحه باز میشه ----
        public ReportsPage()
        {
            InitializeComponent();

            // تاریخ پیش‌فرض: 30 روز گذشته تا امروز
            dpFrom.SelectedDate = DateTime.Today.AddDays(-30);
            dpTo.SelectedDate = DateTime.Today;
        }

        // ---- وقتی نوع گزارش عوض میشه، گزارش جدید نشون بده ----
        private void CbReport_Changed(object sender, SelectionChangedEventArgs e) => LoadReport();

        // ---- دکمه "نمایش گزارش" ----
        private void BtnShow_Click(object sender, RoutedEventArgs e) => LoadReport();

        // ---- گزارش مناسب رو از دیتابیس میگیره و نشون میده ----
        private void LoadReport()
        {
            DateTime from = dpFrom.SelectedDate ?? DateTime.Today.AddDays(-30);
            DateTime to = dpTo.SelectedDate ?? DateTime.Today;

            // بسته به اینکه کاربر کدوم گزارش رو انتخاب کرده، داده‌های متفاوت بیار
            // switch expression = یه if/else کوتاه‌نویسی
            _currentData = cbReport.SelectedIndex switch
            {
                0 => _svc.GetSalesByProduct(from, to),  // گزارش فروش به تفکیک کالا
                1 => _svc.GetProfitReport(from, to),    // گزارش سود و زیان
                2 => _svc.GetCustomerDebts(),           // بدهی مشتریان (تاریخ نداره)
                3 => _svc.GetLowStockProducts(),        // کالاهای کم‌موجود (تاریخ نداره)
                _ => null                               // هیچکدام = نشون نده
            };

            // داده‌ها رو توی جدول نشون بده
            // DefaultView = DataTable رو به فرمتی که DataGrid میفهمه تبدیل میکنه
            dgReport.ItemsSource = _currentData?.DefaultView ?? null;
        }

        // ---- دکمه "خروجی Excel" ----
        // گزارش رو به فرمت CSV ذخیره میکنه که Excel میتونه بازش کنه
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            // اگه داده‌ای نداریم، پیام بده
            if (_currentData == null || _currentData.Rows.Count == 0)
            {
                MessageBox.Show("داده‌ای برای خروجی وجود ندارد.", "خروجی");
                return;
            }

            // پنجره ذخیره فایل رو نشون بده
            var dlg = new SaveFileDialog {
                Filter = "CSV File|*.csv",    // فقط فایل CSV
                FileName = "گزارش.csv"        // اسم پیش‌فرض
            };
            if (dlg.ShowDialog() != true) return; // اگه انصراف داد، برگرد

            // ---- ساخت فایل CSV ----
            var sb = new StringBuilder(); // یه StringBuilder برای ساخت متن

            // ردیف اول = عنوان ستون‌ها
            foreach (DataColumn col in _currentData.Columns)
                sb.Append(col.ColumnName + ","); // ستون‌ها رو با کاما جدا کن
            sb.AppendLine(); // رفتن به خط بعد

            // بقیه ردیف‌ها = داده‌ها
            foreach (DataRow row in _currentData.Rows)
            {
                foreach (var item in row.ItemArray)
                    sb.Append($"\"{item}\","); // مقدار رو توی "" بذار (برای کاماهای داخل متن)
                sb.AppendLine();
            }

            // فایل رو ذخیره کن (با کدگذاری UTF-8 برای فارسی)
            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("فایل CSV ذخیره شد.", "موفق");
        }
    }
}
