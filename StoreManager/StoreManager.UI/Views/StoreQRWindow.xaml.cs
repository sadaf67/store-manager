// ================================================
// این فایل "مغز پنجره QR Code فروشگاه" ماست
// وقتی از صفحه تنظیمات دکمه "QR Code فروشگاه" رو میزنیم،
// این پنجره باز میشه
// یه QR Code نشون میده که:
//   - مشتری با گوشیش اسکن میکنه
//   - گوشی ازش میپرسه "میخوای این شماره رو ذخیره کنی؟"
//   - با یه کلیک شماره فروشگاه توی مخاطبین ذخیره میشه
// ================================================

using Microsoft.Win32;
using StoreManager.UI.Helpers;
using System;
using System.Windows;

namespace StoreManager.UI.Views
{
    // پنجره QR Code اطلاعات تماس فروشگاه
    public partial class StoreQRWindow : Window
    {
        // اطلاعات فروشگاه که توی QR Code میریزیم
        private readonly string _storeName;
        private readonly string _phone;
        private readonly string _address;

        // ---- وقتی پنجره باز میشه ----
        // اطلاعات فروشگاه از صفحه تنظیمات میان
        public StoreQRWindow(string storeName, string phone, string address)
        {
            InitializeComponent();

            // اطلاعات رو ذخیره کن (بعداً برای ذخیره فایل نیاز داریم)
            _storeName = storeName;
            _phone     = phone;
            _address   = address;

            // راست به چپ برای فارسی
            FlowDirection = FlowDirection.RightToLeft;

            LoadQRCode(); // QR Code رو بساز و نشون بده
        }

        // ---- QR Code اطلاعات تماس فروشگاه رو میسازه و نشون میده ----
        private void LoadQRCode()
        {
            try
            {
                // اطلاعات متنی رو زیر QR Code نشون بده
                txtName.Text  = _storeName;
                txtPhone.Text = _phone;

                // QR Code با فرمت vCard بساز
                // vCard = یه استاندارد جهانی برای اطلاعات تماس
                // همه گوشی‌ها (اندروید و iOS) این فرمت رو میفهمن
                var qrImage = QRCodeHelper.GenerateStoreContactQR(
                    storeName: _storeName,
                    phone:     _phone,
                    address:   _address
                );

                // QR Code رو توی Image کنترل نشون بده
                imgQR.Source = qrImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در تولید QR Code:\n{ex.Message}",
                    "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---- دکمه "ذخیره QR Code" ----
        // کاربر میتونه QR Code رو به فایل PNG ذخیره کنه
        // مثلاً برای استفاده روی بنر فروشگاه، کارت ویزیت، اینستاگرام و...
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // پنجره ذخیره فایل رو نشون بده
            var saveDialog = new SaveFileDialog
            {
                Title    = "ذخیره QR Code فروشگاه",
                Filter   = "PNG Image|*.png",
                FileName = $"QR_Store_{_storeName}" // اسم پیش‌فرض
            };

            if (saveDialog.ShowDialog() != true) return; // اگه انصراف داد، برگرد

            try
            {
                // QR Code رو با کیفیت بسیار بالا ذخیره کن
                // pixelsPerModule=15 یعنی هر مربع QR = 15 پیکسل
                // این کیفیت برای چاپ روی بنر مناسبه
                QRCodeHelper.SaveQRCodeToFile(
                    text:            BuildVCardText(),
                    filePath:        saveDialog.FileName,
                    pixelsPerModule: 15
                );

                MessageBox.Show("QR Code با موفقیت ذخیره شد.", "موفق",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره:\n{ex.Message}", "خطا",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---- متن vCard رو برای QR Code میسازه ----
        // vCard 3.0 = یه فرمت استاندارد که توی QR Code رمزگذاری میشه
        // هر خط یه اطلاعات داره (اسم، تلفن، آدرس و...)
        private string BuildVCardText()
        {
            return $"BEGIN:VCARD\r\n"      + // شروع vCard
                   $"VERSION:3.0\r\n"     + // نسخه فرمت
                   $"FN:{_storeName}\r\n" + // FN = Full Name (اسم کامل)
                   $"ORG:{_storeName}\r\n"+ // ORG = Organization (شرکت/فروشگاه)
                   $"TEL:{_phone}\r\n"    + // TEL = Telephone (تلفن)
                   $"ADR:{_address}\r\n"  + // ADR = Address (آدرس)
                   $"END:VCARD";            // پایان vCard
        }

        // ---- دکمه "بستن" ----
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
