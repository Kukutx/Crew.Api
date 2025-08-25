using Crew.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : Controller
    {
        private static readonly List<Event> Events = new()
    {
        new Event { Id = 1, Title = "City Walk", Location = "Berlin" , Description = "A walk through the city center", Latitude = 52.520008, Longitude = 13.404954},
        new Event { Id = 2, Title = "Museum Tour", Location = "Paris" , Description = "A tour of the Louvre Museum", Latitude = 48.856614, Longitude = 2.3522219}
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
    }
}
