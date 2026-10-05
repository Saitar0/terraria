CONTEXTO: Estou construindo um jogo 2D de mundo de tiles estilo sandbox, em C# / .NET 8 + MonoGame (DesktopGL), SEM engine de terceiros, SEM ECS externo.

Assets: 100% originais ou placeholders gerados por código. Não copie assets nem código de nenhum jogo comercial.

Constantes: TILE=16px, TPS=60 (passo fixo), mundo grande 8400x2400 tiles, chunk lógico 64x64.

Metas globais: 60 FPS em 1080p, ZERO alocações por frame no loop quente (sem LINQ, sem lambdas, sem foreach em List<T>, sem new em Update/Draw), dados contíguos (arrays/structs), mundo row-major (índice = y*W + x).

Entrego: código completo, compilável, com namespaces, comentários XML curtos nas APIs públicas e testes xUnit quando indicado.

Responda com: (1) árvore de arquivos, (2) código de cada arquivo, (3) como rodar/testar, (4) checklist dos critérios de aceite.