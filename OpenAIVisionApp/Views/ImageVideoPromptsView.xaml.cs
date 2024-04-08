using OpenAIVisionApp.ViewModels;

namespace OpenAIVisionApp.Views;

public partial class ImageVideoPromptsView : ContentPage
{
	public ImageVideoPromptsView(ImageVideoPromptsViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}