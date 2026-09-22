namespace EuropeanMaui.Extensions
{
    /// <summary>Posicionamento de filhos em Grid por coluna/linha (sem sombrear Layout.Add).</summary>
    public static class GridAddons
    {
        public static void AddAt(this Grid grid, IView child, int column, int row)
        {
            grid.Add(child);
            grid.SetColumn(child, column);
            grid.SetRow(child, row);
        }
    }
}