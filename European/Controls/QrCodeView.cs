using European.Services;

namespace European.Controls
{
    /// <summary>Desenha um QR Code (Services/QrCode) a partir do texto: módulos pretos sobre fundo branco, com margem.</summary>
    public class QrCodeView : GraphicsView
    {
        public static readonly BindableProperty TextoProperty =
            BindableProperty.Create(nameof(Texto), typeof(string), typeof(QrCodeView), string.Empty,
                propertyChanged: (b, _, novo) => ((QrCodeView)b).Atualizar(novo as string));

        private readonly Desenho _desenho = new();

        public QrCodeView()
        {
            Drawable = _desenho;
        }

        public string Texto
        {
            get => (string)GetValue(TextoProperty);
            set => SetValue(TextoProperty, value);
        }

        private void Atualizar(string? texto)
        {
            _desenho.Modulos = string.IsNullOrEmpty(texto) ? null : QrCode.Gerar(texto);
            Invalidate();
        }

        private sealed class Desenho : IDrawable
        {
            /// <summary>Margem obrigatória em volta do código (zona de silêncio), em módulos.</summary>
            private const int Margem = 4;

            public bool[,]? Modulos { get; set; }

            public void Draw(ICanvas canvas, RectF area)
            {
                canvas.FillColor = Colors.White;
                canvas.FillRectangle(area);
                if (Modulos is not { } modulos)
                    return;

                var tamanho = modulos.GetLength(0);
                var lado = Math.Min(area.Width, area.Height);
                var modulo = lado / (tamanho + Margem * 2);
                var inicioX = area.X + (area.Width - modulo * tamanho) / 2;
                var inicioY = area.Y + (area.Height - modulo * tamanho) / 2;

                canvas.FillColor = Colors.Black;
                for (var y = 0; y < tamanho; y++)
                    for (var x = 0; x < tamanho; x++)
                        if (modulos[y, x])
                            // um pouco maior que o módulo, para não aparecer fresta entre quadrados vizinhos
                            canvas.FillRectangle(inicioX + x * modulo, inicioY + y * modulo, modulo + 0.5f, modulo + 0.5f);
            }
        }
    }
}
