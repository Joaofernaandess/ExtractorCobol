using ExtractorCobol.DTOs;
using ExtractorCobol.Services;
using Npgsql;
using System.Data;
using System.Text.RegularExpressions;

namespace ExtractorCobol;

public partial class MainForm : Form
{
    private const string NomeVariavelAmbiente = "EXTRACTOR_DB_CONN";

    private Panel pnlTopo = null!;
    private Button btnSelecionarArquivo = null!;
    private Label lblArquivoAtual = null!;
    private Label lblUsuario = null!;
    private TextBox txtUsuario = null!;
    private Label lblSenha = null!;
    private TextBox txtSenha = null!;
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
    private Button btnBook = null!;
    private ComboBox cmbLayoutBook = null!;
    private Button btnGerarArquivo = null!;
    private Label lblBookAtual = null!;
    private TabControl tabInferior = null!;
    private TabPage tabResultados = null!;
    private TabPage tabMapeamento = null!;
    private DataGridView dgvResultados = null!;
    private DataGridView dgvMapeamento = null!;
    private Label lblStatus = null!;
    private System.ComponentModel.IContainer? components = null;

    private readonly CobolParserService _parserService = new();
    private readonly CopybookParserService _bookService = new();
    private DataTable? _ultimoResultadoQuery = null;
    private List<RegistroBook> _registrosBookCarregados = new();

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
            Title = "Selecione o programa COBOL",
            Filter = "Arquivos COBOL (*.cbl;*.cob;*.txt)|*.cbl;*.cob;*.txt|Todos os arquivos (*.*)|*.*"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            CarregarArquivoCobol(dialog.FileName);
        }
    }

    private string ObterConnectionStringMontada()
    {
        string? baseConn = Environment.GetEnvironmentVariable(NomeVariavelAmbiente);

        if (string.IsNullOrWhiteSpace(baseConn))
        {
            throw new InvalidOperationException(
                $"A variável de ambiente '{NomeVariavelAmbiente}' não foi encontrada no sistema.");
        }

        string connTrim = baseConn.TrimEnd(';', ' ');
        string usuario = txtUsuario.Text.Trim();
        string senha = txtSenha.Text;

        // POSTGRESQL
        return $"{connTrim};Username={usuario};Password={senha};";

        // DB2
        // return $"{connTrim};Uid={usuario};Pwd={senha};";
    }

    private IDbConnection CriarConexao()
    {
        string connString = ObterConnectionStringMontada();

        // POSTGRESQL
        return new NpgsqlConnection(connString);

        // DB2
        // return new OleDbConnection(connString);
    }

    private bool ValidarCamposLogin()
    {
        if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            MessageBox.Show("Informe o usuário e a senha do banco de dados no topo da tela.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private void BtnTestarConexao_Click(object? sender, EventArgs e)
    {
        if (!ValidarCamposLogin())
            return;

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

        if (!ValidarCamposLogin())
            return;

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
            _ultimoResultadoQuery = dt;

            bool possuiResultados = dt.Rows.Count > 0;
            btnBook.Enabled = possuiResultados;

            if (possuiResultados && cmbLayoutBook.SelectedItem is RegistroBook layout)
            {
                MontarGridMapeamento(layout);
            }

            lblStatus.Text = $"Query executada com sucesso! {dt.Rows.Count} linha(s) retornada(s).";
        }
        catch (Exception ex)
        {
            ResetarEstadoBook();
            MessageBox.Show($"Erro ao executar a query:\n\n{ex.Message}", "Erro SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Falha ao executar a query.";
        }
    }

    private void BtnBook_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Selecione o arquivo de Book (Copybook)",
            Filter = "Arquivos Copybook (*.cpy;*.cob;*.txt)|*.cpy;*.cob;*.txt|Todos os arquivos (*.*)|*.*"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            _registrosBookCarregados = _bookService.ExtrairRegistrosDaBook(dialog.FileName);

            cmbLayoutBook.Items.Clear();
            foreach (var reg in _registrosBookCarregados)
            {
                cmbLayoutBook.Items.Add(reg);
            }

            if (_registrosBookCarregados.Count > 0)
            {
                int idxDetalhe = _registrosBookCarregados.FindIndex(r =>
                    r.NomeRegistro.Contains("DETALHE", StringComparison.OrdinalIgnoreCase));

                cmbLayoutBook.SelectedIndex = idxDetalhe >= 0 ? idxDetalhe : _registrosBookCarregados.Count - 1;
                cmbLayoutBook.Enabled = true;
                btnGerarArquivo.Enabled = _ultimoResultadoQuery != null && _ultimoResultadoQuery.Rows.Count > 0;
                lblBookAtual.Text = $"Book: {Path.GetFileName(dialog.FileName)}";
                lblStatus.Text = "Book carregada. Revise o mapeamento na aba 'De/Para'.";
                tabInferior.SelectedTab = tabMapeamento;
            }
            else
            {
                cmbLayoutBook.Enabled = false;
                btnGerarArquivo.Enabled = false;
                lblBookAtual.Text = "Nenhuma Book válida.";
                dgvMapeamento.Rows.Clear();
                MessageBox.Show("Não foram encontrados campos PIC válidos no arquivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao ler a Book:\n\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CmbLayoutBook_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_ultimoResultadoQuery != null && cmbLayoutBook.SelectedItem is RegistroBook layout)
        {
            MontarGridMapeamento(layout);
        }
    }

    private void MontarGridMapeamento(RegistroBook layout)
    {
        dgvMapeamento.Rows.Clear();
        dgvMapeamento.Columns.Clear();

        var colCampo = new DataGridViewTextBoxColumn
        {
            HeaderText = "Campo na Book (Destino)",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        };
        dgvMapeamento.Columns.Add(colCampo);

        var listaColunas = new List<string> { "" };
        foreach (DataColumn col in _ultimoResultadoQuery!.Columns)
        {
            listaColunas.Add(col.ColumnName);
        }

        var colQuery = new DataGridViewComboBoxColumn
        {
            HeaderText = "Coluna da Query (Origem)",
            DataSource = listaColunas,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        };
        dgvMapeamento.Columns.Add(colQuery);

        var mapeamentoAuto = _bookService.GerarMapeamentoInicial(layout.Campos, _ultimoResultadoQuery.Columns);
        var camposDados = layout.Campos.Where(c => !c.EhFiller && !c.EhSeparador).ToList();

        foreach (var campo in camposDados)
        {
            int rowIndex = dgvMapeamento.Rows.Add();
            var row = dgvMapeamento.Rows[rowIndex];
            row.Tag = campo;
            row.Cells[0].Value = $"{campo.Nome} ({campo.PicBruto})";

            string sugerido = mapeamentoAuto.ContainsKey(campo) ? mapeamentoAuto[campo] : "";
            row.Cells[1].Value = sugerido;
        }
    }

    private void BtnGerarArquivo_Click(object? sender, EventArgs e)
    {
        if (_ultimoResultadoQuery == null || _ultimoResultadoQuery.Rows.Count == 0)
        {
            MessageBox.Show("Execute uma query que retorne dados antes de gerar o arquivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cmbLayoutBook.SelectedItem is not RegistroBook registroSelecionado)
        {
            MessageBox.Show("Selecione um layout da Book para gerar o arquivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var saveDialog = new SaveFileDialog
        {
            Title = "Salvar arquivo gerado pela Book",
            Filter = "Arquivo de Texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*",
            FileName = $"saida_{registroSelecionado.NomeRegistro.ToLowerInvariant()}.txt"
        };

        if (saveDialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            var mapeamentoUsuario = new Dictionary<CampoBook, string>();

            foreach (DataGridViewRow row in dgvMapeamento.Rows)
            {
                if (row.Tag is CampoBook campo)
                {
                    string colunaSelecionada = row.Cells[1].Value?.ToString() ?? "";
                    mapeamentoUsuario[campo] = colunaSelecionada;
                }
            }

            _bookService.GerarArquivoDeResultados(_ultimoResultadoQuery, registroSelecionado, saveDialog.FileName, mapeamentoUsuario);
            lblStatus.Text = $"Arquivo gerado com sucesso em: {saveDialog.FileName}";
            MessageBox.Show($"Arquivo gerado com sucesso!\n\nCaminho: {saveDialog.FileName}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao gerar arquivo:\n\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ResetarEstadoBook()
    {
        _ultimoResultadoQuery = null;
        btnBook.Enabled = false;
        cmbLayoutBook.Enabled = false;
        btnGerarArquivo.Enabled = false;
        dgvMapeamento.Rows.Clear();
        dgvMapeamento.Columns.Clear();
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
        ResetarEstadoBook();

        lblArquivoAtual.Text = $"Programa: {nomeArquivo}";

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
            lblStatus.Text = "Nenhum parágrafo com query SQL suportada foi encontrado neste programa.";
        }
    }

    private void LstParagrafos_SelectedIndexChanged(object? sender, EventArgs e)
    {
        dgvParametros.Rows.Clear();
        dgvResultados.DataSource = null;
        ResetarEstadoBook();

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
        lblUsuario = new Label();
        txtUsuario = new TextBox();
        lblSenha = new Label();
        txtSenha = new TextBox();
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
        btnBook = new Button();
        cmbLayoutBook = new ComboBox();
        btnGerarArquivo = new Button();
        lblBookAtual = new Label();
        tabInferior = new TabControl();
        tabResultados = new TabPage();
        dgvResultados = new DataGridView();
        tabMapeamento = new TabPage();
        dgvMapeamento = new DataGridView();
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
        tabInferior.SuspendLayout();
        tabResultados.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
        tabMapeamento.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMapeamento).BeginInit();
        SuspendLayout();

        btnSelecionarArquivo.BackColor = Color.FromArgb(240, 240, 240);
        btnSelecionarArquivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnSelecionarArquivo.Location = new Point(12, 12);
        btnSelecionarArquivo.Size = new Size(160, 32);
        btnSelecionarArquivo.Text = "Programa";
        btnSelecionarArquivo.UseVisualStyleBackColor = false;
        btnSelecionarArquivo.Click += BtnSelecionarArquivo_Click;

        lblArquivoAtual.AutoSize = true;
        lblArquivoAtual.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
        lblArquivoAtual.ForeColor = Color.DimGray;
        lblArquivoAtual.Location = new Point(180, 20);
        lblArquivoAtual.Text = "Nenhum programa selecionado.";

        lblUsuario.AutoSize = true;
        lblUsuario.Location = new Point(12, 60);
        lblUsuario.Text = "Usuário:";

        txtUsuario.Location = new Point(70, 56);
        txtUsuario.Size = new Size(180, 23);

        lblSenha.AutoSize = true;
        lblSenha.Location = new Point(265, 60);
        lblSenha.Text = "Senha:";

        txtSenha.Location = new Point(315, 56);
        txtSenha.Size = new Size(180, 23);
        txtSenha.PasswordChar = '*';

        btnTestarConexao.BackColor = Color.FromArgb(230, 245, 255);
        btnTestarConexao.Font = new Font("Segoe UI", 9F);
        btnTestarConexao.Location = new Point(510, 54);
        btnTestarConexao.Size = new Size(120, 26);
        btnTestarConexao.Text = "Testar Conexão";
        btnTestarConexao.UseVisualStyleBackColor = false;
        btnTestarConexao.Click += BtnTestarConexao_Click;

        pnlTopo.Controls.Add(btnSelecionarArquivo);
        pnlTopo.Controls.Add(lblArquivoAtual);
        pnlTopo.Controls.Add(lblUsuario);
        pnlTopo.Controls.Add(txtUsuario);
        pnlTopo.Controls.Add(lblSenha);
        pnlTopo.Controls.Add(txtSenha);
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
        txtQuery.ReadOnly = true;
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

        btnBook.Enabled = false;
        btnBook.BackColor = Color.FromArgb(240, 240, 240);
        btnBook.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnBook.Location = new Point(146, 6);
        btnBook.Size = new Size(90, 30);
        btnBook.Text = "Book";
        btnBook.UseVisualStyleBackColor = false;
        btnBook.Click += BtnBook_Click;

        cmbLayoutBook.Enabled = false;
        cmbLayoutBook.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLayoutBook.Location = new Point(244, 10);
        cmbLayoutBook.Size = new Size(200, 23);
        cmbLayoutBook.SelectedIndexChanged += CmbLayoutBook_SelectedIndexChanged;

        btnGerarArquivo.Enabled = false;
        btnGerarArquivo.BackColor = Color.FromArgb(220, 245, 220);
        btnGerarArquivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnGerarArquivo.Location = new Point(452, 6);
        btnGerarArquivo.Size = new Size(130, 30);
        btnGerarArquivo.Text = "Gerar Arquivo";
        btnGerarArquivo.UseVisualStyleBackColor = false;
        btnGerarArquivo.Click += BtnGerarArquivo_Click;

        lblBookAtual.AutoSize = true;
        lblBookAtual.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblBookAtual.ForeColor = Color.DimGray;
        lblBookAtual.Location = new Point(592, 13);
        lblBookAtual.Text = "Nenhuma Book selecionada.";

        pnlAcoesQuery.Controls.Add(btnExecutarQuery);
        pnlAcoesQuery.Controls.Add(btnBook);
        pnlAcoesQuery.Controls.Add(cmbLayoutBook);
        pnlAcoesQuery.Controls.Add(btnGerarArquivo);
        pnlAcoesQuery.Controls.Add(lblBookAtual);
        pnlAcoesQuery.Dock = DockStyle.Top;
        pnlAcoesQuery.Height = 42;

        tabInferior.Dock = DockStyle.Fill;
        tabResultados.Text = "Resultados da Query";
        tabMapeamento.Text = "De/Para (Book x Query)";

        dgvResultados.Dock = DockStyle.Fill;
        dgvResultados.AllowUserToAddRows = false;
        dgvResultados.AllowUserToDeleteRows = false;
        dgvResultados.ReadOnly = true;
        dgvResultados.BackgroundColor = Color.White;
        dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

        dgvMapeamento.Dock = DockStyle.Fill;
        dgvMapeamento.AllowUserToAddRows = false;
        dgvMapeamento.AllowUserToDeleteRows = false;
        dgvMapeamento.RowHeadersVisible = false;
        dgvMapeamento.BackgroundColor = Color.WhiteSmoke;

        tabResultados.Controls.Add(dgvResultados);
        tabMapeamento.Controls.Add(dgvMapeamento);
        tabInferior.Controls.Add(tabResultados);
        tabInferior.Controls.Add(tabMapeamento);

        splitDireita.Dock = DockStyle.Fill;
        splitDireita.Orientation = Orientation.Horizontal;
        splitDireita.SplitterDistance = 250;
        splitDireita.Panel1.Controls.Add(grpQuery);
        splitDireita.Panel2.Controls.Add(tabInferior);
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
        lblStatus.Text = "Pronto. Selecione um programa COBOL para extrair as consultas.";

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
        Text = "EmuladorBook - Executor de Queries";

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
        pnlAcoesQuery.PerformLayout();
        tabInferior.ResumeLayout(false);
        tabResultados.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
        tabMapeamento.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvMapeamento).EndInit();
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