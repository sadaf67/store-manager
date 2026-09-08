// ================================================
// این فایل "مغز صفحه مشتریان" ماست
// کاربر از این صفحه میتونه:
//   - لیست همه مشتریان رو ببینه
//   - مشتری جستجو کنه
//   - فقط بدهکاران رو ببینه (دکمه مجزا)
//   - مشتری جدید اضافه کنه
//   - مشتری ویرایش کنه (دابل‌کلیک)
//   - مشتری حذف کنه
// ================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StoreManager.DAL.Repositories;
using StoreManager.Models;

namespace StoreManager.UI.Views
{
    // صفحه مدیریت مشتریان
    public partial class CustomersPage : Page
    {
        // ابزار کار با دیتابیس مشتریان
        private readonly CustomerRepository _repo = new();

        // آیا الان فقط بدهکارها نشون داده میشن؟
        private bool _debtorsOnly = false;

        // ---- وقتی صفحه باز میشه ----
        public CustomersPage()
        {
            InitializeComponent();
            LoadCustomers(); // لیست مشتریان رو نشون بده
        }

        // ---- لیست مشتریان رو (با توجه به فیلتر) نشون میده ----
        private void LoadCustomers()
        {
            // اگه "فقط بدهکارها" فعاله، اونا رو بده - وگرنه همه رو بده
            var list = _debtorsOnly ? _repo.GetDebtors() : _repo.GetAll(txtSearch.Text);
            dgCustomers.ItemsSource = list;
        }

        // ---- وقتی کاربر توی جستجو تایپ میکنه ----
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _debtorsOnly = false; // فیلتر بدهکارها رو خاموش کن
            LoadCustomers();
        }

        // ---- دکمه "فقط بدهکارها" ----
        private void BtnDebtors_Click(object sender, RoutedEventArgs e)
        {
            _debtorsOnly = true; // فیلتر بدهکارها رو روشن کن
            LoadCustomers();
        }

        // ---- دکمه "مشتری جدید" ----
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // یه پنجره کوچیک برای ورود اطلاعات مشتری باز کن
            // new Customer() = مشتری خالی برای ثبت جدید
            var dlg = new CustomerDialog(new Customer()) { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true) LoadCustomers(); // اگه ذخیره شد، رفرش کن
        }

        // ---- دابل‌کلیک روی مشتری = ویرایش ----
        private void DgCustomers_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer c)
            {
                // پنجره ویرایش با اطلاعات مشتری انتخاب شده باز کن
                var dlg = new CustomerDialog(c) { Owner = Window.GetWindow(this) };
                if (dlg.ShowDialog() == true) LoadCustomers();
            }
        }

        // ---- دکمه حذف مشتری ----
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // اگه مشتری انتخاب شده و کاربر تایید کرد، غیرفعال کن
            if (dgCustomers.SelectedItem is Customer c &&
                MessageBox.Show($"مشتری «{c.Name}» حذف شود؟", "حذف", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _repo.Delete(c.Id); // مشتری رو غیرفعال کن (نه حذف واقعی)
                LoadCustomers();    // لیست رو رفرش کن
            }
        }
    }
}
