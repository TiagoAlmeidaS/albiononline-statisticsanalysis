# Soluções para Problema de Debug no Rider

## Problema Identificado

O erro `ActiveSpellEffectsUpdateEvent.cs is not found among the loaded symbol documents` no Rider indica que o debugger não consegue mapear o arquivo de código-fonte com o assembly em execução.

## Análise do Projeto

### Estrutura dos Projetos
- **Projeto Principal**: `StatisticsAnalysisTool` (WPF Application)
- **Projeto Network**: `StatisticsAnalysisTool.Network` (Class Library)
- **Arquivo Problemático**: `ActiveSpellEffectsUpdateEvent.cs` está localizado em:
  ```
  src/StatisticsAnalysisTool/Network/Events/ActiveSpellEffectsUpdateEvent.cs
  ```

### Dependências
- O projeto principal referencia `StatisticsAnalysisTool.Network`
- O arquivo está no projeto principal, mas o Rider pode estar tentando debuggar através do projeto Network

## Soluções Recomendadas

### 1. ✅ Verificar Configuração de Build
**Status**: ✅ Resolvido
- Os arquivos PDB estão sendo gerados corretamente
- Build em modo Debug está funcionando
- Símbolos de debug estão disponíveis

### 2. 🔧 Configurar Projeto de Inicialização no Rider
1. No Rider, vá para **Run/Debug Configurations**
2. Certifique-se de que o projeto de inicialização está configurado como:
   - **Project**: `StatisticsAnalysisTool` (não `StatisticsAnalysisTool.Network`)
   - **Configuration**: `Debug`
   - **Platform**: `Any CPU`

### 3. 🔧 Limpar e Rebuildar
Execute os seguintes comandos na pasta `src`:
```powershell
dotnet clean
dotnet build --configuration Debug
```

### 4. 🔧 Verificar Configurações de Debug no Rider
1. Vá para **File → Settings → Build, Execution, Deployment → Debugger**
2. Certifique-se de que:
   - **Enable debugger** está marcado
   - **Use managed compatibility mode** está desmarcado (para .NET 9)
   - **Enable .NET Framework source stepping** está marcado

### 5. 🔧 Verificar Módulos Carregados
Durante o debug:
1. Vá para **Debug → Windows → Modules**
2. Verifique se `StatisticsAnalysisTool.dll` está carregado com símbolos
3. Se não estiver, clique com botão direito → **Load Symbols**

### 6. 🔧 Configurações Específicas do Projeto
Adicione as seguintes configurações no arquivo `.csproj` se necessário:

```xml
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
  <DebugSymbols>true</DebugSymbols>
  <DebugType>full</DebugType>
  <Optimize>false</Optimize>
  <DefineConstants>DEBUG;TRACE</DefineConstants>
</PropertyGroup>
```

### 7. 🔧 Solução Alternativa: Attach to Process
Se o problema persistir:
1. Execute a aplicação normalmente
2. No Rider, vá para **Run → Attach to Process**
3. Selecione o processo `StatisticsAnalysisTool.exe`
4. Marque **Managed** e clique **Attach**

## Verificações Adicionais

### ✅ Arquivos PDB Gerados
Os seguintes arquivos PDB foram confirmados como sendo gerados:
- `StatisticsAnalysisTool.pdb` (689KB)
- `StatisticsAnalysisTool.Network.pdb` (14KB)
- `StatisticsAnalysisTool.Protocol16.pdb` (22KB)
- E outros...

### ✅ Build Bem-sucedido
- Build em modo Debug completado com sucesso
- Apenas warnings de compatibilidade (não afetam debug)
- Todos os assemblies e símbolos gerados

## Próximos Passos

1. **Teste a configuração**: Configure o projeto de inicialização correto no Rider
2. **Execute o debug**: Tente colocar breakpoints no arquivo `ActiveSpellEffectsUpdateEvent.cs`
3. **Verifique módulos**: Confirme que os símbolos estão carregados durante o debug
4. **Reporte resultado**: Informe se o problema foi resolvido ou se persiste

## Comandos Úteis

```powershell
# Limpar e rebuildar
dotnet clean
dotnet build --configuration Debug

# Verificar arquivos PDB
Get-ChildItem -Path "StatisticsAnalysisTool\bin\Debug\net9.0-windows\*.pdb"

# Executar aplicação
dotnet run --project StatisticsAnalysisTool
```

---

**Data**: 04/09/2025  
**Projeto**: Albion Online Statistics Analysis Tool  
**IDE**: JetBrains Rider  
**Framework**: .NET 9.0
