using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.SecurityServices
{
    public interface ISecurityServise
    {
        public string CreateToken(User user);
        public string CreatePasswordHash(string password);
    }
}
