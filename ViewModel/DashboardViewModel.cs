namespace PlataformaMunicipal.ViewModels
{
    public class ChartItem
    {
        public string Etiqueta { get; set; }
        public int Valor { get; set; }
        public string Color { get; set; }
    }

    public class DashboardViewModel
    {
        public int Total { get; set; }
        public int Resueltas { get; set; }
        public int EnProceso { get; set; }
        public int Vencidas { get; set; }
        public List<ChartItem> PorEstado { get; set; } = new();
        public List<ChartItem> PorDepartamento { get; set; } = new();
        public List<ChartItem> PorSector { get; set; } = new();
    }
}