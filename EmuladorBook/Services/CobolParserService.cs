using EmuladorBook.DTOs;
using System.Text;
using System.Text.RegularExpressions;

namespace EmuladorBook.Services;

public class CobolParserService
{
    private static readonly HashSet<string> PalavrasReservadasCobol = new(StringComparer.OrdinalIgnoreCase)
    {
        "ACCEPT", "ADD", "ALTER", "CALL", "CANCEL", "CLOSE", "COMPUTE", "CONTINUE",
        "DELETE", "DISPLAY", "DIVIDE", "ENTRY", "EVALUATE", "EXIT", "GO", "GOBACK",
        "IF", "INITIALIZE", "INSPECT", "MERGE", "MOVE", "MULTIPLY", "OPEN", "PERFORM",
        "READ", "RELEASE", "RETURN", "REWRITE", "SEARCH", "SET", "SORT", "START",
        "STOP", "STRING", "SUBTRACT", "UNSTRING", "WRITE", "END-IF", "END-EVALUATE",
        "END-PERFORM", "END-READ", "END-WRITE", "END-SEARCH", "END-COMPUTE", "WHEN",
        "ELSE", "THEN", "NOT", "EXEC", "END-EXEC", "PROCEDURE", "DATA", "ENVIRONMENT",
        "IDENTIFICATION", "WORKING-STORAGE", "LINKAGE", "FILE", "LOCAL-STORAGE", "END-STRING"
    };

