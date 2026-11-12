using EmuladorBook.DTOs;
using EmuladorBook.Services;
using Npgsql;
using System.Data;
using System.Text.RegularExpressions;

namespace EmuladorBook;

public partial class MainForm : Form
{
    private const string NomeVariavelAmbiente = "EMULADOR_BOOK_CONN";

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
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
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
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        tabInferior = new TabControl();
        tabResultados = new TabPage();
        dgvResultados = new DataGridView();
        tabMapeamento = new TabPage();
        dgvMapeamento = new DataGridView();
        pnlAcoesQuery = new Panel();
        btnExecutarQuery = new Button();
        btnBook = new Button();
        cmbLayoutBook = new ComboBox();
        btnGerarArquivo = new Button();
        lblBookAtual = new Label();
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
        tabInferior.SuspendLayout();
        tabResultados.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
        tabMapeamento.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMapeamento).BeginInit();
        pnlAcoesQuery.SuspendLayout();
        SuspendLayout();
        // 
        // pnlTopo
        // 
        pnlTopo.Controls.Add(btnSelecionarArquivo);
        pnlTopo.Controls.Add(lblArquivoAtual);
        pnlTopo.Controls.Add(lblUsuario);
        pnlTopo.Controls.Add(txtUsuario);
        pnlTopo.Controls.Add(lblSenha);
        pnlTopo.Controls.Add(txtSenha);
        pnlTopo.Controls.Add(btnTestarConexao);
        pnlTopo.Dock = DockStyle.Top;
        pnlTopo.Location = new Point(0, 0);
        pnlTopo.Name = "pnlTopo";
        pnlTopo.Size = new Size(1100, 100);
        pnlTopo.TabIndex = 1;
        // 
        // btnSelecionarArquivo
        // 
        btnSelecionarArquivo.BackColor = Color.FromArgb(240, 240, 240);
        btnSelecionarArquivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnSelecionarArquivo.Location = new Point(12, 12);
        btnSelecionarArquivo.Name = "btnSelecionarArquivo";
        btnSelecionarArquivo.Size = new Size(160, 32);
        btnSelecionarArquivo.TabIndex = 0;
        btnSelecionarArquivo.Text = "Programa";
        btnSelecionarArquivo.UseVisualStyleBackColor = false;
        btnSelecionarArquivo.Click += BtnSelecionarArquivo_Click;
        // 
        // lblArquivoAtual
        // 
        lblArquivoAtual.AutoSize = true;
        lblArquivoAtual.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
        lblArquivoAtual.ForeColor = Color.DimGray;
        lblArquivoAtual.Location = new Point(180, 20);
        lblArquivoAtual.Name = "lblArquivoAtual";
        lblArquivoAtual.Size = new Size(187, 17);
        lblArquivoAtual.TabIndex = 1;
        lblArquivoAtual.Text = "Nenhum programa selecionado.";
        // 
        // lblUsuario
        // 
        lblUsuario.AutoSize = true;
        lblUsuario.Location = new Point(12, 60);
        lblUsuario.Name = "lblUsuario";
        lblUsuario.Size = new Size(50, 15);
        lblUsuario.TabIndex = 2;
        lblUsuario.Text = "Usuário:";
        // 
        // txtUsuario
        // 
        txtUsuario.Location = new Point(70, 56);
        txtUsuario.Name = "txtUsuario";
        txtUsuario.Size = new Size(180, 23);
        txtUsuario.TabIndex = 3;
        // 
        // lblSenha
        // 
        lblSenha.AutoSize = true;
        lblSenha.Location = new Point(265, 60);
        lblSenha.Name = "lblSenha";
        lblSenha.Size = new Size(42, 15);
        lblSenha.TabIndex = 4;
        lblSenha.Text = "Senha:";
        // 
        // txtSenha
        // 
        txtSenha.Location = new Point(315, 56);
        txtSenha.Name = "txtSenha";
        txtSenha.PasswordChar = '*';
        txtSenha.Size = new Size(180, 23);
        txtSenha.TabIndex = 5;
        // 
        // btnTestarConexao
        // 
        btnTestarConexao.BackColor = Color.FromArgb(230, 245, 255);
        btnTestarConexao.Font = new Font("Segoe UI", 9F);
        btnTestarConexao.Location = new Point(510, 54);
        btnTestarConexao.Name = "btnTestarConexao";
        btnTestarConexao.Size = new Size(120, 26);
        btnTestarConexao.TabIndex = 6;
        btnTestarConexao.Text = "Testar Conexão";
        btnTestarConexao.UseVisualStyleBackColor = false;
        btnTestarConexao.Click += BtnTestarConexao_Click;
        // 
        // splitPrincipal
        // 
        splitPrincipal.Dock = DockStyle.Fill;
        splitPrincipal.Location = new Point(0, 100);
        splitPrincipal.Name = "splitPrincipal";
        // 
        // splitPrincipal.Panel1
        // 
        splitPrincipal.Panel1.Controls.Add(grpParagrafos);
        splitPrincipal.Panel1.Padding = new Padding(10, 0, 5, 5);
        // 
        // splitPrincipal.Panel2
        // 
        splitPrincipal.Panel2.Controls.Add(splitDireita);
        splitPrincipal.Panel2.Padding = new Padding(5, 0, 10, 5);
        splitPrincipal.Size = new Size(1100, 522);
        splitPrincipal.SplitterDistance = 330;
        splitPrincipal.TabIndex = 0;
        // 
        // grpParagrafos
        // 
        grpParagrafos.Controls.Add(lstParagrafos);
        grpParagrafos.Dock = DockStyle.Fill;
        grpParagrafos.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grpParagrafos.Location = new Point(10, 0);
        grpParagrafos.Name = "grpParagrafos";
        grpParagrafos.Padding = new Padding(8);
        grpParagrafos.Size = new Size(315, 517);
        grpParagrafos.TabIndex = 0;
        grpParagrafos.TabStop = false;
        grpParagrafos.Text = "Parágrafos (0)";
        // 
        // lstParagrafos
        // 
        lstParagrafos.Dock = DockStyle.Fill;
        lstParagrafos.Font = new Font("Consolas", 10F);
        lstParagrafos.IntegralHeight = false;
        lstParagrafos.ItemHeight = 15;
        lstParagrafos.Location = new Point(8, 25);
        lstParagrafos.Name = "lstParagrafos";
        lstParagrafos.Size = new Size(299, 484);
        lstParagrafos.TabIndex = 0;
        lstParagrafos.SelectedIndexChanged += LstParagrafos_SelectedIndexChanged;
        // 
        // splitDireita
        // 
        splitDireita.Dock = DockStyle.Fill;
        splitDireita.Location = new Point(5, 0);
        splitDireita.Name = "splitDireita";
        splitDireita.Orientation = Orientation.Horizontal;
        // 
        // splitDireita.Panel1
        // 
        splitDireita.Panel1.Controls.Add(grpQuery);
        // 
        // splitDireita.Panel2
        // 
        splitDireita.Panel2.Controls.Add(tabInferior);
        splitDireita.Panel2.Controls.Add(pnlAcoesQuery);
        splitDireita.Size = new Size(751, 517);
        splitDireita.SplitterDistance = 367;
        splitDireita.TabIndex = 0;
        // 
        // grpQuery
        // 
        grpQuery.Controls.Add(splitQueryParam);
        grpQuery.Dock = DockStyle.Fill;
        grpQuery.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grpQuery.Location = new Point(0, 0);
        grpQuery.Name = "grpQuery";
        grpQuery.Padding = new Padding(8);
        grpQuery.Size = new Size(751, 367);
        grpQuery.TabIndex = 0;
        grpQuery.TabStop = false;
        grpQuery.Text = "Query SQL";
        // 
        // splitQueryParam
        // 
        splitQueryParam.Dock = DockStyle.Fill;
        splitQueryParam.Location = new Point(8, 25);
        splitQueryParam.Name = "splitQueryParam";
        // 
        // splitQueryParam.Panel1
        // 
        splitQueryParam.Panel1.Controls.Add(txtQuery);
        // 
        // splitQueryParam.Panel2
        // 
        splitQueryParam.Panel2.Controls.Add(dgvParametros);
        splitQueryParam.Size = new Size(735, 334);
        splitQueryParam.SplitterDistance = 592;
        splitQueryParam.TabIndex = 0;
        // 
        // txtQuery
        // 
        txtQuery.BackColor = Color.FromArgb(30, 30, 30);
        txtQuery.Dock = DockStyle.Fill;
        txtQuery.Font = new Font("Consolas", 11F);
        txtQuery.ForeColor = Color.FromArgb(220, 220, 170);
        txtQuery.Location = new Point(0, 0);
        txtQuery.Name = "txtQuery";
        txtQuery.ReadOnly = true;
        txtQuery.Size = new Size(592, 334);
        txtQuery.TabIndex = 0;
        txtQuery.Text = "";
        txtQuery.WordWrap = false;
        txtQuery.TextChanged += txtQuery_TextChanged;
        // 
        // dgvParametros
        // 
        dgvParametros.AllowUserToAddRows = false;
        dgvParametros.AllowUserToDeleteRows = false;
        dgvParametros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvParametros.BackgroundColor = Color.WhiteSmoke;
        dgvParametros.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2 });
        dgvParametros.Dock = DockStyle.Fill;
        dgvParametros.Location = new Point(0, 0);
        dgvParametros.Name = "dgvParametros";
        dgvParametros.RowHeadersVisible = false;
        dgvParametros.Size = new Size(139, 334);
        dgvParametros.TabIndex = 0;
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewTextBoxColumn1.HeaderText = "Variável (COBOL)";
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        // 
        // dataGridViewTextBoxColumn2
        // 
        dataGridViewTextBoxColumn2.HeaderText = "Valor (SQL)";
        dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        // 
        // tabInferior
        // 
        tabInferior.Controls.Add(tabResultados);
        tabInferior.Controls.Add(tabMapeamento);
        tabInferior.Dock = DockStyle.Fill;
        tabInferior.Location = new Point(0, 42);
        tabInferior.Name = "tabInferior";
        tabInferior.SelectedIndex = 0;
        tabInferior.Size = new Size(751, 104);
        tabInferior.TabIndex = 0;
        // 
        // tabResultados
        // 
        tabResultados.Controls.Add(dgvResultados);
        tabResultados.Location = new Point(4, 24);
        tabResultados.Name = "tabResultados";
        tabResultados.Size = new Size(743, 76);
        tabResultados.TabIndex = 0;
        tabResultados.Text = "Resultados da Query";
        // 
        // dgvResultados
        // 
        dgvResultados.AllowUserToAddRows = false;
        dgvResultados.AllowUserToDeleteRows = false;
        dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvResultados.BackgroundColor = Color.White;
        dgvResultados.Dock = DockStyle.Fill;
        dgvResultados.Location = new Point(0, 0);
        dgvResultados.Name = "dgvResultados";
        dgvResultados.ReadOnly = true;
        dgvResultados.Size = new Size(743, 76);
        dgvResultados.TabIndex = 0;
        // 
        // tabMapeamento
        // 
        tabMapeamento.Controls.Add(dgvMapeamento);
        tabMapeamento.Location = new Point(4, 24);
        tabMapeamento.Name = "tabMapeamento";
        tabMapeamento.Size = new Size(673, 76);
        tabMapeamento.TabIndex = 1;
        tabMapeamento.Text = "De/Para (Book x Query)";
        // 
        // dgvMapeamento
        // 
        dgvMapeamento.AllowUserToAddRows = false;
        dgvMapeamento.AllowUserToDeleteRows = false;
        dgvMapeamento.BackgroundColor = Color.WhiteSmoke;
        dgvMapeamento.Dock = DockStyle.Fill;
        dgvMapeamento.Location = new Point(0, 0);
        dgvMapeamento.Name = "dgvMapeamento";
        dgvMapeamento.RowHeadersVisible = false;
        dgvMapeamento.Size = new Size(673, 76);
        dgvMapeamento.TabIndex = 0;
        // 
        // pnlAcoesQuery
        // 
        pnlAcoesQuery.Controls.Add(btnExecutarQuery);
        pnlAcoesQuery.Controls.Add(btnBook);
        pnlAcoesQuery.Controls.Add(cmbLayoutBook);
        pnlAcoesQuery.Controls.Add(btnGerarArquivo);
        pnlAcoesQuery.Controls.Add(lblBookAtual);
        pnlAcoesQuery.Dock = DockStyle.Top;
        pnlAcoesQuery.Location = new Point(0, 0);
        pnlAcoesQuery.Name = "pnlAcoesQuery";
        pnlAcoesQuery.Size = new Size(751, 42);
        pnlAcoesQuery.TabIndex = 1;
        // 
        // btnExecutarQuery
        // 
        btnExecutarQuery.Enabled = false;
        btnExecutarQuery.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnExecutarQuery.Location = new Point(8, 6);
        btnExecutarQuery.Name = "btnExecutarQuery";
        btnExecutarQuery.Size = new Size(130, 30);
        btnExecutarQuery.TabIndex = 0;
        btnExecutarQuery.Text = "Executar Query";
        btnExecutarQuery.Click += BtnExecutarQuery_Click;
        // 
        // btnBook
        // 
        btnBook.BackColor = Color.FromArgb(240, 240, 240);
        btnBook.Enabled = false;
        btnBook.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnBook.Location = new Point(146, 6);
        btnBook.Name = "btnBook";
        btnBook.Size = new Size(90, 30);
        btnBook.TabIndex = 1;
        btnBook.Text = "Book";
        btnBook.UseVisualStyleBackColor = false;
        btnBook.Click += BtnBook_Click;
        // 
        // cmbLayoutBook
        // 
        cmbLayoutBook.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLayoutBook.Enabled = false;
        cmbLayoutBook.Location = new Point(244, 10);
        cmbLayoutBook.Name = "cmbLayoutBook";
        cmbLayoutBook.Size = new Size(200, 23);
        cmbLayoutBook.TabIndex = 2;
        cmbLayoutBook.SelectedIndexChanged += CmbLayoutBook_SelectedIndexChanged;
        // 
        // btnGerarArquivo
        // 
        btnGerarArquivo.BackColor = Color.FromArgb(220, 245, 220);
        btnGerarArquivo.Enabled = false;
        btnGerarArquivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnGerarArquivo.Location = new Point(452, 6);
        btnGerarArquivo.Name = "btnGerarArquivo";
        btnGerarArquivo.Size = new Size(130, 30);
        btnGerarArquivo.TabIndex = 3;
        btnGerarArquivo.Text = "Gerar Arquivo";
        btnGerarArquivo.UseVisualStyleBackColor = false;
        btnGerarArquivo.Click += BtnGerarArquivo_Click;
        // 
        // lblBookAtual
        // 
        lblBookAtual.AutoSize = true;
        lblBookAtual.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblBookAtual.ForeColor = Color.DimGray;
        lblBookAtual.Location = new Point(592, 13);
        lblBookAtual.Name = "lblBookAtual";
        lblBookAtual.Size = new Size(156, 15);
        lblBookAtual.TabIndex = 4;
        lblBookAtual.Text = "Nenhuma Book selecionada.";
        // 
        // lblStatus
        // 
        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.Location = new Point(0, 622);
        lblStatus.Name = "lblStatus";
        lblStatus.Padding = new Padding(10, 0, 0, 0);
        lblStatus.Size = new Size(1100, 28);
        lblStatus.TabIndex = 2;
        lblStatus.Text = "Pronto. Selecione um programa COBOL para extrair as consultas.";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // MainForm
        // 
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
        grpParagrafos.ResumeLayout(false);
        splitDireita.Panel1.ResumeLayout(false);
        splitDireita.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitDireita).EndInit();
        splitDireita.ResumeLayout(false);
        grpQuery.ResumeLayout(false);
        splitQueryParam.Panel1.ResumeLayout(false);
        splitQueryParam.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitQueryParam).EndInit();
        splitQueryParam.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvParametros).EndInit();
        tabInferior.ResumeLayout(false);
        tabResultados.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
        tabMapeamento.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvMapeamento).EndInit();
        pnlAcoesQuery.ResumeLayout(false);
        pnlAcoesQuery.PerformLayout();
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

    private void txtQuery_TextChanged(object sender, EventArgs e)
    {

    }
}