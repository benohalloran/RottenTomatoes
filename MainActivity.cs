/***
 * Ben O'Halloran
 * 6/8/14
 * RottenTomatoes: first screen filled with movies of the three types
 */

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Json;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.OS;

namespace RottenTomatoes
{
	[Activity (Label = "Rotten Tomatoes", MainLauncher = true)]
	public class MainActivity : ListActivity
	{
		private MovieListAdapter myAdapter;

		public enum MovieType
		{
			//top box office
			TOP,
			//opening this week
			OPEN,
			//also in theatres
			ALSO
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			ListView.Adapter = (myAdapter = new MovieListAdapter (this));
			Refresh ();
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.main_menu, menu);
			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case Resource.Id.menu_refresh:
				Refresh ();
				Toast.MakeText (this, "Refreshing", ToastLength.Short).Show ();
				return true;
			case Resource.Id.menu_comp:
				new AlertDialog.Builder (this).SetTitle ("API Notes")
					.SetCancelable (true)
					.SetMessage (Resource.String.compliance)
					.SetPositiveButton ("OK", (IDialogInterfaceOnClickListener)null).Show ();
				return true;
			default:
				return base.OnOptionsItemSelected (item);
			}
		}

		private void Refresh ()
		{
			//each type of movie is fetched in parellel 
			Download (MovieType.OPEN);
			Download (MovieType.TOP);
			Download (MovieType.ALSO);
		}

		public async Task<JsonArray> Download (MovieType type)
		{
			string key = GetString (Resource.String.key);
			string url;
			switch (type) {
			case MovieType.TOP:
				url = "http://api.rottentomatoes.com/api/public/v1.0/lists/movies/box_office.json?apikey=";
				break;
			case MovieType.OPEN:
				url = "http://api.rottentomatoes.com/api/public/v1.0/lists/movies/opening.json?apikey=";
				break;
			case MovieType.ALSO:
				url = "http://api.rottentomatoes.com/api/public/v1.0/lists/movies/in_theaters.json?apikey=";
				break;
			default:
				url = "";
				break;
			}
			url += key;
			var client = new HttpClient ();
			string contents = await client.GetStringAsync (url);
			JsonObject obj = (JsonObject)JsonObject.Parse (contents);
			JsonArray array = (JsonArray)obj ["movies"];
			myAdapter.AddData (array, type);
			return array;
		}
	}
}