    private static readonly Regex RegexNomeParagrafo = new(
        @"^([A-Z0-9][A-Z0-9\-_]*)\s*(SECTION)?\s*\.(?:\s+EXIT\s*\.)?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public List<ParagrafoCobol> ExtrairParagrafosComQueryDeArquivo(string caminhoArquivo)
    {
        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException("Arquivo COBOL não encontrado.", caminhoArquivo);

        var linhas = File.ReadAllLines(caminhoArquivo, Encoding.UTF8);
        return ExtrairParagrafosComQueryDeLinhas(linhas);
    }

    private List<ParagrafoCobol> ExtrairParagrafosComQueryDeLinhas(string[] linhas)
    {
        var todosParagrafos = new List<ParagrafoCobol>();
        ParagrafoCobol? paragrafoAtual = null;
        bool dentroDaProcedureDivision = false;
        bool dentroDeExecSql = false;
        var bufferSql = new StringBuilder();

        bool arquivoPossuiProcedureDivision = linhas.Any(l =>
            l.Contains("PROCEDURE DIVISION", StringComparison.OrdinalIgnoreCase));

        if (!arquivoPossuiProcedureDivision)
        {
            dentroDaProcedureDivision = true;
        }

        for (int i = 0; i < linhas.Length; i++)
        {
            string linhaSemComentario = LimparLinhaMainframe(linhas[i], out bool ehComentario);

            if (ehComentario || string.IsNullOrWhiteSpace(linhaSemComentario))
                continue;

            string linhaTrim = linhaSemComentario.Trim();

            if (!dentroDaProcedureDivision)
            {
                if (linhaTrim.Contains("PROCEDURE DIVISION", StringComparison.OrdinalIgnoreCase))
                {
                    dentroDaProcedureDivision = true;
                }
                continue;
            }

            if (!dentroDeExecSql && EhCabecalhoDeParagrafo(linhaTrim, out string nomeParagrafo))
            {
                paragrafoAtual = new ParagrafoCobol
                {
                    NomeParagrafo = nomeParagrafo,
                    LinhaInicial = i + 1
                };
                todosParagrafos.Add(paragrafoAtual);
                continue;
            }

            int idxExecSql = IndexOfRegex(linhaSemComentario, @"EXEC\s+SQL");

            if (!dentroDeExecSql && idxExecSql >= 0)
            {
                dentroDeExecSql = true;
                bufferSql.Clear();

                if (paragrafoAtual == null)
                {
                    paragrafoAtual = new ParagrafoCobol
                    {
                        NomeParagrafo = "PARAGRAFO-PRINCIPAL",
                        LinhaInicial = i + 1
                    };
                    todosParagrafos.Add(paragrafoAtual);
                }

                string trecho = linhaSemComentario.Substring(idxExecSql);
                int idxEndExec = IndexOfRegex(trecho, @"END-EXEC");

                if (idxEndExec >= 0)
                {
                    string queryCompleta = FormatarBlocoSql(trecho.Substring(0, idxEndExec));
                    queryCompleta = RemoverVariaveisInto(queryCompleta);

                    if (EhQueryValida(queryCompleta))
                        paragrafoAtual.Queries.Add(queryCompleta);

                    dentroDeExecSql = false;
                }
                else
                {
                    bufferSql.AppendLine(trecho);
                }
                continue;
            }

            if (dentroDeExecSql)
            {
                int idxEndExec = IndexOfRegex(linhaSemComentario, @"END-EXEC");

                if (idxEndExec >= 0)
                {
                    string parteFinal = linhaSemComentario.Substring(0, idxEndExec);
                    if (!string.IsNullOrWhiteSpace(parteFinal))
                        bufferSql.AppendLine(parteFinal);

                    string queryCompleta = FormatarBlocoSql(bufferSql.ToString());
                    queryCompleta = RemoverVariaveisInto(queryCompleta);

                    if (EhQueryValida(queryCompleta))
                    {
                        if (paragrafoAtual != null)
                            paragrafoAtual.Queries.Add(queryCompleta);
                    }

                    dentroDeExecSql = false;
                    bufferSql.Clear();
                }
                else
                {
                    bufferSql.AppendLine(linhaSemComentario);
                }
            }
        }

        return todosParagrafos.Where(p => p.Queries.Count > 0).ToList();
    }

    private static string RemoverVariaveisInto(string sql)
    {
        return Regex.Replace(sql, @"\bINTO\b[\s\S]*?\bFROM\b", "FROM", RegexOptions.IgnoreCase);
    }

    private static string LimparLinhaMainframe(string linhaBruta, out bool ehComentario)
    {
        ehComentario = false;
        if (string.IsNullOrWhiteSpace(linhaBruta)) return string.Empty;

        string linha = linhaBruta;

        if (linha.Length >= 7 && LinhaSegueColunasMainframe(linha))
        {
            char indicador = linha[6];
            if (indicador == '*' || indicador == '/')
            {
                ehComentario = true;
                return string.Empty;
            }
            int limite = linha.Length >= 72 ? Math.Min(linha.Length, 72) : linha.Length;
            return linha.Substring(7, limite - 7);
        }

        string trim = linha.TrimStart();
        if (trim.StartsWith("*") || trim.StartsWith("/"))
        {
            ehComentario = true;
            return string.Empty;
        }

        return linha;
    }

    private static bool LinhaSegueColunasMainframe(string linha)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!char.IsDigit(linha[i]) && linha[i] != ' ')
                return false;
        }
        return true;
    }

    private static bool EhCabecalhoDeParagrafo(string linhaTrim, out string nomeParagrafo)
    {
        nomeParagrafo = string.Empty;
        var match = RegexNomeParagrafo.Match(linhaTrim);

        if (!match.Success) return false;

        string candidato = match.Groups[1].Value.ToUpperInvariant();

        if (PalavrasReservadasCobol.Contains(candidato)) return false;

        nomeParagrafo = candidato;
        return true;
    }

    private static int IndexOfRegex(string input, string pattern)
    {
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Index : -1;
    }

    private static string FormatarBlocoSql(string blocoBruto)
    {
        string semExecSql = Regex.Replace(blocoBruto, @"^\s*EXEC\s+SQL\b", "", RegexOptions.IgnoreCase);

        var linhas = semExecSql.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                               .SkipWhile(string.IsNullOrWhiteSpace)
                               .Reverse()
                               .SkipWhile(string.IsNullOrWhiteSpace)
                               .Reverse()
                               .ToList();

        if (linhas.Count == 0) return string.Empty;

        int menorIndentacao = linhas
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.TakeWhile(char.IsWhiteSpace).Count())
            .DefaultIfEmpty(0)
            .Min();

        var sb = new StringBuilder();

        foreach (var l in linhas)
        {
            if (string.IsNullOrWhiteSpace(l))
            {
                sb.AppendLine();
            }
            else
            {
                string linhaAlinhada = l.Length >= menorIndentacao ? l.Substring(menorIndentacao) : l.TrimStart();
                sb.AppendLine(linhaAlinhada.TrimEnd());
            }
        }

        return sb.ToString().Trim();
    }

    private static bool EhQueryValida(string sqlLimpo)
    {
        if (string.IsNullOrWhiteSpace(sqlLimpo)) return false;

        if (!Regex.IsMatch(sqlLimpo, @"^(SELECT|INSERT|UPDATE|DELETE)\b", RegexOptions.IgnoreCase))
            return false;

        return true;
    }
}