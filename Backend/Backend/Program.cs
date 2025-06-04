using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;

using Jour2.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Jour2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddDbContext<AppDbContext>();
        builder.Services.AddAuthorization();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        builder.Services.AddOpenApi();
        

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseCors(opt =>
        {
            opt.AllowAnyHeader();
            opt.AllowAnyMethod();
            opt.AllowAnyOrigin();
        });
        app.UseAuthorization();

        Endpoints.Map(app);

        using(var db = new AppDbContext())
            ManageMigrations(db, AppDomain.CurrentDomain.BaseDirectory);

        app.Run();
    }

    public static void ManageMigrations(AppDbContext db, string root)
    {
        if (File.Exists($@"{root}\app.db"))
        {
            if(db.Database.GetPendingMigrations().Any())
            {
                BackupDatabase(root, db.Database.GetPendingMigrations().ToList());
                Console.WriteLine("Applying pending migrations...");
                db.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");
            }
            else
            {
                Console.WriteLine("Database up to date, starting server.");
            }
        }
        else
        {
            Console.WriteLine("Database not found, creating a new one...");
            db.Database.Migrate();
            Console.WriteLine("Database created successfully, starting server.");
        }
    }

    public static void BackupDatabase(string root, List<string> migrations)
    {
        Console.WriteLine("Backing up database...");
        FileInfo db = new($@"{root}\app.db");
        FileInfo shm = new($@"{root}\app.db-shm");
        FileInfo wal = new($@"{root}\app.db-wal");
        if(Directory.Exists($@"{root}\Backups") == false)
        {
            Directory.CreateDirectory($@"{root}\Backups");
        }
        using (ZipArchive archive = ZipFile.Open($@"{root}\Backups\app_{DateTime.Now:yyyyMMdd_HHmmss}.zip", ZipArchiveMode.Create))
        {
            if (db.Exists)
            {
                using (var writer = new BinaryWriter(archive.CreateEntry("app.db").Open()))
                {
                    File.Open(db.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).CopyTo(writer.BaseStream);
                }
            }
            if (shm.Exists)
            {
                using (var writer = new BinaryWriter(archive.CreateEntry("app.db-shm").Open()))
                {
                    File.Open(shm.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).CopyTo(writer.BaseStream);
                }
            }
            if (wal.Exists)
            {
                using (var writer = new BinaryWriter(archive.CreateEntry("app.db-wal").Open()))
                {
                    File.Open(wal.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).CopyTo(writer.BaseStream);
                }
            }
            using(var entryStream = archive.CreateEntry("migrations", CompressionLevel.Optimal).Open())
            {
                entryStream.Write(System.Text.Encoding.UTF8.GetBytes(string.Join("\n", migrations)));
            }
        }
        Console.WriteLine("Backup completed.");
    }
}
