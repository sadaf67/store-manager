// -----------------------------------------------
// این فایل یه "قالب" برای کالاست
// مثلاً وقتی میخوایم یه کالا ذخیره کنیم،
// همه اطلاعاتش رو اینجا نگه میداریم
// -----------------------------------------------

using System;

namespace StoreManager.Models
{
    // این کلاس مثل یه برگه اطلاعاتیه برای هر کالا
    // هر چیزی که درباره یه کالا باید بدونیم اینجاست
    public class Product
    {
        // شماره یونیک هر کالا - خود دیتابیس این رو میده
        public int Id { get; set; }

        // کد کالا - مثلاً "A001" که روی برچسب میزنیم
        public string Code { get; set; }

        // اسم کالا - مثلاً "شیر کم‌چرب 1 لیتری"
        public string Name { get; set; }

        // شماره دسته‌بندی این کالا (مثلاً 3 = لبنیات)
        public int CategoryId { get; set; }

        // اسم دسته‌بندی که وقتی از دیتابیس میخونیم پرش میکنیم
        // خود جدول کالا این رو نداره، از جدول دسته‌بندی میاریم
        public string CategoryName { get; set; }

        // واحد اندازه‌گیری - مثلاً "عدد" یا "کیلوگرم" یا "متر"
        public string Unit { get; set; }

        // قیمتی که ما از تامین‌کننده خریدیم (قیمت خرید)
        public decimal BuyPrice { get; set; }

        // قیمتی که به مشتری میفروشیم (قیمت فروش)
        public decimal SellPrice { get; set; }

        // موجودی الان - چند تا از این کالا داریم؟
        public decimal Stock { get; set; }

        // حداقل موجودی - اگه کمتر از این بشه هشدار بده
        public decimal MinStock { get; set; }

        // توضیحات اضافه - هر چیزی که لازمه بدونیم
        public string Description { get; set; }

        // این کالا فعاله یا حذف شده؟
        // true = فعال، false = حذف شده (ولی از دیتابیس پاک نمیکنیم)
        public bool IsActive { get; set; } = true;

        // تاریخ اضافه شدن این کالا به سیستم
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
