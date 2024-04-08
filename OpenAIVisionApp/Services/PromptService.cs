using Azure;
using Azure.AI.OpenAI;
using OpenAIVisionApp.Helpers;
using OpenAIVisionApp.Models;
using System.Net.Http.Json;

namespace OpenAIVisionApp.Services
{
    public class PromptService : IPromptService
    {
        OpenAIClient oaiClient;
        HttpClient httpClient;
        HttpClient visionClient;

        public PromptService()
        {
            oaiClient = new OpenAIClient(
                new Uri(Constants.AzureOpenAIEndpoint),
                new AzureKeyCredential(Constants.AzureOpenAIKey));

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Constants.AzureOpenAIEndpoint);
            httpClient.DefaultRequestHeaders
                .Add("api-key", Constants.AzureOpenAIKey);

            visionClient = new HttpClient();
            visionClient.BaseAddress = new Uri(Constants.AzureComputerVisionEndpoint);
            visionClient.DefaultRequestHeaders
                .Add("Ocp-Apim-Subscription-Key", Constants.AzureComputerVisionKey);
        }

        public async Task<string> SendPromptWithImageAsync(
            string message, string mediaUrl)
        {
            var chatUserMessage = new ChatRequestUserMessage(
                    new ChatMessageTextContentItem(message),
                    new ChatMessageImageContentItem(
                        new ChatMessageImageUrl(
                            new Uri(mediaUrl))));

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = Constants.AzureOpenAIDeployment,
                Messages =
                {
                    new ChatRequestSystemMessage(Constants.SystemMessage),
                    chatUserMessage
                },
                MaxTokens = 2000
            };

            var chatResponse = await oaiClient
                .GetChatCompletionsAsync(chatCompletionsOptions);
            
            var chatChoice = chatResponse.Value.Choices.FirstOrDefault();

            if (chatChoice.FinishDetails is StopFinishDetails || 
                chatChoice.FinishReason == CompletionsFinishReason.Stopped)
            {
                return chatChoice.Message.Content;
            }

