# EmuladorBook

## Sobre o Sistema

O **EmuladorBook** é uma ferramenta desktop desenvolvida em C# (.NET 8 Windows Forms) projetada para automatizar a rotina de desenvolvedores e analistas que trabalham com integrações de sistemas legados (Mainframe/COBOL).

O sistema realiza a leitura de programas COBOL (`.cbl`, `.cob`, `.txt`), extrai consultas SQL embarcadas (`EXEC SQL`), identifica variáveis de host para preenchimento dinâmico, executa a consulta no banco de dados e gera arquivos posicionais (`.txt`) baseados nas regras e máscaras das Copybooks (`.cpy`).

---

## Pré-requisitos e Variável de Ambiente

Por segurança e boas práticas de arquitetura, o sistema não armazena endereços de servidor no código-fonte. O caminho do banco de dados deve ser configurado através da variável de ambiente **`EMULADOR_BOOK_CONN`** no Windows.

A variável deve conter apenas os dados de conexão com o servidor e banco, **sem usuário e sem senha**, pois as credenciais são informadas diretamente na tela do sistema.

### Configurando para PostgreSQL
Abra o Prompt de Comando (CMD) ou PowerShell e execute:

```cmd
setx EMULADOR_BOOK_CONN "Host=localhost;Port=5433;Database=cobol_db"
```

### Configurando para IBM DB2 (OLE DB)
Abra o Prompt de Comando (CMD) ou PowerShell e execute:

```cmd
setx EMULADOR_BOOK_CONN "Provider=IBMDADB2;Database=MEUBANCO;Hostname=192.168.1.50;Port=50000;Protocol=TCPIP"
```

Após executar o comando `setx`, feche e abra novamente o Visual Studio ou o aplicativo para que o Windows reconheça a nova variável de ambiente.

---

## Passo a Passo de Uso (Fluxo COBOL)

### 1. Carregar o Programa COBOL
1. Abra o **EmuladorBook**.
2. Clique no botão **Programa** no canto superior esquerdo.
3. Selecione o arquivo de código-fonte COBOL (`.cbl`, `.cob` ou `.txt`).
4. O sistema listará automaticamente na coluna da esquerda todos os parágrafos que possuem consultas SQL válidas.

### 2. Autenticar no Banco de Dados
1. No topo da janela, preencha os campos **Usuário** e **Senha**.
2. Clique em **Testar Conexão** para validar a comunicação com o banco configurado na variável `EMULADOR_BOOK_CONN`.

### 3. Preencher Parâmetros e Executar a Query
1. Clique sobre o parágrafo desejado na lista da esquerda para visualizar a query SQL no painel central.
2. Caso a query possua variáveis de host do COBOL (como `:WS-CONTRATO`), elas aparecerão automaticamente na tabela à direita da query.
3. Digite os valores de teste na coluna **Valor (SQL)** (utilize aspas simples para textos, como `'CTR000000001'`, e apenas números para campos numéricos).
4. Clique em **Executar Query**. Os registros retornados serão exibidos na aba **Resultados da Query** e o botão **Book** será habilitado.

### 4. Carregar a Book e Revisar o De/Para
1. Clique no botão **Book** e selecione o arquivo de Copybook (`.cpy`, `.cob` ou `.txt`).
2. Na caixa de seleção ao lado do botão, escolha o layout desejado (registros contendo `DETALHE` são selecionados por padrão).
3. Na aba **De/Para (Book x Query)**, confira o vínculo automático feito entre cada campo `PIC` da Book e as colunas retornadas pela Query. Se necessário, altere a coluna de origem clicando na lista suspensa de cada linha.

### 5. Gerar o Arquivo Posicional
1. Clique no botão **Gerar Arquivo**.
2. Escolha o diretório e o nome do arquivo `.txt` de saída e clique em **Salvar**.
3. O sistema aplicará automaticamente todas as regras da Book (zeros à esquerda para números, espaços à direita para textos, formatação de casas decimais, separadores `;` e tamanhos de `FILLER`).

---

## Como Alternar entre PostgreSQL e DB2 no Código-Fonte

Para compilar uma versão voltada ao IBM DB2 (via `OleDb`) em vez do PostgreSQL (`Npgsql`), altere os métodos `ObterConnectionStringMontada` e `CriarConexao` no arquivo `MainForm.cs`:

### Versão PostgreSQL
```csharp
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

    return $"{connTrim};Username={usuario};Password={senha};";
}

private IDbConnection CriarConexao()
{
    string connString = ObterConnectionStringMontada();
    return new NpgsqlConnection(connString);
}
```

### Versão IBM DB2 (OLE DB)
```csharp
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

    return $"{connTrim};Uid={usuario};Pwd={senha};";
}

private IDbConnection CriarConexao()
{
    string connString = ObterConnectionStringMontada();
    return new OleDbConnection(connString);
}
```

---

## Estrutura de Diretórios

```text
EmuladorBook/
├── resources/
│   └── icone.ico
├── DTOs/
│   ├── CampoBook.cs
│   └── ParagrafoCobol.cs
├── Services/
│   ├── CobolParserService.cs
│   └── CopybookParserService.cs
├── MainForm.cs
├── Program.cs
└── EmuladorBook.csproj
```

---

## Compilação e Publicação (Release)

Para gerar o executável final em arquivo único (`SingleFile`) pronto para distribuição nas máquinas dos usuários, execute na pasta raiz do projeto:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None
```

O executável será disponibilizado em:
`bin/Release/net8.0-windows7.0/win-x64/publish/EmuladorBook.exe`