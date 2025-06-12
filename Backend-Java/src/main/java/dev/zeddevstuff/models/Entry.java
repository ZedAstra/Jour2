package dev.zeddevstuff.models;

import java.util.Objects;

public class Entry
{
    public int EntryId = 0;
    public String Name = "";
    public String Json = "";

    public Entry() {}
    public Entry(String name, String json)
    {
        this.EntryId = 0;
        this.Name = name;
        this.Json = json;
    }

    public boolean isNull()
    {
        return this.EntryId == 0 && Objects.equals(this.Name, "") && Objects.equals(this.Json, "");
    }
}
