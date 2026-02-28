using System;

namespace LeaveOTManagement.Models.Entities;

public partial class Account
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int UserId { get; set; }

    public bool? IsLocked { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual User User { get; set; } = null!;
}