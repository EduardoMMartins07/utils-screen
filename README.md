# Controle rápido

Aplicativo desktop para Windows que reúne controles de volume e brilho em uma janela compacta. Ele foi pensado para quem usa o notebook conectado a uma televisão ou a outra tela e quer enxergar e ajustar esses níveis com facilidade, inclusive à distância.

## Recursos

- **Volume:** ajusta de 0% a 100% o volume geral da saída de áudio padrão do Windows. O app mostra o nome do dispositivo usado.
- **Brilho:** ajusta de 0% a 100% o brilho da tela integrada do notebook.
- **Faders visíveis:** as faixas têm maior espessura e um indicador destacado para facilitar a leitura do nível atual.
- **Temas:** começa em modo escuro e oferece a opção de alternar para modo claro. A escolha fica salva para a próxima abertura.
- **Aplicação imediata:** mover um fader altera o controle correspondente no Windows sem precisar confirmar.
- **Mensagens de disponibilidade:** se o Windows não conseguir acessar o áudio ou não oferecer controle de brilho compatível, o controle correspondente é desativado e uma mensagem é exibida.

## Requisitos e compatibilidade

- Windows 10 ou 11.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) para executar ou compilar o projeto.
- O ajuste de brilho depende de o notebook e o driver da tela oferecerem suporte ao controle de brilho do Windows. Monitores externos não são o alvo deste app.

O projeto usa WPF para a janela e NAudio para acessar o volume da saída padrão. O brilho é consultado e ajustado pela interface WMI do Windows.

## Executar durante o desenvolvimento

Abra o PowerShell na pasta do projeto e execute:

```powershell
dotnet run
```

Na primeira execução, o .NET restaura automaticamente as dependências NuGet definidas no projeto. Também é possível compilar sem abrir o app:

```powershell
dotnet build -c Release
```

## Publicar um executável

Para gerar uma publicação independente para Windows x64, execute:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

O executável será criado em:

```text
bin/Release/net8.0-windows/win-x64/publish/ScreenControls.exe
```

Como a publicação é independente, ela inclui o runtime necessário e não exige que o computador de destino tenha o .NET instalado.

## Preferência de tema

A preferência é salva no arquivo:

```text
%LOCALAPPDATA%\ScreenControls\settings.json
```

Na primeira abertura, antes de existir uma preferência salva, o app usa o modo escuro.
