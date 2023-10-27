﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Models
{
    public class OrderDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderDetailId { get; set; }
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
        public int? MasterOrderDetailId { get; set; }

        [ForeignKey("Id")]
        public virtual Order Order { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        public virtual OrderDetail MasterOrderDetail { get; set; }
        public virtual IEnumerable<OrderDetail> ExtraOrderDetails { get; set; }
    }
}
