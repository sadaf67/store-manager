// ================================================
// این فایل "پنجره اضافه/ویرایش کالا" ماست
// یه پنجره کوچیک که وقتی میخوایم:
//   - کالای جدید اضافه کنیم
//   - یا کالای قدیمی رو ویرایش کنیم
// باز میشه
// ================================================

using System;
using System.Collections.Generic;
using System.Windows;
using StoreManager.DAL;
using StoreManager.DAL.Repositories;
using StoreManager.Models;

namespace StoreManager.UI.Views
{
    // پنجره مستقل برای ورود اطلاعات کالا
    public partial class ProductDialog : Window
    {
        // کالایی که داریم ویرایش میکنیم (یا کالای خالی برای کالای جدید)
        private readonly Product _product;

        // ابزار ذخیره کالا در دیتابیس
        private readonly ProductRepository _repo = new();

        // ---- وقتی پنجره باز میشه ----
        // product = کالایی که میخوایم ویرایش کنیم (یا خالی اگه جدیده)
        public ProductDialog(Product product)
        {
            InitializeComponent(); // کنترل‌های XAML رو بارگذاری کن
            _product = product;
            LoadCategories(); // دسته‌بندی‌ها رو توی ComboBox بریز
            Populate();       // اطلاعات کالا رو توی فیلدها بریز (اگه داریم ویرایش میکنیم)
        }

        // ---- دسته‌بندی‌ها رو از دیتابیس میخونه و توی ComboBox میذاره ----
        private void LoadCategories()
        {
            // اول "بدون دسته" رو اضافه کن (برای وقتی که دسته‌بندی ندارن)
            var cats = new List<Category> { new Category { Id = 0, Name = "-- بدون دسته --" } };
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Categories WHERE IsActive=1 ORDER BY Name");
            foreach (System.Data.DataRow r in dt.Rows)
                cats.Add(new Category { Id = Convert.ToInt32(r["Id"]), Name = r["Name"].ToString() });
            cbCategory.ItemsSource = cats;
        }

        // ---- اطلاعات کالا رو توی فیلدهای فرم میریزه ----
        // برای وقتی که داریم کالای قدیمی رو ویرایش میکنیم
        // اگه کالا جدیده، همه فیلدها خالی میمونن
        private void Populate()
        {
            txtCode.Text = _product.Code;          // کد کالا
            txtName.Text = _product.Name;          // اسم کالا
            cbCategory.SelectedValue = _product.CategoryId; // دسته‌بندی
            // اگه واحد نداشت، "عدد" بذار
            cbUnit.Text = string.IsNullOrEmpty(_product.Unit) ? "عدد" : _product.Unit;
            txtBuyPrice.Text = _product.BuyPrice.ToString();    // قیمت خرید
            txtSellPrice.Text = _product.SellPrice.ToString();  // قیمت فروش
            txtStock.Text = _product.Stock.ToString();          // موجودی فعلی
            txtMinStock.Text = _product.MinStock.ToString();    // حداقل موجودی
        }

        // ---- دکمه "ذخیره" ----
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // اول چک کن که اسم کالا وارد شده باشه (الزامیه)
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("نام کالا الزامی است.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // برو بیرون، ذخیره نکن
            }

            // مقادیر فرم رو توی object کالا بریز
            _product.Code = txtCode.Text.Trim();
            _product.Name = txtName.Text.Trim();
            _product.CategoryId = cbCategory.SelectedValue != null ? (int)cbCategory.SelectedValue : 0;
            _product.Unit = cbUnit.Text;

            // اعداد رو parse کن (از متن به عدد تبدیل کن)
            // TryParse = اگه عدد نبود، 0 بذار (خراب نشه)
            decimal.TryParse(txtBuyPrice.Text, out decimal bp);  _product.BuyPrice = bp;
            decimal.TryParse(txtSellPrice.Text, out decimal sp); _product.SellPrice = sp;
            decimal.TryParse(txtStock.Text, out decimal st);     _product.Stock = st;
            decimal.TryParse(txtMinStock.Text, out decimal ms);  _product.MinStock = ms;

            try
            {
                _repo.Save(_product);   // توی دیتابیس ذخیره کن
                DialogResult = true;    // به صفحه قبل بگو "کاربر ذخیره زد"
            }
            catch (Exception ex)
            {
                // اگه خطا خورد (مثلاً کد تکراریه)، خطا رو نشون بده
                MessageBox.Show("خطا: " + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---- دکمه "انصراف" - پنجره رو ببند بدون ذخیره ----
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
