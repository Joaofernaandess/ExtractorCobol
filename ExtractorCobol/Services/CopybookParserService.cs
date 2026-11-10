using ExtractorCobol.DTOs;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtractorCobol.Services;

public class CopybookParserService
{
    private static readonly Regex RegexItemCobol = new(
        @"^(?<nivel>\d{2})\s+(?<nome>[A-Z0-9\-_]+)(?:\s+REDEFINES\s+[A-Z0-9\-_]+)?(?:\s+PIC\s+(?<pic>[^\.\s]+))?\s*\.",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public List<RegistroBook> ExtrairRegistrosDaBook(string caminhoArquivo)
    {
        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException("Arquivo de Book não encontrado.", caminhoArquivo);

        var linhas = File.ReadAllLines(caminhoArquivo, Encoding.UTF8);
        var registros = new List<RegistroBook>();
        RegistroBook? registroAtual = null;

        foreach (var linhaBruta in linhas)
        {
            string linha = LimparLinhaCobol(linhaBruta);
            if (string.IsNullOrWhiteSpace(linha))
                continue;

            var match = RegexItemCobol.Match(linha.Trim());
            if (!match.Success)
                continue;

            int nivel = int.Parse(match.Groups["nivel"].Value);
            string nome = match.Groups["nome"].Value.ToUpperInvariant();
            string pic = match.Groups["pic"].Value;

            if (nivel == 1)
                continue;

            if (string.IsNullOrWhiteSpace(pic))
            {
                registroAtual = new RegistroBook { NomeRegistro = nome };
                registros.Add(registroAtual);
                continue;
            }

            if (registroAtual == null)
            {
                registroAtual = new RegistroBook { NomeRegistro = "REGISTRO-PADRAO" };
                registros.Add(registroAtual);
            }

            var campo = InterpretarPic(nivel, nome, pic);
            registroAtual.Campos.Add(campo);
        }

        return registros.Where(r => r.Campos.Count > 0).ToList();
    }

    public Dictionary<CampoBook, string> GerarMapeamentoInicial(List<CampoBook> camposBook, DataColumnCollection colunasBanco)
    {
        var mapa = new Dictionary<CampoBook, string>();
        var colunasUsadas = new HashSet<string>();

        var camposDados = camposBook.Where(c => !c.EhFiller && !c.EhSeparador).ToList();

        foreach (var campo in camposDados)
        {
            string nomeCampoNormalizado = NormalizarNome(campo.Nome);
            string colunaEncontrada = string.Empty;

            foreach (DataColumn coluna in colunasBanco)
            {
                if (colunasUsadas.Contains(coluna.ColumnName))
                    continue;

                string nomeColunaNormalizado = NormalizarNome(coluna.ColumnName);
                if (nomeCampoNormalizado == nomeColunaNormalizado ||
                    nomeColunaNormalizado.EndsWith(nomeCampoNormalizado) ||
                    nomeCampoNormalizado.EndsWith(nomeColunaNormalizado))
                {
                    colunaEncontrada = coluna.ColumnName;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(colunaEncontrada))
            {
                mapa[campo] = colunaEncontrada;
                colunasUsadas.Add(colunaEncontrada);
            }
        }

        int proximoIndiceSequencial = 0;
        foreach (var campo in camposDados)
        {
            if (mapa.ContainsKey(campo))
                continue;

            while (proximoIndiceSequencial < colunasBanco.Count && colunasUsadas.Contains(colunasBanco[proximoIndiceSequencial].ColumnName))
            {
                proximoIndiceSequencial++;
            }

            if (proximoIndiceSequencial < colunasBanco.Count)
            {
                string nomeCol = colunasBanco[proximoIndiceSequencial].ColumnName;
                mapa[campo] = nomeCol;
                colunasUsadas.Add(nomeCol);
            }
            else
            {
                mapa[campo] = string.Empty;
            }
        }

        return mapa;
    }

    public void GerarArquivoDeResultados(DataTable tabelaResultados, RegistroBook layoutRegistro, string caminhoSaida, Dictionary<CampoBook, string> mapeamento)
    {
        var sb = new StringBuilder();

        foreach (DataRow row in tabelaResultados.Rows)
        {
            var linhaArquivo = new StringBuilder();

            foreach (var campo in layoutRegistro.Campos)
            {
                if (campo.EhFiller)
                {
                    linhaArquivo.Append(new string(' ', campo.TamanhoTotal));
                    continue;
                }

                if (campo.EhSeparador)
                {
                    linhaArquivo.Append(';');
                    continue;
                }

                object? valorBanco = null;
                Type? tipoColuna = null;

                if (mapeamento.TryGetValue(campo, out string? nomeColuna) && !string.IsNullOrEmpty(nomeColuna) && tabelaResultados.Columns.Contains(nomeColuna))
                {
                    valorBanco = row[nomeColuna];
                    tipoColuna = tabelaResultados.Columns[nomeColuna].DataType;
                }

                string valorFormatado = FormatarValorCampo(campo, valorBanco, tipoColuna);
                linhaArquivo.Append(valorFormatado);
            }

            sb.AppendLine(linhaArquivo.ToString().TrimEnd());
        }

        File.WriteAllText(caminhoSaida, sb.ToString(), Encoding.UTF8);
    }

    private static string NormalizarNome(string nome)
    {
        string semPrefixo = Regex.Replace(nome, @"^(H-|WS-|IND-|CTR_)", "", RegexOptions.IgnoreCase);
        return semPrefixo.Replace("-", "").Replace("_", "").ToUpperInvariant();
    }

    private static string FormatarValorCampo(CampoBook campo, object? valor, Type? tipoColunaBanco)
    {
        bool nuloOuVazio = valor == null || valor == DBNull.Value || string.IsNullOrWhiteSpace(valor.ToString());

        if (campo.PossuiVirgulaExplicita)
        {
            decimal valorDecimal = 0m;
            if (!nuloOuVazio)
            {
                decimal.TryParse(valor!.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out valorDecimal);
            }

            int parteInteiraTam = campo.TamanhoTotal - campo.CasasDecimais - 1;
            string formato = new string('0', Math.Max(1, parteInteiraTam)) + "." + new string('0', campo.CasasDecimais);
            string formatado = valorDecimal.ToString(formato, CultureInfo.InvariantCulture).Replace('.', ',');

            if (formatado.Length > campo.TamanhoTotal)
                return formatado.Substring(formatado.Length - campo.TamanhoTotal);

            return formatado.PadLeft(campo.TamanhoTotal, '0');
        }

        bool colunaBancoEhNumerica = tipoColunaBanco == typeof(int) ||
                                     tipoColunaBanco == typeof(long) ||
                                     tipoColunaBanco == typeof(short) ||
                                     tipoColunaBanco == typeof(decimal) ||
                                     tipoColunaBanco == typeof(double) ||
                                     tipoColunaBanco == typeof(float);

        if (campo.Tipo == '9' || colunaBancoEhNumerica)
        {
            if (nuloOuVazio)
                return new string('0', campo.TamanhoTotal);

            string texto = valor!.ToString()!.Trim();
            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal num))
            {
                texto = Math.Truncate(Math.Abs(num)).ToString("0", CultureInfo.InvariantCulture);
            }

            if (texto.Length > campo.TamanhoTotal)
                return texto.Substring(0, campo.TamanhoTotal);

            return texto.PadLeft(campo.TamanhoTotal, '0');
        }

        if (nuloOuVazio)
            return new string(' ', campo.TamanhoTotal);

        string strValor = valor is DateTime dt
            ? dt.ToString("yyyy-MM-dd")
            : valor!.ToString()!;

        if (strValor.Length > campo.TamanhoTotal)
            return strValor.Substring(0, campo.TamanhoTotal);

        return strValor.PadRight(campo.TamanhoTotal, ' ');
    }

