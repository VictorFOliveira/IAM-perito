# Executar IAM-PERITO no Windows

Pré-requisitos:
1. Windows 10/11 x64.
2. SDK .NET 10 e Windows Desktop workload (WPF).
3. Android SDK Platform Tools (ADB) instalado e disponível no PATH.
4. Android próprio ou autorizado, com depuração USB habilitada e computador autorizado.

No PowerShell:

```powershell
git clone https://github.com/VictorFOliveira/IAM-perito.git
cd IAM-perito
dotnet build IAM.Perito.slnx
dotnet run --project src/IAM.Perito.Desktop
```

Conecte o dispositivo por USB e execute `adb devices`. Deve existir exatamente um dispositivo com estado `device`.

Use **Detectar dispositivo**, **Buscar fotografias**, selecione linhas e **Exportar selecionadas**.

A busca inicial examina arquivos presentes em `/sdcard/DCIM` e `/sdcard/Pictures`. Não lê armazenamento bruto, lixeira privada, dados internos de aplicativos nem fotos definitivamente apagadas.

## Estado de validação

Código inicial publicado; compilação e testes em Windows/Android ainda não executados. Use primeiro com dados de teste. A versão inicial não deve ser usada como ferramenta pericial certificada.

## Segurança

O manifesto contém o serial do aparelho e caminhos potencialmente sensíveis: mantenha a pasta de caso protegida. Não publique evidências no GitHub.
