// ================================================
// این فایل "مغز صفحه ثبت فاکتور فروش" ماست
// مهم‌ترین صفحه برنامه!
// کاربر از اینجا میتونه:
//   - کالا جستجو کنه و به فاکتور اضافه کنه
//   - تعداد و قیمت هر کالا رو تغییر بده
//   - تخفیف و مالیات بذاره
//   - مشتری انتخاب کنه
//   - نوع پرداخت (نقد/نسیه/کارت) رو انتخاب کنه
//   - فاکتور رو ثبت کنه
// بعد از ثبت، پنجره رسید با QR Code نشون داده میشه
// ================================================

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StoreManager.BLL.Services;
using StoreManager.DAL.Repositories;
using StoreManager.Models;

namespace StoreManager.UI.Views
{
    // صفحه ثبت فاکتور
    public partial class SalePage : Page
    {
        // ابزارهایی که نیاز داریم
        private readonly ProductRepository _productRepo = new();    // برای جستجوی کالا
        private readonly CustomerRepository _customerRepo = new();  // برای لیست مشتریان
        private readonly SaleService _saleService = new();          // برای ثبت فاکتور

        // ObservableCollection = یه لیست هوشمند که وقتی چیزی بهش اضافه/حذف میشه
        // خودکار جدول صفحه رو آپدیت میکنه (بدون اینکه ما بگیم)
        private ObservableCollection<InvoiceItem> _items = new();

        // ---- وقتی صفحه باز میشه ----
        public SalePage()
        {
            InitializeComponent();
            dgItems.ItemsSource = _items; // جدول ردیف‌های فاکتور رو به لیست وصل کن
            cbType.SelectedIndex = 0;     // پیش‌فرض: فروش
            cbPayType.SelectedIndex = 0;  // پیش‌فرض: نقدی
            LoadCustomers();              // لیست مشتریان رو بارگذاری کن
        }

        // ---- مشتریان رو از دیتابیس میخونه و توی ComboBox میذاره ----
        private void LoadCustomers()
        {
            cbCustomer.ItemsSource = _customerRepo.GetAll();
        }

        // ---- وقتی کاربر توی فیلد جستجوی کالا Enter میزنه ----
        private void TxtProductSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AddProduct(); // Enter = جستجو کن
        }

        // ---- دکمه جستجوی کالا ----
        private void BtnSearch_Click(object sender, RoutedEventArgs e) => AddProduct();

        // ---- دکمه بارکد (فوکوس رو میبره به فیلد جستجو) ----
        // وقتی بارکدخوان داریم، بعد از زدن این دکمه بارکد رو اسکن میکنیم
        private void BtnBarcode_Click(object sender, RoutedEventArgs e)
        {
            txtProductSearch.Focus(); // فوکوس برو روی فیلد جستجو تا بارکد اسکن بشه
        }

        // ---- کالا رو جستجو میکنه و به لیست فاکتور اضافه میکنه ----
        private void AddProduct()
        {
            // کالاها رو بر اساس متن جستجو پیدا کن
            var products = _productRepo.GetAll(txtProductSearch.Text);

            if (products.Count == 0)
            {
                MessageBox.Show("کالایی یافت نشد.", "جستجو");
                return;
            }

            Product selected;
            if (products.Count == 1)
                // اگه فقط یه کالا پیدا شد، همونو انتخاب کن
                selected = products[0];
            else
            {
                // اگه چند تا پیدا شد، یه لیست نشون بده که کاربر انتخاب کنه
                var dlg = new ProductPickerDialog(products) { Owner = Window.GetWindow(this) };
                if (dlg.ShowDialog() != true) return;
                selected = dlg.SelectedProduct;
            }

            // چک کن آیا این کالا قبلاً توی فاکتور هست؟
            var existing = _items.IndexOf(_items.FirstOrDefault(x => x.ProductId == selected.Id));
            if (existing >= 0)
            {
                // اگه قبلاً بود، تعدادش رو +1 کن (نه اینکه یه ردیف جدید بساز)
                _items[existing] = new InvoiceItem
                {
                    ProductId = _items[existing].ProductId,
                    ProductName = _items[existing].ProductName,
                    Unit = _items[existing].Unit,
                    Qty = _items[existing].Qty + 1,       // تعداد +1
                    UnitPrice = _items[existing].UnitPrice,
                    Discount = _items[existing].Discount
                };
            }
            else
            {
                // اگه قبلاً نبود، یه ردیف جدید اضافه کن
                _items.Add(new InvoiceItem
                {
                    ProductId = selected.Id,
                    ProductName = selected.Name,
                    Unit = selected.Unit,
                    Qty = 1,                      // تعداد اولیه = 1
                    UnitPrice = selected.SellPrice // قیمت فروش پیش‌فرض
                });
            }

            txtProductSearch.Clear();     // فیلد جستجو رو خالی کن
            RecalcTotal(null, null);      // جمع کل رو دوباره حساب کن
        }

