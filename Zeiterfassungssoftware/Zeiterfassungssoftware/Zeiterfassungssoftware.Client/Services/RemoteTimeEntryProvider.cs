using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Zeiterfassungssoftware.SharedData.Activities;
using Zeiterfassungssoftware.SharedData.Time;
using System.Text.Json.Serialization;

namespace Zeiterfassungssoftware.Client.Services
{

	public class RemoteTimeEntryProvider : ITimeEntryProvider
	{
		public static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        public HttpClient HttpClient { get; set; } = new HttpClient()
		{
			BaseAddress = new Uri("https://localhost:7099/api/v1/entries/")
		};

		private List<TimeEntry> _timeEntries = [];
		public bool IsLoaded { get; private set; }


		public RemoteTimeEntryProvider() 
		{
			LoadEntries();
		}

		public async void LoadEntries()
		{
			_timeEntries = await HttpClient.GetFromJsonAsync<List<TimeEntry>>("") ?? new();
			IsLoaded = true;
		}

		public async void Add(TimeEntry Entry)
		{

			string JsonData = JsonSerializer.Serialize(Entry);
			var Content = new StringContent(JsonData, Encoding.UTF8, "application/json");

			HttpResponseMessage Response = await HttpClient.PostAsync("", Content);
			
			try
			{
				Response.EnsureSuccessStatusCode();
				string ReponseContent = await Response.Content.ReadAsStringAsync();
				Entry = JsonSerializer.Deserialize<TimeEntry>(ReponseContent, Options) ?? new();
                _timeEntries.Add(Entry);
			}
			catch (Exception e) { Console.WriteLine("Failed to Send Entry"); }

			return;
			
		}

		public List<TimeEntry> GetEntries()
		{
			return _timeEntries;
		}

		public async Task<TimeEntry> GetEntryById(Guid Id)
		{
			return await HttpClient.GetFromJsonAsync<TimeEntry>($"{Id}");
		}


        public async Task Remove(TimeEntry Entry)
		{
			HttpResponseMessage Message = await HttpClient.DeleteAsync($"{Entry.Id}");
			
			try
			{
				Message.EnsureSuccessStatusCode();

				Entry = _timeEntries.Where(e => e.Id == Entry.Id).FirstOrDefault();
				if (Entry is not null)
					_timeEntries.Remove(Entry);

            } catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

        public async Task Update(TimeEntry Entry)
        {
			string JsonData = JsonSerializer.Serialize(Entry);
			var Content = new StringContent(JsonData, Encoding.UTF8, "application/json");

			HttpResponseMessage Response = await HttpClient.PatchAsync("", Content);

			try
			{
				Response.EnsureSuccessStatusCode();
				_timeEntries[_timeEntries.IndexOf(_timeEntries.FirstOrDefault(e => e.Id == Entry.Id) ?? new())] = Entry;
            }
			catch (Exception e) { Console.WriteLine("Failed to Update Entry"); }

			return;
        }

		
    }
}