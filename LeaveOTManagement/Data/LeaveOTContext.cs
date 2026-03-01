using System;
using System.Collections.Generic;
using LeaveOTManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveOTManagement.Data;

public partial class LeaveOTContext : DbContext
{
    public LeaveOTContext()
    {
    }

    public LeaveOTContext(DbContextOptions<LeaveOTContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; }

    public virtual DbSet<Approval> Approvals { get; set; }

    public DbSet<Account> Accounts { get; set; }

    public virtual DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<LeaveBalance> LeaveBalances { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<Otdetail> Otdetails { get; set; }

    public virtual DbSet<Otrequest> Otrequests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Approval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Approval__3214EC07F464586F");

            entity.HasIndex(e => new { e.ApproverId, e.Status }, "IX_Approval_Inbox");

            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.RequestType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Approver).WithMany(p => p.Approvals)
                .HasForeignKey(d => d.ApproverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Approvals__Appro__6754599E");
        });

        modelBuilder.Entity<ApprovalWorkflow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Approval__3214EC076CA2BCE8");

            entity.HasIndex(e => new { e.RequestType, e.Level }, "UQ_Workflow").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RequestType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.ApprovalWorkflows)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ApprovalW__RoleI__6383C8BA");
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attachme__3214EC0728896DCE");

            entity.Property(e => e.EntityType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC073EB99A09");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "IX_Audit_Entity");

            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ActionDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.ActionByNavigation).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActionBy)
                .HasConstraintName("FK__AuditLogs__Actio__70DDC3D8");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC076073516E");

            entity.HasIndex(e => e.ParentDepartmentId, "IX_Department_Parent");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.ParentDepartment).WithMany(p => p.InverseParentDepartment)
                .HasForeignKey(d => d.ParentDepartmentId)
                .HasConstraintName("FK_Department_Parent");
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Holidays__3214EC0794617A56");

            entity.HasIndex(e => e.HolidayDate, "UQ__Holidays__F1ADD9D143471DAA").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveBal__3214EC07140EB5B7");

            entity.HasIndex(e => e.UserId, "IX_LeaveBalance_User");

            entity.HasIndex(e => new { e.UserId, e.LeaveTypeId, e.Year }, "UQ_LeaveBalance").IsUnique();

            entity.Property(e => e.TotalDays).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UsedDays)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveBalances)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveBala__Leave__4E88ABD4");

            entity.HasOne(d => d.User).WithMany(p => p.LeaveBalances)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveBala__UserI__4D94879B");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveReq__3214EC07300AC827");

            entity.HasIndex(e => new { e.FromDate, e.ToDate }, "IX_Leave_Date");

            entity.HasIndex(e => new { e.UserId, e.Status }, "IX_Leave_User_Status");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CurrentApprovalLevel).HasDefaultValue(1);
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalDays).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveRequ__Leave__5535A963");

            entity.HasOne(d => d.User).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveRequ__UserI__5441852A");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveTyp__3214EC0711D83439");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Otdetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OTDetail__3214EC07C030516C");

            entity.ToTable("OTDetails");

            entity.HasIndex(e => e.OtrequestId, "IX_OTDetail_Request");

            entity.Property(e => e.Hours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.OtrequestId).HasColumnName("OTRequestId");

            entity.HasOne(d => d.Otrequest).WithMany(p => p.Otdetails)
                .HasForeignKey(d => d.OtrequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OTDetails__OTReq__5EBF139D");
        });

        modelBuilder.Entity<Otrequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OTReques__3214EC07C3C33264");

            entity.ToTable("OTRequests");

            entity.HasIndex(e => new { e.UserId, e.Status }, "IX_OT_User_Status");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CurrentApprovalLevel).HasDefaultValue(1);
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.User).WithMany(p => p.Otrequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OTRequest__UserI__5BE2A6F2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07E9416F08");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6D6BBDE6A").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC076A4D2057");

            entity.HasIndex(e => e.DepartmentId, "IX_User_Department");

            entity.HasIndex(e => e.ManagerId, "IX_User_Manager");

            entity.HasIndex(e => e.EmployeeCode, "UQ__Users__1F64254897BA1F1F").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534272481FE").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__Departmen__440B1D61");

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Users__ManagerId__45F365D3");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__44FF419A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
