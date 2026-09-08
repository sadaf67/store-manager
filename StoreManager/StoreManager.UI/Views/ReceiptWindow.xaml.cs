// ================================================
// این فایل "مغز پنجره رسید فاکتور" ماست
// بعد از اینکه فاکتور ثبت میشه، این پنجره باز میشه
// توش نشون میده:
//   - اطلاعات فروشگاه (اسم، تلفن، آدرس)
//   - اطلاعات فاکتور (شماره، تاریخ، مشتری)
//   - لیست کالاهای خریداری شده
//   - جمع، تخفیف، مالیات، مبلغ نهایی، مبلغ پرداختی
//   - QR Code قابل اسکن با گوشی
// همچنین میتونه:
//   - فاکتور رو چاپ کنه
//   - QR Code رو به فایل PNG ذخیره کنه
// ================================================

using Microsoft.Win32;
using StoreManager.DAL;
using StoreManager.Models;
using StoreManager.UI.Helpers;
using System;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;

namespace StoreManager.UI.Views
{
    // پنجره نمایش رسید فاکتور
    public partial class ReceiptWindow : Window
    {
        // اطلاعات فاکتوری که باید نشون بدیم
        private readonly Invoice _invoice;

        // تصویر QR Code که ساختیم (برای ذخیره کردن بعداً)
        private BitmapImage _qrCodeImage;

        // ---- وقتی پنجره باز میشه ----
        // invoice = اطلاعات کامل فاکتور (همراه ردیف‌های کالا)
        public ReceiptWindow(Invoice invoice)
        {
            InitializeComponent();

            // اگه فاکتور null بود، خطا بده (نباید اتفاق بیفته)
            _invoice = invoice ?? throw new ArgumentNullException(nameof(invoice));

            // راست به چپ برای فارسی
            FlowDirection = FlowDirection.RightToLeft;

            LoadReceiptData();           // اطلاعات رو توی صفحه نشون بده
            GenerateAndDisplayQRCode();  // QR Code بساز و نشون بده
        }

        // ---- اطلاعات فاکتور رو توی کنترل‌های صفحه میریزه ----
        private void LoadReceiptData()
        {
            // ---- بخش اول: اطلاعات فروشگاه ----
            var settings = LoadStoreSettings(); // تنظیمات رو از دیتابیس بخون
            txtStoreName.Text    = settings.StoreName;  // اسم فروشگاه
            txtStoreType.Text    = settings.StoreType;  // نوع صنف
            txtStorePhone.Text   = settings.Phone;      // تلفن
            txtStoreAddress.Text = settings.Address;    // آدرس
            txtFooter.Text       = settings.ReceiptFooter; // متن پایین رسید

            // ---- بخش دوم: اطلاعات سرفاکتور ----
            txtInvoiceNo.Text = _invoice.InvoiceNo;
            txtDate.Text      = _invoice.Date.ToString("yyyy/MM/dd   HH:mm");

            // اگه مشتری نداشتیم (نقدی بود)، "نقدی" بنویس
            txtCustomer.Text = string.IsNullOrEmpty(_invoice.CustomerName) ? "نقدی" : _invoice.CustomerName;

            // نوع پرداخت رو به فارسی تبدیل کن
            txtPayType.Text = _invoice.PaymentType switch
            {
                PaymentType.Cash   => "نقدی",
                PaymentType.Credit => "نسیه",
                PaymentType.Card   => "کارت‌خوان",
                PaymentType.Mixed  => "ترکیبی",
                _ => "نقدی"
            };

            // ---- بخش سوم: ردیف‌های کالا ----
            // ItemsControl به صورت خودکار برای هر ردیف یه Template نشون میده
            lstItems.ItemsSource = _invoice.Items;

            // ---- بخش چهارم: جمع‌بندی مالی ----
            string currency = settings.Currency ?? "ریال";
            txtTotal.Text    = $"{_invoice.TotalAmount:N0} {currency}"; // جمع کل

            // اگه تخفیف داشتیم نشون بده، وگرنه "---" بذار
            txtDiscount.Text = _invoice.Discount > 0 ? $"( {_invoice.Discount:N0} {currency} )" : "---";

            // اگه مالیات داشتیم نشون بده، وگرنه "---" بذار
            txtTax.Text  = _invoice.Tax > 0 ? $"{_invoice.Tax:N0} {currency}" : "---";

            txtFinal.Text = $"{_invoice.FinalAmount:N0} {currency}";   // مبلغ نهایی
            txtPaid.Text  = $"{_invoice.PaidAmount:N0} {currency}";    // پرداخت شده

            // مانده: اگه صفر بود یعنی "تسویه" شده
            if (_invoice.RemainAmount <= 0)
                txtRemain.Text = "تسویه ✓"; // علامت تیک = پرداخت کامل
            else
                txtRemain.Text = $"{_invoice.RemainAmount:N0} {currency}"; // مانده بدهی
        }

