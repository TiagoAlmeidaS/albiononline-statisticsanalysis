# Configuração de Debug no Rider - Albion Online Statistics Analysis Tool

## ✅ Status da Preparação

### Build Completo
- ✅ Projeto limpo e recompilado em modo Debug
- ✅ Todos os arquivos PDB gerados com sucesso
- ✅ Símbolos de debug disponíveis (689KB para projeto principal)

### Arquivos PDB Gerados
```
StatisticsAnalysisTool.pdb (689KB) - Projeto principal
StatisticsAnalysisTool.Network.pdb (14KB)
StatisticsAnalysisTool.Protocol16.pdb (22KB)
StatisticsAnalysisTool.Diagnostics.pdb (18KB)
StatisticsAnalysisTool.Abstractions.pdb (11KB)
StatisticAnalysisTool.Extractor.pdb (19KB)
StatisticsAnalysisTool.PhotonPackageParser.pdb (14KB)
```

## 🚀 Configuração no Rider

### 1. Abrir Projeto
1. Abra o Rider
2. **File → Open** → Selecione a pasta `src`
3. Aguarde o Rider indexar o projeto

### 2. Configurar Run/Debug Configuration
1. Vá para **Run → Edit Configurations...**
2. Clique no **+** → **.NET Project**
3. Configure:
   - **Name**: `StatisticsAnalysisTool Debug`
   - **Project**: `StatisticsAnalysisTool` (não o Network!)
   - **Configuration**: `Debug`
   - **Platform**: `Any CPU`
   - **Target Framework**: `net9.0-windows`

### 3. Configurações de Debug
1. **File → Settings → Build, Execution, Deployment → Debugger**
2. Verifique:
   - ✅ **Enable debugger**
   - ❌ **Use managed compatibility mode** (desmarcado para .NET 9)
   - ✅ **Enable .NET Framework source stepping**

### 4. Executar Debug
1. Coloque breakpoints no arquivo `ActiveSpellEffectsUpdateEvent.cs`
2. Pressione **F5** ou clique no botão **Debug**
3. O projeto deve iniciar em modo debug

## 🔧 Comandos Úteis

```powershell
# Limpar e rebuildar
dotnet clean
dotnet build --configuration Debug

# Executar diretamente
dotnet run --project StatisticsAnalysisTool

# Verificar arquivos PDB
Get-ChildItem -Path "StatisticsAnalysisTool\bin\Debug\net9.0-windows\*.pdb"
```

## 🎯 Teste de Debug

### Arquivo para Testar
- **Localização**: `src/StatisticsAnalysisTool/Network/Events/ActiveSpellEffectsUpdateEvent.cs`
- **Linha para breakpoint**: Linha 17 (dentro do construtor)

### Verificações Durante Debug
1. **Debug → Windows → Modules**
   - Verificar se `StatisticsAnalysisTool.dll` está carregado
   - Confirmar que símbolos estão carregados

2. **Debug → Windows → Call Stack**
   - Verificar se o stack trace está visível

3. **Debug → Windows → Locals**
   - Verificar variáveis locais durante execução

## ⚠️ Solução de Problemas

### Se o breakpoint não funcionar:
1. Verifique se está debugando o projeto correto (`StatisticsAnalysisTool`)
2. Confirme que os símbolos estão carregados
3. Tente **Debug → Detach All** e reinicie o debug

### Se aparecer erro de símbolos:
1. **Debug → Windows → Modules**
2. Clique com botão direito em `StatisticsAnalysisTool.dll`
3. Selecione **Load Symbols**

## 📁 Estrutura do Projeto

```
src/
├── StatisticsAnalysisTool/           # ← Projeto principal (WPF)
│   ├── Network/
│   │   └── Events/
│   │       └── ActiveSpellEffectsUpdateEvent.cs  # ← Arquivo para debug
│   └── bin/Debug/net9.0-windows/
│       └── StatisticsAnalysisTool.pdb            # ← Símbolos principais
├── StatisticsAnalysisTool.Network/   # ← Biblioteca (referenciada)
└── StatisticsAnalysisTool.sln        # ← Solução
```

---

**Projeto está pronto para debug!** 🎉
