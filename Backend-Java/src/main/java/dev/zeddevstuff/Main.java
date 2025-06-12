package dev.zeddevstuff;

import io.javalin.Javalin;
import io.javalin.json.JavalinJackson;
import io.javalin.plugin.bundled.CorsPluginConfig;
import net.sf.persism.Session;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.Statement;
import java.util.Optional;

public class Main
{
    public static void main(String[] args)
    {
        try(Connection conn = DriverManager.getConnection("jdbc:sqlite:app.db"))
        {
            conn.createStatement().execute("""
                CREATE TABLE IF NOT EXISTS Entries (
                    EntryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    Json TEXT NOT NULL
                );
                """);
            Javalin app = Javalin.create(config ->
            {
                config.jsonMapper(new JavalinJackson());
                config.bundledPlugins.enableCors(cors ->
                {
                    cors.addRule(CorsPluginConfig.CorsRule::anyHost);
                });
            });
            Endpoints.setup(app);
            app.start(5000);
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
}