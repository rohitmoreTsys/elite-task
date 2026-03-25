using Elite.Common.Utilities.CommonType;
using Elite.Meeting.Microservice.Models;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite_Task.Microservice.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Models.Entities
{
    public partial class EliteTaskContext : DbContext, IUnitOfWork
    {
        public virtual DbSet<EliteTask> EliteTask { get; set; }
        public virtual DbSet<GlobalSearchTaskEntity> GlobalSearchTaskEntity { get; set; }

        public virtual DbSet<TaskAttachmentMapping> TaskAttachmentMapping { get; set; }

        public virtual DbSet<NoticationEventStore> NoticationEventStore { get; set; }

        public virtual DbSet<TaskComment> TaskComment { get; set; }
        public virtual DbSet<TaskCommentAttachmentMapping> TaskCommentAttachmentMapping { get; set; }

        public virtual DbSet<EliteTask> user_tasks { get; set; }

        public virtual DbSet<UserCommittees> user_committees_in_tasks { get; set; }
        public virtual DbSet<vw_alltaskattachments> vwAllTaskatttachments { get; set; }
        public virtual DbSet<TaskOverdueSummary> get_task_overdue_summary { get; set; }

        public EliteTaskContext(DbContextOptions<EliteTaskContext> options)
           : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("elite");
            modelBuilder.Entity<EliteTask>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AgendaId).HasColumnName("AgendaID");

                entity.Property(e => e.CommitteeId).HasColumnName("CommitteeID");

                entity.Property(e => e.CreatedBy).HasColumnType("json");

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MeetingId).HasColumnName("MeetingID");

                entity.Property(e => e.ModifiedBy).HasColumnType("json");

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.Property(e => e.Responsible).HasColumnType("json");

                entity.Property(e => e.CoResponsibles).HasColumnType("json");
                
                entity.Property(e => e.CoResponsibleEmailRecipient).HasColumnType("json");

                entity.Property(e => e.ResponsibleEmailRecipient).HasColumnType("json");

                entity.Property(e => e.TaskGuid).HasColumnName("TaskGUID");

                entity.Property(e => e.IsNotify).HasColumnName("IsNotify");

                entity.Property(e => e.JiraTicketInfo).HasColumnName("JiraTicketInfo").HasColumnType("json");

                entity.Property(e => e.IsPublishedToJira).HasColumnName("IsPublishedToJira");

                entity.Property(e => e.IsCustomEmailRecipient).HasColumnName("IsCustomEmailRecipient");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("SubTask_FK");
            });

            modelBuilder.Entity<TaskComment>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatedBy).HasColumnType("json");

                entity.Property(e => e.ModifiedBy).HasColumnType("json");

                entity.Property(e => e.TaskId).HasColumnName("taskId");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.TaskComment)
                    .HasForeignKey(d => d.TaskId)
                    .HasConstraintName("TaskComment_FK");
            });


            modelBuilder.Entity<NoticationEventStore>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.IsFailed).HasColumnName("isFailed");

                entity.Property(e => e.IsProcessed).HasColumnName("isProcessed");

                entity.Property(e => e.JsonMessage)
                    .IsRequired()
                    .HasColumnType("json");

            entity.Property(e => e.Sourcetypeid).HasColumnName("sourcetypeid");

                entity.Property(e => e.IsReminder).HasColumnName("IsReminder");
            
            });

            modelBuilder.Entity<TaskAttachmentMapping>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AttachmentGuid).HasColumnName("AttachmentGUID");

                entity.Property(e => e.AttachmentName).HasMaxLength(200);

                entity.Property(e => e.CreatedBy).HasColumnType("json");

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.TaskAttachmentMapping)
                    .HasForeignKey(d => d.TaskId)
                    .HasConstraintName("TaskAttachment_FK");
            });

            modelBuilder.Entity<TaskCommentAttachmentMapping>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AttachmentGuid).HasColumnName("AttachmentGUID");
                entity.Property(e => e.AttachmentName).HasColumnName("AttachmentName");
                entity.Property(e => e.AttachmentSize).HasColumnName("AttachmentSize");


                entity.Property(e => e.CreatedBy).HasColumnType("json");

                entity.Property(e => e.commentId).HasColumnName("CommentID");
                entity.Property(e => e.CreatedDate).HasColumnType("date");

                entity.HasOne(d => d.Comment)
                   .WithMany(p => p.TaskCommentAttachmentMapping)
                   .HasForeignKey(d => d.commentId)
                   .HasConstraintName("TaskCommentAttachment_FK");


            });

            modelBuilder.HasSequence("TaskComment_ID_seq");
            modelBuilder.HasSequence("NoticationEventStore_ID_seq");

            modelBuilder.Entity<vw_alltaskattachments>(entity =>
            {
                entity.HasKey(e => new { e.AttachmentGUID });
                entity.ToTable("vw_alltaskattachments");
            });
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {

            var result = await base.SaveChangesAsync();
            return result;
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken)
        {
            return Convert.ToBoolean(await base.SaveChangesAsync());
        }
    }
}
