using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Maui.Uploader.Components
{
	public class HyperlinkLabel : Label
	{
		public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(HyperlinkLabel), null);

		public string Url
		{
			get { return (string)GetValue(UrlProperty); }
			set { SetValue(UrlProperty, value); }
		}

		public HyperlinkLabel()
		{
			TextDecorations = TextDecorations.Underline;
			TextColor = Colors.Blue;
			GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(async () => await Launcher.OpenAsync(Url))
			});

		}
	}
}
