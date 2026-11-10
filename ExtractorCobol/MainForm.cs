using ExtractorCobol.DTOs;
using ExtractorCobol.Services;
using Npgsql;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;

namespace ExtractorCobol;

public partial class MainForm : Form
{
    private Panel pnlTopo = null!;
    private Button btnSelecionarArquivo = null!;
    private Label lblArquivoAtual = null!;
    private Label lblTipoBanco = null!;
    private ComboBox cmbTipoBanco = null!;
    private Label lblStringConexao = null!;
    private TextBox txtConnectionString = null!;
    private Button btnTestarConexao = null!;
    private SplitContainer splitPrincipal = null!;
    private GroupBox grpParagrafos = null!;
    private ListBox lstParagrafos = null!;
    private SplitContainer splitDireita = null!;
    private GroupBox grpQuery = null!;
    private SplitContainer splitQueryParam = null!;
    private RichTextBox txtQuery = null!;
    private DataGridView dgvParametros = null!;
    private Panel pnlAcoesQuery = null!;
    private Button btnExecutarQuery = null!;
    private DataGridView dgvResultados = null!;
    private Label lblStatus = null!;
    private System.ComponentModel.IContainer? components = null;

    private readonly CobolParserService _parserService = new();

    public MainForm()
    {
        ConfigurarIcone();
        InitializeComponent();
    }

