namespace Crew.Api.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Location { get; set; } = "";
        public double Latitude { get; set; }   // 新增
        public double Longitude { get; set; }  // 新增
    }
}
