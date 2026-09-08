// ================================================
// این فایل "مغز صفحه کالا و موجودی" ماست
// کاربر از این صفحه میتونه:
//   - لیست همه کالاها رو ببینه
//   - کالا جستجو کنه
//   - بر اساس دسته‌بندی فیلتر کنه
//   - کالای جدید اضافه کنه
//   - کالا ویرایش کنه (دابل‌کلیک)
//   - کالا حذف کنه
// ================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StoreManager.DAL.Repositories;
using StoreManager.Models;

namespace StoreManager.UI.Views
{
    // صفحه مدیریت کالاها
    public partial class ProductsPage : Page
    {
        // ProductRepository = کلاسی که کارهای دیتابیس کالا رو انجام میده
        private readonly ProductRepository _repo = new();

        // ---- وقتی صفحه باز میشه ----
        public ProductsPage()
        {
            InitializeComponent(); // کنترل‌های XAML رو بارگذاری کن
            LoadCategories();      // دسته‌بندی‌ها رو توی ComboBox بریز
            LoadProducts();        // لیست کالاها رو نشون بده
        }

        // ---- دسته‌بندی‌ها رو از دیتابیس میخونه و توی ComboBox میذاره ----
        private void LoadCategories()
        {
            // اول "همه دسته‌ها" رو اضافه کن (برای نشون دادن همه کالاها)
            var cats = new System.Collections.Generic.List<Category> {
                new Category { Id = 0, Name = "همه دسته‌ها" }
            };

            // بعد دسته‌بندی‌های واقعی رو از دیتابیس بخون
            var dt = DAL.DatabaseHelper.ExecuteQuery("SELECT * FROM Categories WHERE IsActive=1 ORDER BY Name");
            foreach (System.Data.DataRow r in dt.Rows)
                cats.Add(new Category {
                    Id = System.Convert.ToInt32(r["Id"]),
                    Name = r["Name"].ToString()
                });

            cbCategory.ItemsSource = cats;  // توی ComboBox بریز
            cbCategory.SelectedIndex = 0;   // "همه دسته‌ها" رو انتخاب کن
        }

        // ---- لیست کالاها رو (با توجه به فیلترها) نشون میده ----
        private void LoadProducts()
        {
            // ببین کدوم دسته‌بندی انتخاب شده
            int catId = cbCategory.SelectedValue != null ? (int)cbCategory.SelectedValue : 0;
            // کالاها رو از دیتابیس بگیر (با جستجو و فیلتر دسته‌بندی)
            dgProducts.ItemsSource = _repo.GetAll(txtSearch.Text, catId);
        }

        // ---- وقتی کاربر توی جستجو تایپ میکنه، لیست آپدیت میشه ----
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadProducts();

        // ---- وقتی دسته‌بندی عوض میشه، لیست آپدیت میشه ----
        private void CbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadProducts();

        // ---- دکمه "کالای جدید" ----
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // یه پنجره کوچیک (Dialog) برای وارد کردن اطلاعات کالا باز کن
            // new Product() = یه کالای خالی بده (چون داریم جدید اضافه میکنیم)
            var dlg = new ProductDialog(new Product()) { Owner = Window.GetWindow(this) };

            // اگه کاربر "ذخیره" زد (نه "انصراف")، لیست رو آپدیت کن
            if (dlg.ShowDialog() == true) LoadProducts();
        }

        // ---- وقتی روی یه کالا دابل‌کلیک میکنه، ویرایش باز میشه ----
        private void DgProducts_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // چک کن آیا واقعاً روی یه کالا کلیک کرده (نه روی ردیف خالی)
            if (dgProducts.SelectedItem is Product p)
            {
                // پنجره ویرایش رو با اطلاعات اون کالا باز کن
                var dlg = new ProductDialog(p) { Owner = Window.GetWindow(this) };
                if (dlg.ShowDialog() == true) LoadProducts();
            }
        }

        // ---- دکمه حذف کالا ----
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // اگه کالایی انتخاب شده و کاربر تایید کرد، حذف کن
            if (dgProducts.SelectedItem is Product p &&
                MessageBox.Show($"کالای «{p.Name}» حذف شود؟", "حذف", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _repo.Delete(p.Id); // از دیتابیس حذف (یا بهتر بگیم غیرفعال) کن
                LoadProducts();     // لیست رو رفرش کن
            }
        }
    }
}
