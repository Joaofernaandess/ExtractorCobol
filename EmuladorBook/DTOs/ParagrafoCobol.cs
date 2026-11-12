namespace EmuladorBook.DTOs;

public class ParagrafoCobol
{
    public string NomeParagrafo { get; set; } = string.Empty;
    public int LinhaInicial { get; set; }
    public List<string> Queries { get; set; } = new();

    public string ObterQueriesFormatadas()
    {
        if (Queries.Count == 1)
            return Queries[0];

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Queries.Count; i++)
        {
            sb.AppendLine($"-- [Query {i + 1} do parágrafo {NomeParagrafo}]");
            sb.AppendLine(Queries[i]);
            sb.AppendLine(new string('-', 60));
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    public override string ToString()
    {
        return $"{NomeParagrafo} ({Queries.Count} {(Queries.Count == 1 ? "query" : "queries")})";
    }
}