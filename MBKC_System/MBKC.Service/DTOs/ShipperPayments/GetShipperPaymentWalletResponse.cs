﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.DTOs.ShipperPayments
{
    public class GetShipperPaymentWalletResponse
    {
        public int PaymentId { get; set; }
        public string Status { get; set; }
        public string Content { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public string PaymentMethod { get; set; }
        public int OrderId { get; set; }
        public int? KCBankingAccountId { get; set; }
        public string?KCBankingAccountName { get; set; }
    }
}
