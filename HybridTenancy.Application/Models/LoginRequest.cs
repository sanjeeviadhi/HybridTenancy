﻿namespace HybridTenancy.Application.Models
{
    public class LoginRequest
    {
        public string Identifier { get; set; } = string.Empty; 
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
