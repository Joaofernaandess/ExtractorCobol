namespace EmuladorBook.DTOs;

public class RegistroBook
{
    public string NomeRegistro { get; set; } = string.Empty;
    public List<CampoBook> Campos { get; set; } = new();

    public override string ToString() => $"{NomeRegistro} ({Campos.Count} campos)";
}

public class CampoBook
{
    public int Nivel { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string PicBruto { get; set; } = string.Empty;
    public char Tipo { get; set; }
    public int TamanhoTotal { get; set; }
    public int CasasDecimais { get; set; }
    public bool PossuiVirgulaExplicita { get; set; }

    public bool EhSeparador => Nome.StartsWith("SEP-", StringComparison.OrdinalIgnoreCase) && TamanhoTotal == 1;
    public bool EhFiller => Nome.Equals("FILLER", StringComparison.OrdinalIgnoreCase);
}