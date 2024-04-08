using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

using OpenAIVisionApp.Services;

namespace OpenAIVisionApp.ViewModels
{
    public partial class ImageVideoPromptsViewModel : BaseViewModel
    {
        [ObservableProperty]
        string message;

        [ObservableProperty]
        string mediaUrl;

        [ObservableProperty]
        bool isImage;

        [ObservableProperty]
        bool isEnhanced;

        [ObservableProperty]
        string answer;

        string docId;

        IPromptService promptService;
        IDocumentService documentService;
        IMediaPicker mediaPicker;

        public ImageVideoPromptsViewModel(IPromptService promptService, 
            IDocumentService documentService,
            IMediaPicker mediaPicker)
        {
            this.promptService = promptService;
            this.documentService = documentService;
            this.mediaPicker = mediaPicker;
        }

        [RelayCommand]
        async Task ChoosePictureAsync()
        {
            IsImage = true;

            try
            {
                var photo = await mediaPicker.PickPhotoAsync();

                if (photo != null)
                    await uploadMediaAsync(photo);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task CreateVideoIndexAsync()
        {
            await promptService.CreateVideoIndex();
        }

        [RelayCommand]
        async Task IngestVideoAsync()
        {
            docId = await promptService.IngestVideo(MediaUrl);
        }

        [RelayCommand]
        async Task ChooseVideoAsync()
        {
            IsImage = false;

            try
            {
                var photo = await mediaPicker.PickVideoAsync();

                if (photo != null)
                    await uploadMediaAsync(photo);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task SendPromptAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                if (IsImage)
                {
                    if (!IsEnhanced)
                    {
                        Answer = await promptService
                            .SendPromptWithImageAsync(Message, MediaUrl);
                    }
                    else
                    {
                        Answer = await promptService
                            .SendPromptWithImageEnhancedAsync(Message, MediaUrl);
                    }
                }
                else
                {
                    Answer = await promptService
                        .SendPromptWithVideoEnhancedAsync(Message, MediaUrl, docId);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task uploadMediaAsync(FileResult file)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                MediaUrl = await documentService.UploadDocumentAsync(file);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
