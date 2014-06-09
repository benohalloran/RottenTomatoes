/***
 * Ben O'Halloran
 * 6/8/14
 * RottenTomatoes: adapter for main screen
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Json;
using System.Net;

using Android.OS;
using Android.App;
using Android.Graphics;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;

namespace RottenTomatoes
{
	class MovieListAdapter : BaseAdapter<JsonObject>
	{
		public static string DataKey = "data";

		JsonArray open, top, also;
		//these are the "movie" J
		Activity context;

		public MovieListAdapter (Activity context) : base ()
		{
			this.context = context;
			open = LoadJson (MainActivity.MovieType.OPEN);
			top = LoadJson (MainActivity.MovieType.TOP);
			also = LoadJson (MainActivity.MovieType.ALSO);
			NotifyDataSetChanged ();
		}

		public void AddData (JsonArray a, MainActivity.MovieType type)
		{
			JsonArray write;
			switch (type) {
			case MainActivity.MovieType.ALSO:
				if (also.Equals (a))
					return;
				write = also = a;
				break;
			case MainActivity.MovieType.OPEN:
				if (open.Equals (a))
					return;
				write =	open = a;
				break;
			case MainActivity.MovieType.TOP:
				if (top.Equals (a))
					return;
				write = top = a;
				break;
			default:
				return;
			}
			WriteJson (write, type);
			NotifyDataSetChanged ();
		}

		public override long GetItemId (int pos)
		{
			return pos;
		}

		public override int Count {
			get { return open.Count + also.Count + top.Count + 3; }
		}

		public override JsonObject this [int position] {
			get {
				if (position == 0) //open header
					return null;
				else if (position == open.Count + 1) //top header
					return null;
				else if (position == open.Count + top.Count + 2) //also header
					return null;
				else if (position < open.Count + 1)
					return (JsonObject)open.ElementAt (position - 1);
				else if (position < open.Count + 2 + top.Count)
					return (JsonObject)top.ElementAt (position - 2 - open.Count);
				else
					return (JsonObject)also.ElementAt (position - 3 - open.Count - top.Count);
			}
		}


		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			//handle the headers
			JsonObject data = this [position];
			if (data == null) {
				if (position == 0)
					return MakeHeaderView ("Opening This Week");
				else if (position == open.Count + 1)
					return MakeHeaderView ("Top Box Office");
				else
					return MakeHeaderView ("Also in theaters");
			}
			View v = context.LayoutInflater.Inflate (Resource.Layout.movie_view, null);
			//textviews
			TextView name = v.FindViewById<TextView> (Resource.Id.movie_title);
			TextView actors = v.FindViewById<TextView> (Resource.Id.cast);
			TextView critic = v.FindViewById<TextView> (Resource.Id.overall_critic);
			TextView rating = v.FindViewById<TextView> (Resource.Id.rating);
			TextView runtime = v.FindViewById<TextView> (Resource.Id.run_time);
			//imageviews
			ImageView freshRotten = v.FindViewById<ImageView> (Resource.Id.fresh_rotten_img);
			ImageView poster = v.FindViewById<ImageView> (Resource.Id.movie_poster);

			//set the textviews

			name.Text = data ["title"];
			actors.Text = ActorString ((JsonArray)data ["abridged_cast"]);
			JsonObject ratings = (JsonObject)data ["ratings"];

			critic.Text = "";
			if (ratings.ContainsKey ("critics_score"))
				critic.Text += ratings ["critics_score"].ToString () + "% ";
			if (ratings.ContainsKey ("critics_rating"))
				critic.Text += ratings ["critics_rating"].ToString ().Replace ("\"", "");
			rating.Text = data ["mpaa_rating"];
			runtime.Text = GetRunTime ((int)data ["runtime"]);

			//set the image view
			if (ratings.ContainsKey ("critics_rating"))
				freshRotten.SetImageResource (FreshRottenId (ratings ["critics_rating"].ToString ()));
			else
				freshRotten.SetImageBitmap (null);
			Bitmap img;
			poster.SetImageBitmap (img = PosterHelper (data));

			v.Click += delegate {
				var launch = new Intent (context, typeof(MovieDetails));
				launch.PutExtra (DataKey, (int)data ["id"]);
				MovieDetails.data = img;
				context.StartActivity (launch);
			};
			v.Id = position;
			return v;
		}

		private Bitmap PosterHelper (JsonObject objRoot)
		{
			JsonObject posters = (JsonObject)objRoot ["posters"];
			var temp = posters ["thumbnail"];
			return GetBitmapFromURL (temp.ToString ().Replace ("\"", ""));
		}

		private Bitmap GetBitmapFromURL (string url)
		{
			//from https://forums.xamarin.com/discussion/4323/image-from-url-in-imageview
			Bitmap imageBitmap = null;
			using (var webClient = new WebClient ()) {
				var imageBytes = webClient.DownloadData (url);
				if (imageBytes != null && imageBytes.Length > 0) {
					imageBitmap = BitmapFactory.DecodeByteArray (imageBytes, 0, imageBytes.Length);
				}
			}
			return imageBitmap;
		}

		/**These methods write and load a json array. 
		 * This allows the app to load more quickly.*/
		private  void WriteJson (JsonArray data, MainActivity.MovieType type)
		{
			Task.Run (() => {
				File.WriteAllText (context.GetFileStreamPath (type.ToString () + ".json").ToString (), 
					data.ToString ());
				var v = File.OpenWrite ("");
			});
		}

		private JsonArray LoadJson (MainActivity.MovieType type)
		{
			try {
				var str = File.ReadAllText (context.GetFileStreamPath (type.ToString () + ".json").AbsoluteFile.AbsolutePath);
				return (JsonArray)JsonArray.Parse (str);
			} catch {
				return new JsonArray ();
			}
		}

		public static string GetRunTime (int min)
		{
			return string.Format ("{0} hr. {1} min.", min / 60, min % 60);
		}

		private string ActorString (JsonArray info)
		{
			string s = "";
			if (info.Count > 0)
				s += info.ElementAt (0) ["name"];
			if (info.Count >= 1)
				s += ", " + info.ElementAt (1) ["name"];
			return s;
		}

		public static int FreshRottenId (string rating)
		{
			if (rating.ToLower ().Contains ("rotten"))
				return Resource.Drawable.rotten;
			else if (rating.ToLower ().Contains ("fresh"))
				return Resource.Drawable.fresh;
			else
				return -1;
		}

		public TextView MakeHeaderView (string txt)
		{
			TextView v = (TextView)context.LayoutInflater.Inflate (Resource.Layout.header, null);
			v.Text = txt;
			return v;
		}
	}
}
