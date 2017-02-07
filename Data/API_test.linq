<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>System</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

public class APIResultNoteData { public List<string> tags = new List<string>(); public bool deleted; public string shareURL, content, publishURL; public List<string> systemTags = new List<string>(); public double modificationDate, creationDate; }

void Main()
{
	string APP_ID = "chalk-bump-f49";
	
	var userName = "mailport@mikescher.de";
	var password = "NoChuWugFoZobQuixHeckPyPhlynKaTir";
	
	string apiKey = "6ebfbdf6bfa8423e85d8733f6b6bbc25";
	string appId = "chalk-bump-f49";
	
	string AccessToken = "3eea439309b945109bc66f9be866f32e";
	
	var web = new WebClient();
	web.Headers["User-Agent"] = "Microsoft.Net/CommonNote";
	web.Headers["X-Simperium-API-Key"] = apiKey;
	web.Headers["X-Simperium-Token"] = AccessToken;


	//var uri = new Uri($"https://auth.simperium.com/1/{APP_ID}/authorize/");
	//var content = $"{{\"username\":\"{userName}\", \"password\":\"{password}\"}}";
	//web.UploadString(uri, content).Dump();
	//{ "username": "mailport@mikescher.de", "access_token": "3eea439309b945109bc66f9be866f32e", "userid": "1eabbd44ca707c8289d3bedc134e2aef"}


	//web.DownloadString($"https://api.simperium.com/1/{APP_ID}/note/index").Dump();
	// {"current": "589a1d954806f95991e72562", "index": [{"id": "d8d760f13e1b48b7ba305012960d69b8", "v": 2}, {"id": "37f0176b-3287-4e33-884e-ceb745549894", "v": 49}, {"id": "86bdcd48-97d1-44a4-825b-0e56d8884a0c", "v": 10}, {"id": "1e18828ce750457999b3eafe99dbdbbb", "v": 2}, {"id": "6ab1d7d3-ecca-4e3d-a094-b970a99b45f7", "v": 17}, {"id": "welcome-android", "v": 2}, {"id": "02a8d362-31fb-400d-8b07-2b3d55696bbe", "v": 3}, {"id": "agtzaW1wbGUtbm90ZXIRCxIETm90ZRiAgJD2lYH3CAw", "v": 3}]}

	//var data = web.DownloadString($"https://api.simperium.com/1/{APP_ID}/note/i/{"d8d760f13e1b48b7ba305012960d69b8"}").Dump();
	//web.ResponseHeaders["X-Simperium-Version"].Dump();
	//JsonConvert.DeserializeObject<APIResultNoteData>(data).Dump();
	//{"tags": [], "deleted": false, "shareURL": "", "systemTags": [], "content": "TODO Urlaub\n\nTODO Urlaub \n\nTreffen 6:00 Fabian Appenweier Nesselriederstra\u00dfe 35\n\nGep\u00e4ck:\n\n[x] Koffer gro\u00df\n[x] Koffer Handgep\u00e4ck\n[ ] Benzingeld 2 Konrad\n[x] Klamotten\n[x] Duschgel\n[x] Handt\u00fccher\n[x] Schuhe\n[x] Laptop\n[x] Router + Kabel\n[x] Kartenspiele\n[x] Ball etc whatever\n[x] Handy\n[x] kindle (B\u00fccher laden)\n[x] Ladeger\u00e4t Kindle + Handy + Laptop\n[x] Fake Skat\n[x] Wasserdichtes SKAT\n[x] Anker boxen\n[x] Tischtennis Schl\u00e4ger\n[x] Sonnenbrille\n[x] Sonnencreme\n[x] miXed playlists 2 handy\n[x] Audiibooks 4 handy\n[x] Fnails\n[ ] Boox (tv) \n[x] Kopfh\u00f6rer h\n", "publishURL": "", "modificationDate": 1486473659, "creationDate": 1485191732.436}
}
