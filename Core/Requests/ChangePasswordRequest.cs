﻿namespace Core.Requests;

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
