// ================================================
// این فایل "کارخونه QR Code" ماست
// هر جا توی برنامه QR Code نیاز داشتیم، از اینجا میگیریم
// سه نوع QR Code میسازه:
//   ۱. QR Code کالا یا متن دلخواه
//   ۲. QR Code فاکتور (شماره فاکتور + مبلغ + تاریخ)
//   ۳. QR Code اطلاعات تماس فروشگاه (فرمت vCard)
// ================================================

using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace StoreManager.UI.Helpers
{
    // static یعنی نیازی نیست new کنیم، مستقیم صداش میزنیم
    public static class QRCodeHelper
    {
        // ---- ساخت QR Code از هر متنی ----
        // text = متنی که میخوایم توی QR Code بریزیم
        // pixelsPerModule = هر مربع کوچیک QR چند پیکسل باشه (بزرگتر = واضح‌تر)
        // برمیگردونه: BitmapImage که میشه مستقیم توی Image کنترل WPF نشون داد
        public static BitmapImage GenerateQRCode(string text, int pixelsPerModule = 5)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("متن QR Code نمی‌تواند خالی باشد.", nameof(text));

            // ---- ساخت داده QR Code ----
            // QRCodeGenerator = کلاسی که متن رو به داده QR تبدیل میکنه
            // ECCLevel.M = سطح تصحیح خطای متوسط (15% از QR آسیب ببینه باز هم خوانده میشه)
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new QRCode(qrData);

            // ---- تبدیل به تصویر با رنگ آبی فروشگاه ----
            // darkColor = رنگ مربع‌های QR (آبی تیره - رنگ برنامه)
            // lightColor = رنگ پس‌زمینه (سفید)
            // drawQuietZones = حاشیه سفید اطراف که برای خوانایی لازمه
            using var bitmap = qrCode.GetGraphic(
                pixelsPerModule,
                darkColor: Color.FromArgb(21, 101, 192),   // آبی تیره (#1565C0)
                lightColor: Color.White,
                drawQuietZones: true
            );

            // تبدیل Bitmap (فرمت Windows) به BitmapImage (فرمت WPF)
            return BitmapToBitmapImage(bitmap);
        }

        // ---- ساخت QR Code مخصوص فاکتور ----
        // اطلاعات فاکتور رو توی QR Code میریزه
        // کاربر با گوشیش QR رو اسکن میکنه و اطلاعات فاکتور رو میبینه
        public static BitmapImage GenerateInvoiceQR(string invoiceNo, decimal amount, DateTime date, string customerName = "")
        {
            // ساختار داده فاکتور - فرمت ساده با | بین هر بخش
            // مثال: INVOICE:S202406001|DATE:2024/06/01 14:30|AMOUNT:500,000|CUSTOMER:علی محمدی
            var qrContent = $"INVOICE:{invoiceNo}" +
                            $"|DATE:{date:yyyy/MM/dd HH:mm}" +
                            $"|AMOUNT:{amount:N0}" +
                            $"|CUSTOMER:{customerName}";

            return GenerateQRCode(qrContent, pixelsPerModule: 4); // کوچیکتر چون جای کمتری داریم
        }

        // ---- ساخت QR Code اطلاعات تماس فروشگاه ----
        // از فرمت vCard استفاده میکنه - یه استاندارد جهانی برای اطلاعات تماس
        // وقتی کسی این QR رو اسکن کنه، گوشیش ازش میپرسه:
        // "میخوای این شماره رو ذخیره کنی؟"
        public static BitmapImage GenerateStoreContactQR(string storeName, string phone, string address)
        {
            // فرمت vCard 3.0 - همه گوشی‌ها این رو میفهمن
            // BEGIN:VCARD و END:VCARD مثل باز و بستن پرانتز
            var vCard = $"BEGIN:VCARD\r\n" +
                        $"VERSION:3.0\r\n" +
                        $"FN:{storeName}\r\n" +     // Full Name = اسم کامل
                        $"ORG:{storeName}\r\n" +    // Organization = سازمان/شرکت
                        $"TEL:{phone}\r\n" +        // Telephone = تلفن
                        $"ADR:{address}\r\n" +      // Address = آدرس
                        $"END:VCARD";

            return GenerateQRCode(vCard, pixelsPerModule: 5);
        }

        // ---- ذخیره QR Code به صورت فایل PNG ----
        // text = متن QR Code
        // filePath = مسیری که میخوایم فایل رو ذخیره کنیم (مثلاً C:\Desktop\qr.png)
        // pixelsPerModule = کیفیت (10 = کیفیت خوب برای چاپ)
        public static void SaveQRCodeToFile(string text, string filePath, int pixelsPerModule = 10)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new QRCode(qrData);
            using var bitmap = qrCode.GetGraphic(pixelsPerModule);

            // PNG = فرمت بدون افت کیفیت (بهتر از JPG برای QR Code)
            bitmap.Save(filePath, ImageFormat.Png);
        }

        // ---- تبدیل Bitmap به BitmapImage ----
        // WPF از BitmapImage استفاده میکنه، ولی QRCoder به ما Bitmap میده
        // این متد این تبدیل رو انجام میده
        // این متد خصوصیه - فقط داخل این کلاس استفاده میشه
        private static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            // MemoryStream = یه فضای موقت توی RAM برای ذخیره داده‌های باینری
            using var memory = new MemoryStream();

            // تصویر رو توی RAM ذخیره کن (نه روی هارد)
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0; // برگرد به ابتدای RAM برای خوندن

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();       // شروع کار
            bitmapImage.StreamSource = memory; // منبع تصویر = همون RAM
            // CacheOnLoad = تصویر رو توی حافظه نگه دار حتی بعد از بسته شدن MemoryStream
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();         // پایان کار
            bitmapImage.Freeze();          // قفل کن تا از همه جاهای برنامه قابل استفاده باشه

            return bitmapImage;
        }
    }
}
