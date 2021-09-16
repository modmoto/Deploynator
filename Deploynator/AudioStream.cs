using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace Deploynator
{
    public class AudioStream
    {
        private readonly SpeechSynthesizer _synthesizer;

        public AudioStream(EventBus eventBus)
        {
            eventBus.ReleasesTriggered += (_, args) => PlayReleases(args);
            eventBus.ServiceStarted += (_, _) => Play("Deployment ready, awaiting deployment sequence");
            eventBus.ReleaseFailed += (_, _) => Play("Release failed, leave the building immediatly");
            eventBus.ReleaseSuceeded += (_, _) => Play("Release suceeded, time to open that bottle of champagne");

            eventBus.SelectedDeloyment += (_, args) =>
            {
                Play($"{(args as SelectArgs)?.Name} selected for Deployment, get ready to fuck");
            };

            var config = SpeechConfig.FromSubscription("990a253fc3cb487e8f02867fcd3d86c2", "francecentral");
            config.SpeechSynthesisVoiceName = "en-US-SaraNeural";
            _synthesizer = new SpeechSynthesizer(config);
        }

        private async Task  PlayReleases(EventArgs args)
        {
            var deployArgs = args as DeployArgs;
            await Play($"Starting to deploy services: {string.Join(", ", deployArgs.SelectedDeloyments)}");
        }

        public async Task Play(string message)
        {
            await _synthesizer.SpeakTextAsync(message);
        }
    }
}