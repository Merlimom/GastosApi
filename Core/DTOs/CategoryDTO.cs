﻿namespace Core.DTOs;

public class CategoryDTO
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public UserBasicDTO User { get; set; } = null!;
}
public class UserBasicDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}