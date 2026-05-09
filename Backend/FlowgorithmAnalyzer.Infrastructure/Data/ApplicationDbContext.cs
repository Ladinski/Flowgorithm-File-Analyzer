using Microsoft.EntityFrameworkCore;
using FlowgorithmAnalyzer.Core.Models;

namespace FlowgorithmAnalyzer.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;
    public DbSet<AnalysisResult> AnalysisResults { get; set; } = null!;
    public DbSet<RiskIndicator> RiskIndicators { get; set; } = null!;
    public DbSet<SimilarityMatch> SimilarityMatches { get; set; } = null!;
    public DbSet<ForensicAnalysis> ForensicAnalyses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Student configuration
        modelBuilder.Entity<Student>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.StudentId)
            .IsUnique();
        modelBuilder.Entity<Student>()
            .HasMany(s => s.Submissions)
            .WithOne(sub => sub.Student)
            .HasForeignKey(sub => sub.StudentId);
        modelBuilder.Entity<Student>()
            .HasMany(s => s.AnalysisResults)
            .WithOne(ar => ar.Student)
            .HasForeignKey(ar => ar.StudentId);

        // Submission configuration
        modelBuilder.Entity<Submission>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Submission>()
            .HasMany(s => s.AnalysisResults)
            .WithOne(ar => ar.Submission)
            .HasForeignKey(ar => ar.SubmissionId);

        // AnalysisResult configuration
        modelBuilder.Entity<AnalysisResult>()
            .HasKey(ar => ar.Id);
        modelBuilder.Entity<AnalysisResult>()
            .HasMany(ar => ar.RiskIndicators)
            .WithOne(ri => ri.AnalysisResult)
            .HasForeignKey(ri => ri.AnalysisResultId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AnalysisResult>()
            .HasMany(ar => ar.SimilarityMatches)
            .WithOne(sm => sm.AnalysisResult)
            .HasForeignKey(sm => sm.AnalysisResultId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AnalysisResult>()
            .HasOne(ar => ar.ForensicAnalysis)
            .WithOne(fa => fa.AnalysisResult)
            .HasForeignKey<ForensicAnalysis>(fa => fa.AnalysisResultId)
            .OnDelete(DeleteBehavior.Cascade);

        // SimilarityMatch configuration
        modelBuilder.Entity<SimilarityMatch>()
            .HasKey(sm => sm.Id);
        modelBuilder.Entity<SimilarityMatch>()
            .HasOne(sm => sm.MatchedAnalysisResult)
            .WithMany()
            .HasForeignKey(sm => sm.MatchedAnalysisResultId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
