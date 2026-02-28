using System;
using System.Collections.Generic;

namespace LeaveOTManagement.Models.Entities;

public partial class User
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int RoleId { get; set; }

    public int? ManagerId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<User> InverseManager { get; set; } = new List<User>();

    public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual User? Manager { get; set; }

    public virtual ICollection<Otrequest> Otrequests { get; set; } = new List<Otrequest>();

    public virtual Role Role { get; set; } = null!;
}