        // ---- دکمه حذف ردیف از فاکتور ----
        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgItems.SelectedItem is InvoiceItem item)
            {
                _items.Remove(item);        // از لیست حذف کن
                RecalcTotal(null, null);    // جمع رو آپدیت کن
            }
        }

        // ---- وقتی نوع فاکتور (فروش/خرید) عوض میشه ----
        private void CbType_Changed(object sender, SelectionChangedEventArgs e)
        {
            // میتونیم اینجا قیمت‌ها رو به قیمت خرید تغییر بدیم
        }

        // ---- جمع کل فاکتور رو حساب میکنه و نشون میده ----
        // هر بار که تخفیف یا مالیات عوض میشه، این متد صدا زده میشه
        public void RecalcTotal(object sender, TextChangedEventArgs e)
        {
            // جمع همه ردیف‌ها رو حساب کن
            decimal total = 0;
            foreach (var item in _items) total += item.TotalPrice;

            // تخفیف، مالیات و مبلغ پرداختی رو بخون (اگه نبود، 0 بذار)
            decimal.TryParse(txtDiscount?.Text, out decimal disc);
            decimal.TryParse(txtTax?.Text, out decimal tax);
            decimal.TryParse(txtPaid?.Text, out decimal paid);

            decimal final = total - disc + tax; // مبلغ نهایی

            // روی صفحه نشون بده
            if (lblTotal != null) lblTotal.Text = total.ToString("N0") + " ریال";
            if (lblFinal != null) lblFinal.Text = final.ToString("N0") + " ریال";
        }

        // ---- دکمه "ثبت فاکتور" ----
        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // چک کن که حداقل یه کالا وارد شده باشه
            if (_items.Count == 0)
            {
                MessageBox.Show("هیچ کالایی اضافه نشده است.", "خطا");
                return;
            }

            // اطلاعات فرم رو بخون
            var tag = (cbType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "0";
            decimal.TryParse(txtDiscount.Text, out decimal disc);
            decimal.TryParse(txtTax.Text, out decimal tax);
            decimal.TryParse(txtPaid.Text, out decimal paid);

            // یه object فاکتور بساز
            var invoice = new Invoice
            {
                Type = (InvoiceType)int.Parse(tag),                    // نوع: فروش/خرید/برگشت
                CustomerId = cbCustomer.SelectedValue as int?,          // مشتری (اگه انتخاب شده)
                Date = DateTime.Now,
                Discount = disc,
                Tax = tax,
                PaidAmount = paid,
                PaymentType = (PaymentType)cbPayType.SelectedIndex,    // نوع پرداخت
                Description = txtDesc.Text,
                Items = new System.Collections.Generic.List<InvoiceItem>(_items) // کپی ردیف‌ها
            };

            try
            {
                // فاکتور رو در دیتابیس ثبت کن
                // این کار همزمان: فاکتور + ردیف‌ها + موجودی + حساب مشتری رو آپدیت میکنه
                int id = _saleService.SaveSale(invoice);

                // فاکتور کامل رو از دیتابیس بخون (همراه ردیف‌ها)
                var savedInvoice = new DAL.Repositories.InvoiceRepository().GetById(id);

                // پنجره رسید با QR Code باز کن
                var receiptWindow = new ReceiptWindow(savedInvoice)
                {
                    Owner = Window.GetWindow(this)
                };
                receiptWindow.ShowDialog(); // صبر کن تا کاربر پنجره رو ببنده

                // بعد از موفقیت، فرم رو پاک کن برای فاکتور بعدی
                _items.Clear();
                txtDiscount.Text = "0";
                txtTax.Text = "0";
                txtPaid.Text = "0";
                RecalcTotal(null, null);
            }
            catch (Exception ex)
            {
                // اگه خطا خورد، پیام بده
                MessageBox.Show("خطا در ثبت فاکتور: " + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // ---- یه کلاس کمکی کوچیک ----
    // چون ObservableCollection خودش FirstOrDefault نداره، ما اضافه کردیم
    // Extension Method = متدی که به یه کلاس موجود اضافه میشه
    static class LinqHelper
    {
        public static T FirstOrDefault<T>(this ObservableCollection<T> col, Func<T, bool> predicate)
        {
            foreach (var item in col)
                if (predicate(item)) return item; // اگه شرط درست بود، همین رو برگردون
            return default; // اگه پیدا نشد، null برگردون
        }
    }
}
