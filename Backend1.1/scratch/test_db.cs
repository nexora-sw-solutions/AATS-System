using System;
using Npgsql;

try
{
    var connString = "postgresql://postgres:0yDN3Tn0J4JadSb6]@db.kihytvpkjgjrhvulzbdn.supabase.co:5432/postgres";
    using var conn = new NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("Connection successful!");
}
catch (Exception ex)
{
    Console.WriteLine("Connection failed: " + ex.Message);
}
