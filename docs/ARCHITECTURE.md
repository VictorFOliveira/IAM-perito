# Arquitetura e plano técnico

## Módulos

- **Desktop**: WPF, MVVM, fluxo de casos, dispositivo e evidências.
- **AndroidAdapter**: descoberta ADB, autorização, inventário e coleta lógica.
- **PhotoDiscovery**: busca em DCIM, Pictures e diretórios acessíveis; classificação por origem.
- **EvidenceStore**: cópias em diretório de caso, SHA-256, metadados e manifesto.
- **Reporting**: relatório de resultados, método, limites e cadeia de custódia.
- **CarvingEngine (futuro)**: motor Rust para arquivos/imagens de armazenamento obtidos legitimamente.

## Fluxo v0.1

1. Criar caso com identificador único e consentimento.
2. Detectar dispositivo conectado e estado ADB.
3. Registrar fabricante, modelo, versão e identificadores estritamente necessários.
4. Executar busca somente em caminhos permitidos.
5. Apresentar resultados classificados: mídia existente, cópia/miniatura, artefato não validado.
6. Copiar itens selecionados para diretório de caso; registrar SHA-256.
7. Emitir manifesto JSON e relatório técnico com limitações.

## Segurança e confiabilidade

- Não alterar, excluir ou restaurar dados no aparelho automaticamente.
- Não executar root, desbloqueio de bootloader nem comandos destrutivos.
- Não registrar identificadores sensíveis em logs globais.
- Separar coleta de análise e preservar evidência original.
- Documentar ações ADB que possam modificar estado do dispositivo.
- Testar em aparelhos próprios com fotografias de teste e exclusões controladas.

## Critérios de aceite do primeiro marco

- Conexão e desconexão tratadas sem travar interface.
- Diferenciar ADB não instalado, USB ausente, unauthorized e device.
- Busca cancelável, com progresso e tratamento de falhas.
- Copiar arquivo sem alterar origem; SHA-256 verificável.
- Manifesto informa caminho de origem, caminho de destino, data de coleta e tipo de achado.
- Testes automatizados para classificação e hashes.

## Próximos marcos

- v0.1: descoberta, coleta lógica e manifesto.
- v0.2: thumbnails, análise de metadados e relatórios.
- v0.3: importação de backups/imagens e validação de formatos.
- v0.4: carving em Rust para imagens forenses, com testes de referência.

Nenhuma dessas etapas garante recuperação de fotos permanentemente apagadas do armazenamento interno criptografado.