            return "Error. Try again.";
        }

        public async Task<string> SendPromptWithImageEnhancedAsync(string message, string mediaUrl)
        {
            var chatCompletionsEndpoint = $"openai/deployments/{Constants.AzureOpenAIDeployment}/extensions/chat/completions?api-version={Constants.AzureOpenAIApiVersion}";

            var payload = new
            {
                enhancements = new
                {
                    ocr = new { enabled = true },
                    grounding = new { enabled = true }
                },
                dataSources = new object[]
                {
                    new {
                        type = "AzureComputerVision",
                        parameters = new {
                            endpoint = Constants.AzureComputerVisionEndpoint,
                            key = Constants.AzureComputerVisionKey
                        }
                    }    
                },
                messages = new object[]
                {
                    new {
                        role = "system",
                        content = new object[] {
                            new {
                                type = "text",
                                text = Constants.SystemMessage,
                            }
                        }
                    },
                    new {
                        role = "user",
                        content = new object[] {
                            new {
                                type = "image_url",
                                image_url = new {
                                    url = mediaUrl
                                }
                            },
                            new {
                                type = "text",
                                text = message
                            }
                        }
                    }
                },
                temperature = 0.7,
                top_p = 0.95,
                max_tokens = 2000,
                stream = false
            };

            var chatResponse = await httpClient.PostAsJsonAsync(
                chatCompletionsEndpoint, payload);

            if (chatResponse.IsSuccessStatusCode)
            {
                var aoaiResponse = await chatResponse
                    .Content.ReadFromJsonAsync< AOAI_Response>();

                var chatChoice = aoaiResponse.choices.FirstOrDefault();
                var answer = chatChoice.message.content + "\n";

                var grounding = chatChoice.enhancements.grounding;
                answer += "--Grounding--\n";
                answer += $"Status: {grounding.status}\n";
                answer += "Lines:\n";

                foreach (var line in grounding.lines)
                {
                    answer += $"Text: {line.text}.\n";
                    answer += $"Objects: \n";

                    foreach (var span in line.spans)
                    {
                        answer += $"**{span.text}**\n";

                        foreach (var polygon in span.polygon)
                        {
                            answer += $"X={polygon.x}, Y= {polygon.y}\n";
                        }

                        answer += "\n" ;
                    }
                }

                return answer;
            }
            else
            {
                return ($"Error: {chatResponse.StatusCode}, {chatResponse.ReasonPhrase}");
            }
        }

        public async Task<string> SendPromptWithVideoEnhancedAsync(string message, string mediaUrl, string docId)
        {
            var chatCompletionsEndpoint = $"openai/deployments/{Constants.AzureOpenAIDeployment}/extensions/chat/completions?api-version={Constants.AzureOpenAIApiVersion}";

            var payload = new
            {
                enhancements = new
                {
                    video = new { enabled = true }
                },
                dataSources = new object[]
                {
                    new {
                        type = "AzureComputerVisionVideoIndex",
                        parameters = new {
                            computerVisionBaseUrl = Constants.AzureComputerVisionEndpoint,
                            computerVisionApiKey = Constants.AzureComputerVisionKey,
                            indexName = Constants.AzureComputerVisionIndex,
                            videoUrls = new object[] { mediaUrl }
                        }
                    }
                },
                messages = new object[]
                {
                    new {
                        role = "system",
                        content = new object[] {
                            new {
                                type = "text",
                                text = Constants.VideoSystemMessage,
                            }
                        }
                    },
                    new {
                        role = "user",
                        content = new object[] {
                            new {
                                type = "acv_document_id",
                                acv_document_id = docId
                            },
                            new {
                                type = "text",
                                text = message
                            }
                        }
                    }
                },
                max_tokens = 100,
            };

            var chatResponse = await httpClient.PostAsJsonAsync(
                chatCompletionsEndpoint, payload);

            if (chatResponse.IsSuccessStatusCode)
            {
                var response = await chatResponse.Content.ReadAsStringAsync();

                var aoaiResponse = await chatResponse
                    .Content.ReadFromJsonAsync<AOAI_Response>();

                var chatChoice = aoaiResponse.choices.FirstOrDefault();
                var answer = chatChoice.message.content + "\n";

                return answer;
            }
            else
            {
                return ($"Error: {chatResponse.StatusCode}, {chatResponse.ReasonPhrase}");
            }
        }


        public async Task CreateVideoIndex()
        {
            var visionIndexEndpoint = $"computervision/retrieval/indexes/{Constants.AzureComputerVisionIndex}?api-version={Constants.AzureComputerVisionApiVersion}";

            var payload = @" {
            'metadataSchema': {
                'fields': [
                    {
                        'name': 'cameraId',
                        'searchable': false,
                        'filterable': true,
                        'type': 'string'
                    },
                    {
                        'name': 'timestamp',
                        'searchable': false,
                        'filterable': true,
                        'type': 'datetime'
                    }
                ]
            },
            'features': [
                {
                    'name': 'vision',
                    'domain': 'surveillance'
                },
                {
                    'name': 'speech'
                }
            ]
        }";
            
            var content = new StringContent(payload);
            content.Headers.ContentType.MediaType = "application/json";

            var response = await visionClient.PutAsync(visionIndexEndpoint, content);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("Index created successfully.");
            else
                Console.WriteLine($"Failed to create index. Status code: {response.StatusCode}");

        }

        public async Task<string> IngestVideo(string videoUrl)
        {
            var ingestVideoEndpoint = $"computervision/retrieval/indexes/{Constants.AzureComputerVisionIndex}/ingestions/{Constants.AzureComputerVisionIngest}?api-version={Constants.AzureComputerVisionApiVersion}";

            var docId = Guid.NewGuid().ToString("N");

            var payload = $@"
        {{
            'videos': [
                {{
                    'mode': 'add',
                    'documentId': '{docId}',
                    'documentUrl': '{videoUrl}',
                    'metadata': {{
                        'cameraId': 'camera1',
                        'timestamp': '2023-06-30 17:40:33'
                    }}
                }}
            ]
        }}";

            var content = new StringContent(payload);
            content.Headers.ContentType.MediaType = "application/json";

            var response = await visionClient.PutAsync(ingestVideoEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Videos ingested successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to ingest videos. Status code: {response.StatusCode}");
            }

            return docId;
        }


    }
}
