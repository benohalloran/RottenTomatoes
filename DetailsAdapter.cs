/***
 * Ben O'Halloran
 * 6/8/14
 * RottenTomatoes: adapter for the MovieDetails critic's view
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RottenTomatoes
{
	class DetailsAdapter: BaseAdapter<JsonObject>
	{
		public string DataKey = "data";

		public enum DataType
		{
			CAST,
			REVIEWS
		}

		JsonArray data;
		Activity context;

		public DetailsAdapter (Activity context, JsonArray data) : base ()
		{
			this.context = context; 
			this.data = data;
		}

		public override long GetItemId (int pos)
		{
			return pos;
		}

		public override int Count {
			get { return data.Count; }
		}

		public override JsonObject this [int position] {
			get { return (JsonObject)data [position]; }
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			//handle the headers
			JsonObject data = this [position];
			View v = convertView;
			if (v == null)
				v = context.LayoutInflater.Inflate (Resource.Layout.critic_review, null);
			bool actor = data.ContainsKey ("name");
			var img = v.FindViewById<ImageView> (Resource.Id.critic_img);
			var head = v.FindViewById<TextView> (Resource.Id.critic_header);
			var quote = v.FindViewById<TextView> (Resource.Id.critic_quote);
		
			v.Click += delegate {
				var url = data ["links"] ["review"];
				var intent = new Intent (Intent.ActionView, Android.Net.Uri.Parse (url));
				try {
					context.StartActivity (intent);
				} catch (Exception e) {
					Toast.MakeText (context, "Couldn't load the review", ToastLength.Short).Show ();
					Console.Error.WriteLine (e);
				}
			};
			img.SetImageResource (MovieListAdapter.FreshRottenId (data ["freshness"]));
			head.Text = data ["critic"] + ", " + data ["publication"];
			quote.Text = data ["quote"];
			return v;
		}
	}
}