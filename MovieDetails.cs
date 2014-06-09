/***
 * Ben O'Halloran
 * 6/8/14
 * RottenTomatoes: MovieDetails activity
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RottenTomatoes
{
	[Activity (Label = "Movie Details")]			
	public class MovieDetails : Activity
	{
		//Passing the Bitmap by Bundle.PutExtra(Parcable) is very slow and memory intensive, so I used this simple hack
		public static Bitmap data;

		public enum FetchType
		{
			MOVIE,
			CRITIC
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.movie_details);
			int id = -1;
			if (Intent.Extras != null)
				id = Intent.GetIntExtra (MovieListAdapter.DataKey, -1);
			else if (bundle != null)
				id = bundle.GetInt (MovieListAdapter.DataKey, -1);

			Task<JsonObject> movieTask = download (id, FetchType.MOVIE);
			Task<JsonObject> criticTask = download (id, FetchType.CRITIC);
			FindViewById<ImageView> (Resource.Id.movie_poster).SetImageBitmap (MovieDetails.data);
		}

		public async Task<JsonObject> download (int id, FetchType type)
		{
			string root = type == FetchType.CRITIC ? 
				"http://api.rottentomatoes.com/api/public/v1.0/movies/{0}/reviews.json?apikey={1}" :
				"http://api.rottentomatoes.com/api/public/v1.0/movies/{0}.json?apikey={1}";

			string url = string.Format (root, id, GetString (Resource.String.key));
			var client = new HttpClient ();

			string contents = await client.GetStringAsync (url);
			var data = JsonArray.Parse (contents);
			if (type == FetchType.MOVIE) {
				//set getting information as the api is fetched
				var title = FindViewById<TextView> (Resource.Id.movie_title);
				var actors = FindViewById<TextView> (Resource.Id.cast);
				var dir = FindViewById<TextView> (Resource.Id.movie_director);
				var rating = FindViewById<TextView> (Resource.Id.movie_rating);
				var runtime = FindViewById<TextView> (Resource.Id.movie_runtime);
				var genres = FindViewById<TextView> (Resource.Id.movie_genre);
				var releaseDate = FindViewById<TextView> (Resource.Id.movie_release);
				var synop = FindViewById<TextView> (Resource.Id.movie_sum);
				var critic = FindViewById<ListView> (Resource.Id.movie_critic);

				title.Text = data ["title"];
				dir.Text = ListString ((JsonArray)data ["abridged_directors"], "name");
				runtime.Text = MovieListAdapter.GetRunTime (data ["runtime"]);
				genres.Text = ListString ((JsonArray)data ["genres"]);
				releaseDate.Text = DateFormater (data ["release_dates"] ["theater"].ToString ());
				synop.Text = data ["synopsis"];
				actors.Text = ListString ((JsonArray)data ["abridged_cast"], "name");
				rating.Text = data ["mpaa_rating"];
			} else {
				FindViewById<ListView> (Resource.Id.movie_critic).Adapter = 
					new DetailsAdapter (this, (JsonArray)data ["reviews"]);
			}
			return (JsonObject)JsonObject.Parse (contents);
		}

		private string DateFormater (string raw)
		{
			var split = raw.Replace ("\"", "").Split ('-');
			string month = "";
			switch (split [1]) {
			case "01":
				month = "January";
				break;
			case "02":
				month = "February";
				break;
			case "03":
				month = "March";
				break;
			case "04":
				month = "April";
				break;
			case "05":
				month = "May";
				break;
			case "06":
				month = "June";
				break;
			case "07":
				month = "July";
				break;
			case "08":
				month = "August";
				break;
			case "09":
				month = "September";
				break;
			case "10":
				month = "October";
				break;
			case "11":
				month = "November";
				break;
			case "12":
				month = "December";
				break;
			default:
				month = split [1].Replace ("0", "");
				break;
			}
			return month + " " + split [2].Replace ("0", "") + ", " + split [0];
		}

		public string ListString (JsonArray array, string key)
		{
			string s = "";
			if (array.Count > 0)
				s += array.ElementAt (0) [key];
			for (int i = 1; i < array.Count; i++)
				s += ", " + array.ElementAt (i) [key];
			return s;
		}
		//this this activity from memory
		public override void OnBackPressed ()
		{
			Finish (); //actually clear, solves issue where the critic views won't appear for anything but the fist movie selected
			base.OnBackPressed ();
		}
		public string ListString (JsonArray array)
		{
			string s = "";
			if (array.Count > 0)
				s += array.ElementAt (0);
			for (int i = 1; i < array.Count; i++)
				s += ", " + array.ElementAt (i);
			return s;
		}
	}
}

