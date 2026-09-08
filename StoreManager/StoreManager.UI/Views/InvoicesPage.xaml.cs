// ================================================
// این فایل "مغز صفحه فاکتورها" ماست
// کاربر از این صفحه میتونه:
//   - لیست همه فاکتورها رو ببینه
//   - بر اساس تاریخ فیلتر کنه (از ... تا ...)
//   - نوع فاکتور رو انتخاب کنه (فروش/خرید/برگشت)
//   - روی فاکتور دابل‌کلیک کنه برای جزئیات
// ================================================

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StoreManager.DAL.Repositories;
using StoreManager.Models;

namespace StoreManager.UI.Views
{
    // صفحه لیست فاکتورها
    public partial class InvoicesPage : Page
    {
        // ابزار کار با دیتابیس فاکتورها
        private readonly InvoiceRepository _repo = new();

        // ---- وقتی صفحه باز میشه ----
        public InvoicesPage()
        {
            InitializeComponent();

            // تاریخ پیش‌فرض: از 30 روز پیش تا امروز
            dpFrom.SelectedDate = DateTime.Today.AddDays(-30);
            dpTo.SelectedDate = DateTime.Today;

            LoadInvoices(); // فاکتورها رو نشون بده
        }

        // ---- فاکتورها رو (با توجه به فیلترها) از دیتابیس میخونه و نشون میده ----
        private void LoadInvoices()
        {
            // نوع فاکتور رو بخون
            // اگه اولین گزینه ("همه") انتخاب شده، null بده (بدون فیلتر نوع)
            InvoiceType? type = cbType.SelectedIndex > 0 ? (InvoiceType?)(cbType.SelectedIndex - 1) : null;

            // فاکتورها رو با فیلترهای تاریخ و نوع بگیر
            var list = _repo.GetAll(dpFrom.SelectedDate, dpTo.SelectedDate, type);

            // لیست رو به یه فرم قابل نمایش تبدیل کن
            // (میخوایم "نوع" به جای عدد، فارسی نشون بده)
            var view = list.Select(i => new {
                i.Id,
                i.InvoiceNo,
                // نوع فاکتور رو به فارسی تبدیل کن
                TypeName = i.Type == InvoiceType.Sale ? "فروش" : i.Type == InvoiceType.Purchase ? "خرید" : "برگشت",
                i.Date,
                i.CustomerName,
                i.FinalAmount,
                i.PaidAmount,
                i.RemainAmount  // مانده (خودش حساب میکنه: Final - Paid)
            }).ToList();

            dgInvoices.ItemsSource = view; // جدول رو پر کن
        }

        // ---- دکمه "اعمال فیلتر" ----
        private void BtnFilter_Click(object sender, RoutedEventArgs e) => LoadInvoices();

        // ---- دابل‌کلیک روی فاکتور = نمایش جزئیات ----
        private void DgInvoices_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // میتونیم اینجا پنجره جزئیات فاکتور رو باز کنیم
            // در حال حاضر پیاده‌سازی نشده - قابل گسترش
        }
    }
}
