# Recursos de Template Matching

Este diretório contém os arquivos de template necessários para o template matching do VisionDetector.

## Arquivos Necessários

### bobber.png
- **Descrição**: Template do bobber de pesca do Albion Online
- **Formato**: PNG (recomendado) ou JPG
- **Tamanho**: Idealmente 32x32 a 64x64 pixels
- **Características**: 
  - Deve ser uma imagem limpa do bobber sem fundo
  - Preferencialmente com fundo transparente
  - Deve representar o bobber em sua forma mais comum no jogo

### bobber_in_water.png
- **Descrição**: Template para detecção do anzol/bobber NA ÁGUA (usado por `BobberInWaterDetector`)
- **Formato**: PNG/JPG
- **Sugestão de tamanho**: similar ao `bobber.png` (32x32 a 64x64)
- **Localização sugerida**: `data/images/bobber_in_water.png`
  - O detector usa esse caminho por padrão, mas você pode alterar via `SetTemplatePath()` ou no construtor.

## Como Criar o Template

1. **Capture uma screenshot** do bobber no jogo
2. **Recorte apenas o bobber** usando um editor de imagem
3. **Remova o fundo** se possível (use transparência)
4. **Redimensione** para um tamanho adequado (32x32 a 64x64 pixels)
5. **Salve como PNG** no diretório `resources/`

## Configuração

O `VisionDetector` procura automaticamente por `resources/bobber.png`. 
Se o arquivo não for encontrado, o sistema usará o método de fallback (análise de brilho).

O `BobberInWaterDetector` usa por padrão `data/images/bobber_in_water.png`. Você pode:
- Definir via construtor: `new BobberInWaterDetector("data/images/meu_template.png")`
- Definir depois: `detector.SetTemplatePath("...")`

## Debug Visual

Para ativar o debug visual e ver o template matching em tempo real, descomente as linhas no método `AnalyzeTemplateMatching`:

```csharp
// Emgu.CV.CvInvoke.Rectangle(sourceMat, new Rectangle(maxLoc.X, maxLoc.Y, templateMat.Width, templateMat.Height), new Emgu.CV.Structure.MCvScalar(0, 0, 255), 2);
// Emgu.CV.CvInvoke.ImShow("Match Preview", sourceMat);
// Emgu.CV.CvInvoke.WaitKey(1);
```

## Threshold de Confiança

O threshold padrão é 0.4 (40%). Você pode ajustar este valor no construtor do VisionDetector:

```csharp
var detector = new VisionDetector(region, "template", 0.5); // 50% de confiança
``` 