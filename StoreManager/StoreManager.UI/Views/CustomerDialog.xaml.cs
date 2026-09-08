// ================================================
// این فایل "پنجره اضافه/ویرایش مشتری" ماست
// یه پنجره کوچیک که وقتی میخوایم:
//   - مشتری جدید اضافه کنیم
//   - یا مشتری قدیمی رو ویرایش کنیم
// باز میشه
// ================================================

using System;
using System.Windows;
using StoreManager.DAL.Repositories;
using StoreManager.Models;

namespace StoreManager.UI.Views
{
    // پنجره مستقل برای ورود اطلاعات مشتری
    public partial class CustomerDialog : Window
    {
        // مشتری که داریم ویرایش میکنیم (یا مشتری خالی برای ثبت جدید)
        private readonly Customer _customer;

        // ابزار ذخیره مشتری در دیتابیس
        private readonly CustomerRepository _repo = new();

        // ---- وقتی پنجره باز میشه ----
        // customer = مشتری که میخوایم ویرایش کنیم (یا خالی اگه جدیده)
        public CustomerDialog(Customer customer)
        {
            InitializeComponent();
            _customer = customer;

            // اطلاعات مشتری رو توی فیلدهای فرم بریز
            txtCode.Text = customer.Code;           // کد مشتری
            txtName.Text = customer.Name;           // اسم
            txtMobile.Text = customer.Mobile;       // موبایل
            txtPhone.Text = customer.Phone;         // تلفن ثابت
            txtAddress.Text = customer.Address;     // آدرس
            txtCredit.Text = customer.CreditLimit.ToString(); // سقف نسیه
            txtNational.Text = customer.NationalCode; // کد ملی
        }

        // ---- دکمه "ذخیره" ----
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // اول چک کن که اسم مشتری وارد شده (الزامیه)
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("نام مشتری الزامی است.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // مقادیر فرم رو توی object مشتری بریز
            _customer.Code = txtCode.Text.Trim();
            _customer.Name = txtName.Text.Trim();
            _customer.Mobile = txtMobile.Text.Trim();
            _customer.Phone = txtPhone.Text.Trim();
            _customer.Address = txtAddress.Text.Trim();
            // سقف نسیه رو از متن به عدد تبدیل کن
            decimal.TryParse(txtCredit.Text, out decimal cred);
            _customer.CreditLimit = cred;
            _customer.NationalCode = txtNational.Text.Trim();

            try
            {
                _repo.Save(_customer); // توی دیتابیس ذخیره کن
                DialogResult = true;   // به صفحه قبل بگو "ذخیره شد"
            }
            catch (Exception ex)
            {
                // اگه خطا خورد (مثلاً کد تکراری)، خطا رو نشون بده
                MessageBox.Show("خطا: " + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---- دکمه "انصراف" - پنجره رو ببند بدون ذخیره ----
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
