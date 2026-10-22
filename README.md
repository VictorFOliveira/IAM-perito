# IAM-PERITO

Laboratório desktop de perícia digital para dispositivos Android e análise de evidências.

> Status: planejamento técnico / versão 0.1.0. Nenhuma capacidade de recuperação foi validada ainda.

## Objetivo inicial

Coletar e analisar fotografias de dispositivos Android autorizados, distinguir arquivos existentes de cópias residuais e registrar evidências verificáveis.

## Stack

- Windows x64: C# / .NET 10, WPF + MVVM
- Android: Android Platform Tools (ADB), com autorização do dispositivo
- Casos e inventário: SQLite
- Evidências: SHA-256, manifesto JSON e trilha de auditoria
- Motor binário futuro: Rust, executado como processo isolado
- Testes: xUnit

## Limites técnicos

ADB não recupera diretamente blocos apagados. Em Android moderno, criptografia e TRIM frequentemente inviabilizam recuperação de fotos permanentemente excluídas. A primeira versão pesquisa arquivos e cópias ainda acessíveis, sem prometer recuperação física.

## Princípios

1. Somente dispositivos próprios ou com autorização documentada.
2. Nenhum root automático, desbloqueio de bootloader ou bypass de senha.
3. Coleta não destrutiva sempre que possível; registrar qualquer alteração inevitável.
4. Preservar arquivos originais e calcular SHA-256 após coleta.
5. Registrar ferramenta, versão, comandos, horários, erros e limitações.
6. Nunca classificar miniatura como fotografia original recuperada.

Veja [o plano técnico](docs/ARCHITECTURE.md).
