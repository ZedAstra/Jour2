using ClosedXML.Excel;

using DocumentFormat.OpenXml.Office2010.ExcelAc;

using Jour2.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Jour2;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("api/upload", async (IFormFile file, AppDbContext db) =>
        {
            try
            {
                List<List<string>> data = [];
                using XLWorkbook workbook = new(file.OpenReadStream());
                foreach (var worksheet in workbook.Worksheets)
                {
                    foreach (var col in worksheet.RowsUsed())
                    {
                        List<string> rowData = [];
                        foreach (var cell in col.CellsUsed())
                        {
                            rowData.Add(cell.GetFormattedString());
                        }
                        data.Add(rowData);
                    }
                }
                Entry? existing = db.Entries.Where(entry => entry.Name == file.FileName).FirstOrDefault();
                if(existing is not null)
                {
                    existing.Json = JsonSerializer.Serialize(data);
                    db.Update(existing);
                }
                else
                {
                    await db.Entries.AddAsync(new Entry()
                    {
                        Name = Path.GetFileNameWithoutExtension(file.FileName),
                        Json = JsonSerializer.Serialize(data)
                    });
                }
                await db.SaveChangesAsync();
                return Results.Accepted(value: data);
            }
            catch (Exception)
            {
                return Results.BadRequest("File is either corrupted or not an Excel file");
            }
        })
            .WithName("UploadFile")
            .WithSummary("Upload an Excel file")
            .WithDescription("Uploads an Excel file and processes it.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<List<List<string>>>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .DisableAntiforgery();

        app.MapGet("/api/list", (AppDbContext db) => Results.Ok(db.Entries.Select(x => x.Name).ToList()))
            .WithName("ListEntries")
            .WithSummary("Lists all processed entries by name")
            .WithDescription("Lists all processed entries by name")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/entry/{name}", async (string name, AppDbContext db) =>
        {             
            var entry = await db.Entries
                .Where(entry => entry.Name == name)
                .FirstOrDefaultAsync();
            return entry == null
                ? Results.NotFound($"Entry with name '{name}' not found.")
                : Results.Ok(JsonSerializer.Deserialize<List<List<string>>>(entry.Json));
        })
            .WithName("GetEntryByName")
            .WithSummary("Retrieves an entry by its name")
            .WithDescription("Retrieves an entry by its name")
            .Produces<List<List<string>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
