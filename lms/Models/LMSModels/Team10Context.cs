using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class Team10Context : DbContext
    {
        public virtual DbSet<Administrator> Administrator { get; set; }
        public virtual DbSet<Assignment> Assignment { get; set; }
        public virtual DbSet<AssignmentCategory> AssignmentCategory { get; set; }
        public virtual DbSet<Class> Class { get; set; }
        public virtual DbSet<Course> Course { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<Enrollment> Enrollment { get; set; }
        public virtual DbSet<Professor> Professor { get; set; }
        public virtual DbSet<Student> Student { get; set; }
        public virtual DbSet<Submission> Submission { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("Server=atr.eng.utah.edu;User Id=u0934995;Password=1234!Abcd&E;Database=Team10");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.HasKey(e => e.UId);

                entity.Property(e => e.UId)
                    .HasColumnName("u_id")
                    .HasColumnType("char(8)");

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(100);

                entity.Property(e => e.UserPass)
                    .HasColumnName("user_pass")
                    .HasMaxLength(20);

                entity.Property(e => e.birth_date)
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.AssignId);

                entity.HasIndex(e => e.AssignType)
                    .HasName("assign_type");

                entity.HasIndex(e => e.ClassId)
                    .HasName("class_id");

                entity.Property(e => e.AssignId)
                    .HasColumnName("assign_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AssignType)
                    .HasColumnName("assign_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ClassId)
                    .HasColumnName("class_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Contents)
                    .HasColumnName("contents")
                    .HasColumnType("text");

                entity.Property(e => e.Due)
                    .HasColumnName("due")
                    .HasColumnType("datetime");

                entity.Property(e => e.MaxPoints)
                    .HasColumnName("max_points");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100);

                entity.Property(e => e.SubmitFormat)
                    .HasColumnName("submit_format")
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("'b\\'0\\''");

                entity.HasOne(d => d.AssignTypeNavigation)
                    .WithMany(p => p.Assignment)
                    .HasForeignKey(d => d.AssignType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignment_ibfk_2");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Assignment)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignment_ibfk_1");
            });

            modelBuilder.Entity<AssignmentCategory>(entity =>
            {
                entity.HasKey(e => e.CatId);

                entity.HasIndex(e => e.ClassId)
                   .HasName("class_id");

                entity.Property(e => e.CatId)
                    .HasColumnName("cat_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100);

                entity.Property(e => e.Weight)
                    .HasColumnName("weight")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ClassId)
                    .HasColumnName("class_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Class)
                   .WithMany(p => p.AssignmentCategories)
                   .HasForeignKey(d => d.ClassId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("AssignmentCategory_Class_class_id_fk");

            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => e.CourseId)
                    .HasName("course_id");

                entity.HasIndex(e => e.ProfUId)
                    .HasName("Prof_fk");

                entity.Property(e => e.ClassId)
                    .HasColumnName("class_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.end_time)
                    .HasColumnName("end_time")
                    .HasColumnType("time");

                entity.Property(e => e.Location)
                    .HasColumnName("location")
                    .HasMaxLength(100);

                entity.Property(e => e.ProfUId)
                    .IsRequired()
                    .HasColumnName("prof_u_id")
                    .HasColumnType("char(8)");

                entity.Property(e => e.start_time)
                    .HasColumnName("start_time")
                    .HasColumnType("time");
                entity.Property(e => e.Semester)
                    .HasColumnName("semester")
                    .HasMaxLength(10);

                entity.Property(e => e.year)
                    .HasColumnType("year");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Class)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Class_ibfk_1");

                entity.HasOne(d => d.ProfU)
                    .WithMany(p => p.Class)
                    .HasForeignKey(d => d.ProfUId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Prof_fk");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => e.DeptId)
                    .HasName("dept_id");

                entity.HasIndex(e => new { e.CourseNum, e.DeptId })
                    .HasName("course_num")
                    .IsUnique();

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CourseNum)
                    .HasColumnName("course_num")
                    .HasColumnType("int(4)");

                entity.Property(e => e.DeptId)
                    .IsRequired()
                    .HasColumnName("dept_id")
                    .HasMaxLength(4);

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100);

                entity.Property(e => e.numCredits)
                   .HasColumnName("num_credits")
                   .HasColumnType("int(1)");

                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.Course)
                    .HasForeignKey(d => d.DeptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Course_ibfk_1");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DeptAbbrev);

                entity.Property(e => e.DeptAbbrev)
                    .HasColumnName("dept_abbrev")
                    .HasMaxLength(4);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.ClassId });

                entity.HasIndex(e => e.ClassId)
                    .HasName("class_id");

                entity.Property(e => e.StudentId)
                    .HasColumnName("student_id")
                    .HasColumnType("char(8)");

                entity.Property(e => e.ClassId)
                    .HasColumnName("class_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Grade)
                    .HasColumnName("grade")
                    .HasMaxLength(2);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrollment)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollment_ibfk_2");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Enrollment)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollment_ibfk_1");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.UId);

                entity.HasIndex(e => e.DeptId)
                    .HasName("dept_id");

                entity.Property(e => e.UId)
                    .HasColumnName("u_id")
                    .HasColumnType("char(8)");

                entity.Property(e => e.DeptId)
                    .IsRequired()
                    .HasColumnName("dept_id")
                    .HasMaxLength(4);

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(100);

                entity.Property(e => e.UserPass)
                    .HasColumnName("user_pass")
                    .HasMaxLength(20);

                entity.Property(e => e.birth_date)
                .HasColumnType("datetime");

                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.Professor)
                    .HasForeignKey(d => d.DeptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Professor_ibfk_1");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.UId);

                entity.HasIndex(e => e.DeptId)
                    .HasName("dept_id");

                entity.Property(e => e.UId)
                    .HasColumnName("u_id")
                    .HasColumnType("char(8)");

                entity.Property(e => e.DeptId)
                    .HasColumnName("dept_id")
                    .HasMaxLength(4);

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(100);

                entity.Property(e => e.UserPass)
                    .HasColumnName("user_pass")
                    .HasMaxLength(20);

                entity.Property(e => e.birth_date)
                    .HasColumnType("datetime");


                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.Student)
                    .HasForeignKey(d => d.DeptId)
                    .HasConstraintName("Student_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => e.SubId);

                entity.HasIndex(e => e.AssignId)
                    .HasName("assign_id");

                entity.HasIndex(e => new { e.UserId, e.SubmitTime, e.AssignId })
                    .HasName("user_id")
                    .IsUnique();

                entity.Property(e => e.SubId)
                    .HasColumnName("sub_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AssignId)
                    .HasColumnName("assign_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BinSub)
                    .HasColumnName("bin_sub")
                    .HasColumnType("blob");

                entity.Property(e => e.NumPoints).HasColumnName("num_points");

                entity.Property(e => e.SubmitTime)
                    .HasColumnName("submit_time")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.TextSub)
                    .HasColumnName("text_sub")
                    .HasColumnType("text");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("user_id")
                    .HasColumnType("char(8)");

                entity.HasOne(d => d.Assign)
                    .WithMany(p => p.Submission)
                    .HasForeignKey(d => d.AssignId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Submission)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_1");
            });
        }
    }
}