        // ---- QR Code فاکتور رو میسازه و توی صفحه نشون میده ----
        private void GenerateAndDisplayQRCode()
        {
            try
            {
                // QR Code حاوی اطلاعات فاکتور بساز
                // کسی که اسکن کنه: شماره فاکتور، مبلغ، تاریخ و مشتری رو میبینه
                _qrCodeImage = QRCodeHelper.GenerateInvoiceQR(
                    invoiceNo:    _invoice.InvoiceNo,
                    amount:       _invoice.FinalAmount,
                    date:         _invoice.Date,
                    customerName: _invoice.CustomerName ?? ""
                );

                // QR Code رو توی Image کنترل نشون بده
                imgQRCode.Source = _qrCodeImage;
            }
            catch (Exception ex)
            {
                // اگه QR Code ساخته نشد، فقط یه هشدار بده
                // پنجره همچنان نشون داده میشه، فقط QR نداره
                MessageBox.Show($"خطا در تولید QR Code:\n{ex.Message}",
                    "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ---- تنظیمات فروشگاه رو از دیتابیس میخونه ----
        private AppSettings LoadStoreSettings()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT [Key],[Value] FROM AppSettings");
            var d = new System.Collections.Generic.Dictionary<string, string>();
            foreach (System.Data.DataRow r in dt.Rows)
                d[r["Key"].ToString()] = r["Value"]?.ToString() ?? "";

            // یه object تنظیمات بساز و مقادیر رو بریز
            return new AppSettings
            {
                StoreName     = d.GetValueOrDefault("StoreName", "فروشگاه من"),
                StoreType     = d.GetValueOrDefault("StoreType", ""),
                Phone         = d.GetValueOrDefault("Phone", ""),
                Address       = d.GetValueOrDefault("Address", ""),
                Currency      = d.GetValueOrDefault("Currency", "ریال"),
                ReceiptFooter = d.GetValueOrDefault("ReceiptFooter", "")
            };
        }

        // ---- دکمه "چاپ رسید" ----
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            // پنجره انتخاب پرینتر رو نشون بده
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true) return; // اگه انصراف داد، برگرد

            try
            {
                // محتوای کل پنجره رو چاپ کن
                printDialog.PrintVisual(this, $"رسید فاکتور {_invoice.InvoiceNo}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در چاپ:\n{ex.Message}", "خطا در چاپ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---- دکمه "ذخیره QR Code" ----
        // QR Code رو به عنوان فایل PNG ذخیره میکنه
        // کاربر میتونه توی واتساپ یا تلگرام برای مشتری بفرسته
        private void BtnSaveQR_Click(object sender, RoutedEventArgs e)
        {
            if (_qrCodeImage == null)
            {
                MessageBox.Show("QR Code تولید نشده است.", "خطا",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // پنجره ذخیره فایل رو نشون بده
            var saveDialog = new SaveFileDialog
            {
                Title    = "ذخیره QR Code فاکتور",
                Filter   = "PNG Image|*.png|JPEG Image|*.jpg",
                // اسم پیش‌فرض: QR_فاکتور_شماره_تاریخ
                FileName = $"QR_Invoice_{_invoice.InvoiceNo}_{DateTime.Now:yyyyMMdd}"
            };

            if (saveDialog.ShowDialog() != true) return;

            try
            {
                // QR Code رو با کیفیت بالا ذخیره کن (برای چاپ)
                QRCodeHelper.SaveQRCodeToFile(
                    text:            BuildQRText(),
                    filePath:        saveDialog.FileName,
                    pixelsPerModule: 10  // هر مربع = 10 پیکسل (کیفیت بالا)
                );

                MessageBox.Show(
                    $"QR Code در مسیر زیر ذخیره شد:\n{saveDialog.FileName}",
                    "ذخیره موفق", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره:\n{ex.Message}", "خطا",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---- متن QR Code فاکتور رو میسازه ----
        // این متن توی QR Code رمزگذاری میشه
        // هر بخش با | از هم جدا شده
        private string BuildQRText()
        {
            return $"INVOICE:{_invoice.InvoiceNo}"             +
                   $"|DATE:{_invoice.Date:yyyy/MM/dd HH:mm}"  +
                   $"|AMOUNT:{_invoice.FinalAmount:N0}"        +
                   $"|PAID:{_invoice.PaidAmount:N0}"           +
                   $"|REMAIN:{_invoice.RemainAmount:N0}"       +
                   $"|CUSTOMER:{_invoice.CustomerName ?? "نقدی"}";
        }

        // ---- دکمه "بستن" ----
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close(); // پنجره رو ببند
        }
    }
}
