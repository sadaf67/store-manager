// ================================================
// این فایل یه "مترجم کوچیک" برای رابط کاربریه
// کارش اینه که وقتی یه عدد منفیه، true برگردونه
// کجا استفاده میشه؟ مثلاً توی XAML میگیم:
//   "اگه موجودی مشتری منفی بود (بدهکاره)، متن رو قرمز کن"
// WPF این تبدیل رو خودش نمیفهمه، به این کلاس نیاز داره
// ================================================

using System;
using System.Globalization;
using System.Windows.Data;

namespace StoreManager.UI.Converters
{
    // IValueConverter = یه قرارداد (اینترفیس) که WPF داره
    // میگه: "هر کلاسی که بخواد مترجم باشه باید این دو متد رو داشته باشه"
    public class NegativeToBoolConverter : IValueConverter
    {
        // ---- تبدیل عدد به true/false ----
        // value = مقداری که WPF بهمون میده (مثلاً موجودی مشتری)
        // اگه عدد منفی بود: true برگردون (یعنی "بله، بدهکاره")
        // اگه صفر یا مثبت بود: false برگردون (یعنی "نه، بدهکار نیست")
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is decimal d && d < 0;
        // این یه خط مختصر نویسیه:
        // "اگه value یه decimal بود و کوچیکتر از صفر بود، true، وگرنه false"

        // ---- این جهت عکس - ما ازش استفاده نمیکنیم ----
        // ConvertBack = از true/false به عدد برگردون
        // ما این رو نیاز نداریم، پس خطا میده اگه کسی صداش زد
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
