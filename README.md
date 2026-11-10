# SommusESales Desktop

## Pré-requisitos do Sistema

### Driver de Banco de Dados

Para que o integrador consiga ler os arquivos `.DBF` do Autosys, é obrigatória a instalação do driver Microsoft OLE DB Provider for Visual FoxPro 8.0 (`VFPOLEDB`).

O instalador do SommusESales executa a instalação do `VFPOLEDB` automaticamente. Se o driver não estiver instalado, o sistema pode apresentar o erro: `O provedor 'VFPOLEDB.1' não está registrado na máquina local`.

---

## Estrutura de Diretórios

O instalador deve ser executado preferencialmente apontando para a pasta raiz do Autosys, por exemplo `C:\SCI`. O instalador sugere esse caminho por padrão, mas permite selecionar outro diretório quando o ambiente do cliente utilizar uma raiz diferente.

A estrutura esperada após a instalação é:

```text
SCI/
├── DBF/
└── IntegradorSommusESales/
    └── SommusSalesColecta.exe
```

O executável permanece dentro da pasta `IntegradorSommusESales`, criada automaticamente pelo instalador. A pasta `DBF` fica um nível acima do diretório do executável e é o local preferencial de leitura dos arquivos `.DBF`.

Se a pasta `DBF` não existir, o integrador mantém compatibilidade com instalações antigas e tenta ler os `.DBF` diretamente da raiz selecionada.

---

## Configurações e Comportamento

### Inicialização com o Windows

A opção "Iniciar com o Windows" utiliza o Registro do Windows para persistir a preferência.

- **Chave:** `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- **Nome do valor:** `SommusSalesColecta`
- **Dados do valor:** caminho completo do executável

### Arquivos de Controle, Logs e Saída

O sistema gera arquivos auxiliares junto ao diretório do executável:

- `data/last_sync.txt`: data e hora da última sincronização realizada com sucesso
- `data/Arquivos_Envio/`: arquivos `.txt` gerados para integração com eSales
- `logs/`: logs diários de execução e sincronização

---

## Modo de uso

O fluxo de geração do executável e do instalador deve ser feito em duas etapas, seguindo o mesmo padrão do SommusDemander.

1. Gerar o release com o comando:

```bash
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:DebugType=None
```

O comando deve ser executado na pasta do projeto:

```powershell
cd D:\Sommus\SommusESales-desktop\SommusESales
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:DebugType=None
```

2. Após a publicação, gerar o instalador compilando o script [SommusESales/Instalador/Instalador.iss](SommusESales/Instalador/Instalador.iss).

O script do instalador está configurado para consumir os arquivos publicados em `SommusESales/bin/Release/net8.0-windows7.0/win-x86/publish`, portanto o `dotnet publish` deve ser executado antes da geração do instalador.

O instalador gerado deve ser executado selecionando a pasta raiz onde ficam os dados do Autosys. Em uma instalação padrão, selecione `C:\SCI`; em instalações customizadas, selecione a raiz equivalente usada pelo cliente.
