// ================================================
// این فایل "مغز صفحه تنظیمات" ماست
// کاربر از این صفحه میتونه اطلاعات فروشگاه رو تنظیم کنه:
//   - اسم فروشگاه و نوع صنف
//   - اسم صاحب، تلفن، آدرس
//   - شماره مالیاتی و نرخ مالیات
//   - متن پایین فاکتور
//   - تنظیمات چاپ و هشدار
//   - نمایش QR Code اطلاعات تماس فروشگاه
// ================================================

using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using StoreManager.DAL;

namespace StoreManager.UI.Views
{
    // صفحه تنظیمات برنامه
    public partial class SettingsPage : Page
    {
        // ---- وقتی صفحه باز میشه ----
        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings(); // تنظیمات رو از دیتابیس بخون و نشون بده
        }

        // ---- تنظیمات رو از دیتابیس میخونه و توی فیلدها میذاره ----
        private void LoadSettings()
        {
            // همه تنظیمات رو یه‌جا بخون (جدول AppSettings)
            var dt = DatabaseHelper.ExecuteQuery("SELECT [Key],[Value] FROM AppSettings");

            // اونا رو توی یه دیکشنری بریز (مثل: {"StoreName": "فروشگاه علی", ...})
            var dict = new System.Collections.Generic.Dictionary<string, string>();
            foreach (System.Data.DataRow r in dt.Rows)
                dict[r["Key"].ToString()] = r["Value"].ToString();

            // هر مقدار رو توی فیلد مربوطه بذار
            txtStoreName.Text = dict.GetValueOrDefault("StoreName");  // اسم فروشگاه
            cbStoreType.Text  = dict.GetValueOrDefault("StoreType");  // نوع صنف
            txtOwner.Text     = dict.GetValueOrDefault("OwnerName");  // اسم صاحب
            txtPhone.Text     = dict.GetValueOrDefault("Phone");      // تلفن
            txtAddress.Text   = dict.GetValueOrDefault("Address");    // آدرس
            txtTaxNo.Text     = dict.GetValueOrDefault("TaxNumber");  // شماره مالیاتی
            txtTaxRate.Text   = dict.GetValueOrDefault("TaxRate");    // درصد مالیات
            txtFooter.Text    = dict.GetValueOrDefault("ReceiptFooter"); // متن پایین فاکتور
            // "1" = تیک خورده، هر چیز دیگه = تیک نخورده
            chkPrint.IsChecked    = dict.GetValueOrDefault("PrintAfterSale") == "1";
            chkLowStock.IsChecked = dict.GetValueOrDefault("ShowLowStockAlert") != "0";
        }

        // ---- دکمه "نمایش QR Code فروشگاه" ----
        // یه پنجره با QR Code اطلاعات تماس باز میکنه
        // مشتری میتونه این QR رو اسکن کنه و شماره فروشگاه رو ذخیره کنه
        private void BtnShowQR_Click(object sender, RoutedEventArgs e)
        {
            // پنجره QR Code با اطلاعات فروشگاه باز کن
            var qrWindow = new StoreQRWindow(
                storeName: txtStoreName.Text,
                phone: txtPhone.Text,
                address: txtAddress.Text
            )
            { Owner = Window.GetWindow(this) };

            qrWindow.ShowDialog(); // پنجره رو باز کن و صبر کن
        }

        // ---- دکمه "ذخیره تنظیمات" ----
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // یه تابع کمکی کوچیک برای آپدیت کردن هر تنظیم
            // key = اسم تنظیم، val = مقدار جدید
            void Set(string key, string val) => DatabaseHelper.ExecuteNonQuery(
                "UPDATE AppSettings SET [Value]=@v WHERE [Key]=@k",
                new[] { new SqlParameter("@v", val), new SqlParameter("@k", key) });

            // همه تنظیمات رو یکی یکی ذخیره کن
            Set("StoreName",       txtStoreName.Text);
            Set("StoreType",       cbStoreType.Text);
            Set("OwnerName",       txtOwner.Text);
            Set("Phone",           txtPhone.Text);
            Set("Address",         txtAddress.Text);
            Set("TaxNumber",       txtTaxNo.Text);
            Set("TaxRate",         txtTaxRate.Text);
            Set("ReceiptFooter",   txtFooter.Text);
            // برای Checkbox: تیک خورده = "1"، تیک نخورده = "0"
            Set("PrintAfterSale",     chkPrint.IsChecked == true ? "1" : "0");
            Set("ShowLowStockAlert",  chkLowStock.IsChecked == true ? "1" : "0");

            MessageBox.Show("تنظیمات ذخیره شد.", "موفق", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
