using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LeaveOTManagement.TempModels;

public partial class TempContext : DbContext
{
    public TempContext()
    {
    }
     
    public TempContext(DbContextOptions<TempContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
