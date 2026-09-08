// -----------------------------------------------
// این فایل قالب فاکتوره
// هر بار که چیزی میفروشیم یا میخریم،
// یه فاکتور ثبت میشه که اطلاعاتش اینجاست
// -----------------------------------------------

using System;
using System.Collections.Generic;

namespace StoreManager.Models
{
    // نوع فاکتور - سه حالت داره:
    // Sale = فروش به مشتری
    // Purchase = خرید از تامین‌کننده
    // Return = برگشت از فروش (مشتری کالا برگردوند)
    public enum InvoiceType { Sale, Purchase, Return }

    // نوع پرداخت - چطور پول رد وبدل شد؟
    // Cash = نقدی (همون لحظه پول گرفتیم)
    // Credit = نسیه (بعداً میده)
    // Card = کارت‌خوان
    // Mixed = ترکیبی (مثلاً نصف نقد نصف نسیه)
    public enum PaymentType { Cash, Credit, Card, Mixed }

    // اطلاعات کامل یه فاکتور
    public class Invoice
    {
        // شماره یونیک فاکتور توی دیتابیس
        public int Id { get; set; }

        // شماره فاکتور که روی کاغذ چاپ میشه - مثلاً "S202406001"
        // S = فروش، P = خرید، R = برگشت
        public string InvoiceNo { get; set; }

        // نوع فاکتور (فروش/خرید/برگشت)
        public InvoiceType Type { get; set; }

        // شماره مشتری - اگه null باشه یعنی فروش نقدی بدون ثبت مشتری
        public int? CustomerId { get; set; }

        // اسم مشتری - وقتی از دیتابیس میخونیم پرش میکنیم
        public string CustomerName { get; set; }

        // تاریخ و ساعت ثبت فاکتور
        public DateTime Date { get; set; } = DateTime.Now;

        // جمع قیمت همه کالاها (قبل از تخفیف و مالیات)
        public decimal TotalAmount { get; set; }

        // مقدار تخفیف کل فاکتور
        public decimal Discount { get; set; }

        // مقدار مالیات
        public decimal Tax { get; set; }

        // مبلغ نهایی که مشتری باید بده
        // = TotalAmount - Discount + Tax
        public decimal FinalAmount { get; set; }

        // مبلغی که مشتری پرداخت کرده
        public decimal PaidAmount { get; set; }

        // مانده‌ای که هنوز نداده (بدهی مشتری از این فاکتور)
        // این خودش حساب میکنه، نیازی نیست ذخیره کنیم
        public decimal RemainAmount => FinalAmount - PaidAmount;

        // نوع پرداخت (نقد/نسیه/کارت/ترکیبی)
        public PaymentType PaymentType { get; set; }

        // توضیحات اضافه - مثلاً "مشتری گفت فردا میاد پول میده"
        public string Description { get; set; }

        // لیست کالاهایی که توی این فاکتور هستن
        // هر فاکتور میتونه چند تا کالا داشته باشه
        public List<InvoiceItem> Items { get; set; } = new();
    }

    // هر ردیف کالا توی فاکتور
    // مثلاً: شیر 1 لیتری × 3 عدد × 25,000 ریال
    public class InvoiceItem
    {
        // شماره یونیک این ردیف
        public int Id { get; set; }

        // به کدوم فاکتور تعلق داره؟
        public int InvoiceId { get; set; }

        // کدوم کالاست؟
        public int ProductId { get; set; }

        // اسم کالا - از جدول کالا میاریم
        public string ProductName { get; set; }

        // واحد اندازه‌گیری - مثلاً "عدد" یا "کیلو"
        public string Unit { get; set; }

        // چند تا؟ (تعداد یا وزن)
        public decimal Qty { get; set; }

        // قیمت هر واحد
        public decimal UnitPrice { get; set; }

        // تخفیف این ردیف خاص
        public decimal Discount { get; set; }

        // جمع این ردیف = تعداد × قیمت - تخفیف
        // خودش حساب میکنه
        public decimal TotalPrice => (Qty * UnitPrice) - Discount;
    }
}