    private static CampoBook InterpretarPic(int nivel, string nome, string picBruto)
    {
        string pic = picBruto.ToUpperInvariant();
        var campo = new CampoBook
        {
            Nivel = nivel,
            Nome = nome,
            PicBruto = picBruto
        };

        if (pic.Contains(','))
        {
            campo.Tipo = '9';
            campo.PossuiVirgulaExplicita = true;
            var partes = pic.Split(',');
            int inteiros = CalcularTamanhoSimples(partes[0]);
            int decimais = partes.Length > 1 ? CalcularTamanhoSimples(partes[1]) : 0;
            campo.CasasDecimais = decimais;
            campo.TamanhoTotal = inteiros + 1 + decimais;
            return campo;
        }

        campo.Tipo = pic.StartsWith("9") || pic.StartsWith("S9") ? '9' : 'X';
        campo.TamanhoTotal = CalcularTamanhoSimples(pic);
        return campo;
    }

    private static int CalcularTamanhoSimples(string expressaoPic)
    {
        var matchParenteses = Regex.Match(expressaoPic, @"\((\d+)\)");
        if (matchParenteses.Success)
        {
            return int.Parse(matchParenteses.Groups[1].Value);
        }

        return expressaoPic.Count(c => c == '9' || c == 'X' || c == 'A');
    }

    private static string LimparLinhaCobol(string linhaBruta)
    {
        if (string.IsNullOrWhiteSpace(linhaBruta)) return string.Empty;

        if (linhaBruta.Length >= 7)
        {
            char indicador = linhaBruta[6];
            if (indicador == '*' || indicador == '/')
                return string.Empty;

            int limite = linhaBruta.Length >= 72 ? Math.Min(linhaBruta.Length, 72) : linhaBruta.Length;
            if (linhaBruta.Take(6).All(c => char.IsDigit(c) || c == ' '))
                return linhaBruta.Substring(7, limite - 7);
        }

        string trim = linhaBruta.TrimStart();
        if (trim.StartsWith("*") || trim.StartsWith("/"))
            return string.Empty;

        return linhaBruta;
    }
}