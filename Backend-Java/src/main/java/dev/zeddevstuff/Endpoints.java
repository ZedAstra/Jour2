package dev.zeddevstuff;

import dev.zeddevstuff.models.Entry;
import io.javalin.Javalin;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.Workbook;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;

import java.sql.Connection;
import java.sql.DriverManager;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

public class Endpoints
{
    public static void setup(Javalin app)
    {
        app.post("/api/upload", ctx ->
        {
            var file = ctx.uploadedFiles().stream().findFirst();
            if(file.isEmpty())
            {
                ctx.status(400).result("No file uploaded");
            }
            else
            {
                try
                {
                    Workbook workbook = new XSSFWorkbook(file.get().content());
                    ArrayList<ArrayList<String>> data = new ArrayList<>();
                    for(var worksheet : workbook)
                    {
                        for (Iterator<Row> it = worksheet.rowIterator(); it.hasNext(); )
                        {
                            var col = it.next();
                            ArrayList<String> rowData = new ArrayList<>();
                            for (int i = 0; i < col.getLastCellNum(); i++)
                            {
                                var cell = col.getCell(i);
                                if (cell != null)
                                {
                                    rowData.add(cell.toString());
                                }
                            }
                            data.add(rowData);
                        }
                    }
                    try
                    {
                        var fileName = file.get().filename();
                        fileName = fileName.substring(0, fileName.lastIndexOf('.')); // Remove file extension
                        Entry entry = getEntryByName(fileName);
                        entry.Json = ctx.jsonMapper().toJsonString(data, data.getClass());
                        upsertEntry(entry);
                        ctx.status(202)
                            .json(data);
                    }
                    catch (Exception e)
                    {
                        ctx.status(500).result("Sqlite error: " + e.getMessage());
                    }
                }
                catch (Exception discarded)
                {
                    System.out.println("Error processing file: " + discarded.getMessage());
                    ctx.status(400)
                        .result("File is either corrupted or not an Excel file");
                }
            }
        });

        app.get("/api/list", ctx ->
        {
            List<String> entries = getAllEntries().stream()
                .map(entry -> entry.Name)
                .toList();
            ctx.json(entries);
        });

        app.get("/api/entry/{name}", ctx ->
        {
            String name = ctx.pathParam("name");
            Entry entry = getEntryByName(name);
            if(entry.isNull())
                ctx.status(404)
                    .result("Entry not found");
            else ctx.result(entry.Json);
        });
    }

    static List<Entry> getAllEntries()
    {
        List<Entry> entries = new ArrayList<>();
        try(Connection connection = DriverManager.getConnection("jdbc:sqlite:app.db");
            var preparedStatement = connection.prepareStatement("SELECT * FROM Entries"))
        {
            try(var resultSet = preparedStatement.executeQuery())
            {
                while(resultSet.next())
                {
                    Entry entry = new Entry();
                    entry.EntryId = resultSet.getInt("EntryId");
                    entry.Name = resultSet.getString("Name");
                    entry.Json = resultSet.getString("Json");
                    entries.add(entry);
                }
            }
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
        return entries;
    }

    static Entry getEntryByName(String name)
    {
        try(Connection connection = DriverManager.getConnection("jdbc:sqlite:app.db");
            var preparedStatement = connection.prepareStatement("SELECT * FROM Entries WHERE Name = ?"))
        {
            preparedStatement.setString(1, name);
            try(var resultSet = preparedStatement.executeQuery())
            {
                if(resultSet.next())
                {
                    Entry entry = new Entry();
                    entry.EntryId = resultSet.getInt("EntryId");
                    entry.Name = resultSet.getString("Name");
                    entry.Json = resultSet.getString("Json");
                    return entry;
                }
                else
                {
                    return new Entry(name, "");  // No entry found
                }
            }
        }
        catch (Exception e)
        {
            e.printStackTrace();
            return new Entry(name, ""); // Error occurred
        }
    }

    static boolean upsertEntry(Entry entry)
    {
        try(Connection connection = DriverManager.getConnection("jdbc:sqlite:app.db");
            var preparedStatement = connection.prepareStatement("""
            Insert Into Entries (Name, Json) Values (?, ?)
            ON CONFLICT(Name)
            DO
                UPDATE SET Json = ?
                WHERE Name = ?
            """))
        {
            preparedStatement.setString(1, entry.Name);
            preparedStatement.setString(2, entry.Json);
            preparedStatement.setString(3, entry.Json);
            preparedStatement.setString(4, entry.Name);
            return preparedStatement.executeUpdate() > 0;
        }
        catch (Exception e)
        {
            e.printStackTrace();
            return false; // Error occurred
        }
    }
}
