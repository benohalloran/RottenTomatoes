RottenTomatoes
==============
A simple app written in C# for Android using Xamarin
Consumes and displays the [Rotten Tomatoes API](http://developer.rottentomatoes.com/)
Note: This may not adhere to the brand guidlines set forth by Rotten Tomatoes because the [PDF which outlines](http://content50.flixster.com/static/ads/RT_Brand_Guidelines_v10.pdf) is not avaliable (as of 6/8/14).
All images are from Rotten Tomatoes.

As required by Android, all web access is done on background threads, in this case the C# await and async structure. The same method which features the data also uses it to fill in data into the views.
Additionally, the API responce is saved for faster loads. Note that when Xamarin Studio removes the previously installed application, the data is of course deleted.