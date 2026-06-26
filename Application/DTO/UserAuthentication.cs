using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTO
{
    public class UserAuthentication
    {
        [Required(ErrorMessage = "Логин обязателен для заполнения.")]
        public required string Login { get; set; }
        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        public required string Password { get; set; }
    }
}
