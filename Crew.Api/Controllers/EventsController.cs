using Crew.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private static readonly List<Event> Events = new()
    {
        new Event { Id = 1, Title = "City Walk", Location = "Berlin" , Description = "A walk through the city center", Latitude = 52.520008, Longitude = 13.404954},
        new Event { Id = 2, Title = "Museum Tour", Location = "Paris" , Description = "A tour of the Louvre Museum", Latitude = 48.856614, Longitude = 2.3522219},
        new Event { Id = 1, Title = "Coffee Meetup", Location = "Paris",Description = "聊聊创业和生活", Latitude = 48.8566, Longitude = 2.3522 }, // Paris
        new Event { Id = 2, Title = "Art Gallery Walk",  Location = "Berlin",Description = "一起探索当代艺术", Latitude = 51.5074, Longitude = -0.1278 }, // London
        new Event { Id = 3, Title = "Hiking Adventure", Location = "Berlin", Description = "阿尔卑斯山徒步", Latitude = 46.2044, Longitude = 6.1432 }, // Geneva
        new Event { Id = 4, Title = "Board Games Night",  Location = "Paris",Description = "桌游+社交", Latitude = 52.5200, Longitude = 13.4050 }, // Berlin
        new Event { Id = 5, Title = "Live Music", Location = "Berlin", Description = "一起听爵士", Latitude = 41.9028, Longitude = 12.4964 }, // Rome
    };

        [HttpGet]
        public ActionResult<IEnumerable<Event>> GetAll() => Events;

        [HttpGet("{id}")]
        public ActionResult<Event> GetById(int id)
        {
            var ev = Events.FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();
            return ev;
        }

        [HttpPost]
        public ActionResult<Event> Create(Event newEvent)
        {
            newEvent.Id = Events.Max(e => e.Id) + 1;
            Events.Add(newEvent);
            return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, newEvent);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Event>>> SearchEvents(string query, double? lat, double? lng, double? radiusKm = 10)
        {
            //var events = _context.Events.AsQueryable();       // 还没数据库先用静态数据

            //// 按标题关键字搜索
            //if (!string.IsNullOrEmpty(query))
            //{
            //    events = events.Where(e => e.Title.Contains(query));
            //}

            //// 可选: 附近搜索 (如果传了坐标)
            //if (lat.HasValue && lng.HasValue)
            //{
            //    double latVal = lat.Value;
            //    double lngVal = lng.Value;
            //    double radius = radiusKm ?? 10;

            //    events = events.Where(e =>
            //        (6371 * Math.Acos(
            //            Math.Cos(Deg2Rad(latVal)) *
            //            Math.Cos(Deg2Rad(e.Latitude)) *
            //            Math.Cos(Deg2Rad(e.Longitude) - Deg2Rad(lngVal)) +
            //            Math.Sin(Deg2Rad(latVal)) *
            //            Math.Sin(Deg2Rad(e.Latitude))
            //        )) <= radius
            //    );
            //}

            //return await events.ToListAsync();

            // 还没数据库先用静态数据，测试用的
            var events = Events.AsQueryable();
            if (!string.IsNullOrEmpty(query))
            {
                events = events.Where(e => e.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(events.ToList());
        }

        private double Deg2Rad(double deg) => deg * (Math.PI / 180);
    }
}
