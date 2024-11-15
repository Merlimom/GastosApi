﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string token, string resetUrl);

}
