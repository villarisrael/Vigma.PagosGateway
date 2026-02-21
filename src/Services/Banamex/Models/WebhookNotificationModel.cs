using System;

namespace gateway_csharp_sample_code.Models
{
    public class WebhookNotificationModel
    {
        public long Timestamp { get; set; }
        public TransactionModel Transaction { get; set; }
        public OrderModel Order { get; set; }

        public string OrderId => Order.Id;
        public string TransactionId => Transaction.Id;
        public string OrderStatus => Order.Status;
        public string Amount => Order.Amount;
    }

    public class OrderModel
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Amount { get; set; }
    }

    public class TransactionModel
    {
        public string Id { get; set; }
    }
}
