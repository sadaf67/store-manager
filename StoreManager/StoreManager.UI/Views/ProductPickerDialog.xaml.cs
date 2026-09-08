// ================================================
// این فایل "پنجره انتخاب کالا" ماست
// وقتی کاربر جستجو میکنه و چند تا کالا پیدا میشه،
// این پنجره باز میشه که کاربر بتونه کالای دقیق رو انتخاب کنه
// مثلاً: تایپ کرد "شیر" → سه تا کالا پیدا شد:
//   - شیر 1 لیتری
//   - شیر 0.5 لیتری
//   - شیر کم‌چرب
// این پنجره لیست اینا رو نشون میده تا کاربر انتخاب کنه
// ================================================

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StoreManager.Models;

namespace StoreManager.UI.Views
{
    // پنجره انتخاب کالا از لیست
    public partial class ProductPickerDialog : Window
    {
        // کالایی که کاربر انتخاب کرد - SalePage از اینجا میخونه
        public Product SelectedProduct { get; private set; }

        // ---- وقتی پنجره باز میشه ----
        // products = لیست کالاهایی که پیدا شدن
        public ProductPickerDialog(List<Product> products)
        {
            InitializeComponent();
            dgProducts.ItemsSource = products; // لیست کالاها رو توی جدول نشون بده
        }

        // ---- دکمه "انتخاب" ----
        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            // اگه یه کالا انتخاب شده، اون رو نگه‌دار و پنجره رو ببند
            if (dgProducts.SelectedItem is Product p)
            {
                SelectedProduct = p;  // کالای انتخاب شده رو ذخیره کن
                DialogResult = true;  // به صفحه قبل بگو "انتخاب شد"
            }
        }

        // ---- دابل‌کلیک روی کالا = انتخاب سریع ----
        // بدون نیاز به دکمه، فقط دابل‌کلیک کن تا انتخاب بشه
        private void DgProducts_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgProducts.SelectedItem is Product p)
            {
                SelectedProduct = p;
                DialogResult = true;
            }
        }
    }
}
