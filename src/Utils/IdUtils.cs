namespace BanamexPaymentGateway.Utils
{
    public static class IdUtils
    {
        /// <summary>
        /// Genera un ID único para órdenes y transacciones
        /// </summary>
        public static string GenerateSampleId()
        {
            return $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
        
        /// <summary>
        /// Genera un Order ID
        /// </summary>
        public static string GenerateOrderId()
        {
            return $"ORD-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
        
        /// <summary>
        /// Genera un Transaction ID
        /// </summary>
        public static string GenerateTransactionId()
        {
            return $"TXN-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
    }
}
