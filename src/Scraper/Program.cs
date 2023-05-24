using System;
using System.Net.Http;
using System.Threading.Tasks;

// See https://aka.ms/new-console-template for more information
using var client = new HttpClient();
var response = await client.GetAsync("https://timeapi.io/api/Time/current/zone?timeZone=Europe/Amsterdam");
var content = await response.Content.ReadAsStringAsync();
File.WriteAllText("time.json", content);