

namespace backend.Dto
{

    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;
    }

    public class UpdateCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

        public class ReadCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

}