﻿using StockBand.Data;
using StockBand.Models;
using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class SettingsUserDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        [MinLength(5)]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[0-9a-zA-Z]*$", ErrorMessage = Message.Code11)]
        public string Name { get; set; }
        [Required]
        public bool Block { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password admin")]
        public string PasswordAdmin { get; set; }
        public string Role { get; set; }
    }
}
