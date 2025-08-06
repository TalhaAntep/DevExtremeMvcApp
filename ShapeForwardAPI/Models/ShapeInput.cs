namespace ShapeForwardAPI.Models {

    public class ShapeInput
    {
        public int Id { get; set; }
        public string Shape { get; set; }
        public int Width { get; set; }
        public int? Height { get; set; }
        public string UserId { get; set; }
        public int CalculationResult { get; set; }

        public bool IsCalculated { get; set; } = false;
    }

}

