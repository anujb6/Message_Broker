using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Message_Broker.Models;

public partial class MessageBrokerContext : DbContext
{
    public MessageBrokerContext()
    {
    }

    public MessageBrokerContext(DbContextOptions<MessageBrokerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Message> Messages => Set<Message>();

    public virtual DbSet<Subscription> Subscriptions => Set<Subscription>();

    public virtual DbSet<Topic> Topics => Set<Topic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("message");

            entity.HasIndex(e => e.SubscriptionId, "message_subscription_SubscriptionId_idx");

            entity.Property(e => e.ExpiresAfter).HasColumnType("datetime");
            entity.Property(e => e.MessageStatus)
                .HasMaxLength(105)
                .HasDefaultValueSql("'NEW'");
            entity.Property(e => e.TopicMessage).HasMaxLength(200);

            entity.HasOne(d => d.Subscription).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("message_subscription_SubscriptionId");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("subscription");

            entity.HasIndex(e => e.TopicId, "topic_subscription_TopicId_idx");

            entity.Property(e => e.Name).HasMaxLength(45);

            entity.HasOne(d => d.Topic).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("topic_subscription_TopicId");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("topic");

            entity.Property(e => e.Name).HasMaxLength(45);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
