﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.DTOs.Cashiers
{
    public class UpdateCashierRequest
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public IFormFile? Avatar { get; set; }
        public string CitizenNumber { get; set; }
        public string Status { get; set; }
        public string? NewPassword { get; set; }
    }
}