    private void ConfigurarIcone()
    {
        try
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "icone.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }
        }
        catch { }
    }

    private void BtnSelecionarArquivo_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Selecione o arquivo COBOL",
            Filter = "Arquivos COBOL (*.cbl;*.cob;*.txt)|*.cbl;*.cob;*.txt|Todos os arquivos (*.*)|*.*"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            CarregarArquivoCobol(dialog.FileName);
        }
    }

    private IDbConnection CriarConexao()
    {
        if (cmbTipoBanco.SelectedItem != null && cmbTipoBanco.SelectedItem.ToString() == "PostgreSQL")
        {
            return new NpgsqlConnection(txtConnectionString.Text);
        }

        return new OleDbConnection(txtConnectionString.Text);
    }

    private void BtnTestarConexao_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
        {
            MessageBox.Show("Informe a string de conexão primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var conn = CriarConexao();
            conn.Open();
            MessageBox.Show("Conexão estabelecida com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Falha ao conectar:\n\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnExecutarQuery_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtQuery.Text))
        {
            MessageBox.Show("Nenhuma query selecionada para executar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
        {
            MessageBox.Show("Informe a string de conexão no topo da tela antes de executar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            lblStatus.Text = "Executando query no banco de dados...";
            Application.DoEvents();

            string queryProntaParaRodar = txtQuery.Text;

            foreach (DataGridViewRow row in dgvParametros.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    string nomeParam = row.Cells[0].Value.ToString()!;
                    string valorParam = row.Cells[1].Value?.ToString() ?? "NULL";

                    if (string.IsNullOrWhiteSpace(valorParam))
                    {
                        valorParam = "NULL";
                    }

                    string padrao = Regex.Escape(nomeParam) + @"\b";
                    queryProntaParaRodar = Regex.Replace(queryProntaParaRodar, padrao, valorParam, RegexOptions.IgnoreCase);
                }
            }

            using var conn = CriarConexao();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = queryProntaParaRodar;

            using var reader = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);

            dgvResultados.DataSource = dt;
            lblStatus.Text = $"Query executada com sucesso! {dt.Rows.Count} linha(s) retornada(s).";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao executar a query:\n\n{ex.Message}", "Erro SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Falha ao executar a query.";
        }
    }

    private void CarregarArquivoCobol(string caminhoArquivo)
    {
        try
        {
            var paragrafos = _parserService.ExtrairParagrafosComQueryDeArquivo(caminhoArquivo);
            PreencherListaParagrafos(paragrafos, Path.GetFileName(caminhoArquivo));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao ler o arquivo COBOL: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PreencherListaParagrafos(List<ParagrafoCobol> paragrafos, string nomeArquivo)
    {
        lstParagrafos.Items.Clear();
        txtQuery.Clear();
        dgvParametros.Rows.Clear();
        dgvResultados.DataSource = null;

        lblArquivoAtual.Text = $"Arquivo: {nomeArquivo}";

        foreach (var p in paragrafos)
        {
            lstParagrafos.Items.Add(p);
        }

        grpParagrafos.Text = $"Parágrafos ({paragrafos.Count})";

        if (paragrafos.Count > 0)
        {
            lstParagrafos.SelectedIndex = 0;
            lblStatus.Text = $"Foram encontrados {paragrafos.Count} parágrafo(s) contendo comandos SQL.";
        }
        else
        {
            lblStatus.Text = "Nenhum parágrafo com query SQL suportada foi encontrado neste arquivo.";
        }
    }

    private void LstParagrafos_SelectedIndexChanged(object? sender, EventArgs e)
    {
        dgvParametros.Rows.Clear();
        dgvResultados.DataSource = null;

        if (lstParagrafos.SelectedItem is ParagrafoCobol paragrafoSelecionado)
        {
            grpQuery.Text = $"{paragrafoSelecionado.NomeParagrafo} (Linha {paragrafoSelecionado.LinhaInicial})";
            txtQuery.Text = paragrafoSelecionado.ObterQueriesFormatadas();
            btnExecutarQuery.Enabled = true;

            var matches = Regex.Matches(txtQuery.Text, @":[\w\-\.]+");
            var parametrosEncontrados = new HashSet<string>();

            foreach (Match match in matches)
            {
                parametrosEncontrados.Add(match.Value);
            }

            foreach (var param in parametrosEncontrados)
            {
                dgvParametros.Rows.Add(param, "");
            }
        }
        else
        {
            grpQuery.Text = "Query SQL";
            txtQuery.Clear();
            btnExecutarQuery.Enabled = false;
        }
    }

    private void InitializeComponent()
    {
        pnlTopo = new Panel();
        btnSelecionarArquivo = new Button();
        lblArquivoAtual = new Label();
        lblTipoBanco = new Label();
        cmbTipoBanco = new ComboBox();
        lblStringConexao = new Label();
        txtConnectionString = new TextBox();
        btnTestarConexao = new Button();
        splitPrincipal = new SplitContainer();
        grpParagrafos = new GroupBox();
        lstParagrafos = new ListBox();
        splitDireita = new SplitContainer();
        grpQuery = new GroupBox();
        splitQueryParam = new SplitContainer();
        txtQuery = new RichTextBox();
        dgvParametros = new DataGridView();
        pnlAcoesQuery = new Panel();
        btnExecutarQuery = new Button();
        dgvResultados = new DataGridView();
        lblStatus = new Label();
        pnlTopo.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitPrincipal).BeginInit();
        splitPrincipal.Panel1.SuspendLayout();
        splitPrincipal.Panel2.SuspendLayout();
        splitPrincipal.SuspendLayout();
        grpParagrafos.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitDireita).BeginInit();
        splitDireita.Panel1.SuspendLayout();
        splitDireita.Panel2.SuspendLayout();
        splitDireita.SuspendLayout();
        grpQuery.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitQueryParam).BeginInit();
        splitQueryParam.Panel1.SuspendLayout();
        splitQueryParam.Panel2.SuspendLayout();
        splitQueryParam.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvParametros).BeginInit();
        pnlAcoesQuery.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
        SuspendLayout();

        btnSelecionarArquivo.BackColor = Color.FromArgb(240, 240, 240);
        btnSelecionarArquivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnSelecionarArquivo.Location = new Point(12, 12);
        btnSelecionarArquivo.Size = new Size(160, 32);
        btnSelecionarArquivo.Text = "Arquivo COBOL";
        btnSelecionarArquivo.UseVisualStyleBackColor = false;
        btnSelecionarArquivo.Click += BtnSelecionarArquivo_Click;

        lblArquivoAtual.AutoSize = true;
        lblArquivoAtual.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
        lblArquivoAtual.ForeColor = Color.DimGray;
        lblArquivoAtual.Location = new Point(180, 20);
        lblArquivoAtual.Text = "Nenhum arquivo selecionado.";

        lblTipoBanco.AutoSize = true;
        lblTipoBanco.Location = new Point(12, 60);
        lblTipoBanco.Text = "Banco:";

        cmbTipoBanco.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTipoBanco.Items.AddRange(new object[] { "PostgreSQL", "OLEDB (DB2, SQL Server)" });
        cmbTipoBanco.SelectedIndex = 0;
        cmbTipoBanco.Location = new Point(60, 56);
        cmbTipoBanco.Size = new Size(160, 23);

        lblStringConexao.AutoSize = true;
        lblStringConexao.Location = new Point(230, 60);
        lblStringConexao.Text = "Conexão:";

        txtConnectionString.Location = new Point(290, 56);
        txtConnectionString.Size = new Size(380, 23);
        txtConnectionString.Text = string.Empty;

        btnTestarConexao.BackColor = Color.FromArgb(230, 245, 255);
        btnTestarConexao.Font = new Font("Segoe UI", 9F);
        btnTestarConexao.Location = new Point(680, 54);
        btnTestarConexao.Size = new Size(120, 26);
        btnTestarConexao.Text = "Testar Conexão";
        btnTestarConexao.UseVisualStyleBackColor = false;
        btnTestarConexao.Click += BtnTestarConexao_Click;

        pnlTopo.Controls.Add(btnSelecionarArquivo);
        pnlTopo.Controls.Add(lblArquivoAtual);
        pnlTopo.Controls.Add(lblTipoBanco);
        pnlTopo.Controls.Add(cmbTipoBanco);
        pnlTopo.Controls.Add(lblStringConexao);
        pnlTopo.Controls.Add(txtConnectionString);
        pnlTopo.Controls.Add(btnTestarConexao);
        pnlTopo.Dock = DockStyle.Top;
        pnlTopo.Height = 100;

        lstParagrafos.Dock = DockStyle.Fill;
        lstParagrafos.Font = new Font("Consolas", 10F);
        lstParagrafos.IntegralHeight = false;
        lstParagrafos.SelectedIndexChanged += LstParagrafos_SelectedIndexChanged;

        grpParagrafos.Controls.Add(lstParagrafos);
        grpParagrafos.Dock = DockStyle.Fill;
        grpParagrafos.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grpParagrafos.Padding = new Padding(8);
        grpParagrafos.Text = "Parágrafos (0)";

        txtQuery.BackColor = Color.FromArgb(30, 30, 30);
        txtQuery.ForeColor = Color.FromArgb(220, 220, 170);
        txtQuery.Dock = DockStyle.Fill;
        txtQuery.Font = new Font("Consolas", 11F);
        txtQuery.ReadOnly = false;
        txtQuery.WordWrap = false;

        dgvParametros.Dock = DockStyle.Fill;
        dgvParametros.AllowUserToAddRows = false;
        dgvParametros.AllowUserToDeleteRows = false;
        dgvParametros.RowHeadersVisible = false;
        dgvParametros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvParametros.BackgroundColor = Color.WhiteSmoke;
        dgvParametros.Columns.Add("Parametro", "Variável (COBOL)");
        dgvParametros.Columns[0].ReadOnly = true;
        dgvParametros.Columns.Add("Valor", "Valor (SQL)");

        splitQueryParam.Dock = DockStyle.Fill;
        splitQueryParam.Orientation = Orientation.Vertical;
        splitQueryParam.SplitterDistance = 500;
        splitQueryParam.Panel1.Controls.Add(txtQuery);
        splitQueryParam.Panel2.Controls.Add(dgvParametros);

        grpQuery.Controls.Add(splitQueryParam);
        grpQuery.Dock = DockStyle.Fill;
        grpQuery.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grpQuery.Padding = new Padding(8);
        grpQuery.Text = "Query SQL";

        btnExecutarQuery.Enabled = false;
        btnExecutarQuery.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnExecutarQuery.Location = new Point(8, 6);
        btnExecutarQuery.Size = new Size(130, 30);
        btnExecutarQuery.Text = "Executar Query";
        btnExecutarQuery.Click += BtnExecutarQuery_Click;

        pnlAcoesQuery.Controls.Add(btnExecutarQuery);
        pnlAcoesQuery.Dock = DockStyle.Top;
        pnlAcoesQuery.Height = 42;

        dgvResultados.Dock = DockStyle.Fill;
        dgvResultados.AllowUserToAddRows = false;
        dgvResultados.AllowUserToDeleteRows = false;
        dgvResultados.ReadOnly = true;
        dgvResultados.BackgroundColor = Color.White;
        dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

        splitDireita.Dock = DockStyle.Fill;
        splitDireita.Orientation = Orientation.Horizontal;
        splitDireita.SplitterDistance = 250;
        splitDireita.Panel1.Controls.Add(grpQuery);
        splitDireita.Panel2.Controls.Add(dgvResultados);
        splitDireita.Panel2.Controls.Add(pnlAcoesQuery);

        splitPrincipal.Dock = DockStyle.Fill;
        splitPrincipal.Location = new Point(0, 100);
        splitPrincipal.Size = new Size(1000, 522);
        splitPrincipal.SplitterDistance = 300;
        splitPrincipal.Panel1.Padding = new Padding(10, 0, 5, 5);
        splitPrincipal.Panel1.Controls.Add(grpParagrafos);
        splitPrincipal.Panel2.Padding = new Padding(5, 0, 10, 5);
        splitPrincipal.Panel2.Controls.Add(splitDireita);

        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.Height = 28;
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        lblStatus.Padding = new Padding(10, 0, 0, 0);
        lblStatus.Text = "Pronto. Selecione um arquivo COBOL para extrair as consultas.";

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(1100, 650);
        Controls.Add(splitPrincipal);
        Controls.Add(pnlTopo);
        Controls.Add(lblStatus);
        MinimumSize = new Size(950, 500);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ExtractorCobol - Executor de Queries";

        pnlTopo.ResumeLayout(false);
        pnlTopo.PerformLayout();
        splitPrincipal.Panel1.ResumeLayout(false);
        splitPrincipal.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitPrincipal).EndInit();
        splitPrincipal.ResumeLayout(false);
        splitDireita.Panel1.ResumeLayout(false);
        splitDireita.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitDireita).EndInit();
        splitDireita.ResumeLayout(false);
        grpParagrafos.ResumeLayout(false);
        grpQuery.ResumeLayout(false);
        splitQueryParam.Panel1.ResumeLayout(false);
        splitQueryParam.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitQueryParam).EndInit();
        splitQueryParam.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvParametros).EndInit();
        pnlAcoesQuery.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
        ResumeLayout(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }
}